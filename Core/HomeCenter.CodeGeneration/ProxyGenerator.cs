using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HomeCenter.CodeGeneration
{
    public class ProxyGenerator
    {
        private TransformationContext _context;
        private List<UsingDirectiveSyntax> _usingSyntax = new List<UsingDirectiveSyntax>();

        public RichGenerationResult Generate(TransformationContext context)
        {
            _context = context;
            var classSyntax = (ClassDeclarationSyntax)context.ProcessingNode;
            var model = context.SemanticModel;
            ClassDeclarationSyntax classDeclaration = null;

            var classSemantic = model.GetDeclaredSymbol(classSyntax);
            var className = $"{classSemantic.Name}Proxy";

            try
            {
                classDeclaration = GenerateClass(classSemantic, className);

                classDeclaration = AddReciveMapMethod(classSyntax, model, classDeclaration);

                classDeclaration = AddConstructor(classDeclaration, classSemantic, className);

                classDeclaration = AddSubscriptions(classSyntax, model, classDeclaration);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString(), "");
                classDeclaration = ClassDeclaration(className).WithCloseBraceToken(Token(TriviaList(Comment($"//{e}")), SyntaxKind.CloseBraceToken, TriviaList()));
            }

            var namespaveDeclaration = AddNamespace(classSyntax, classDeclaration);

            var result = new RichGenerationResult
            {
                Members = List<MemberDeclarationSyntax>().Add(namespaveDeclaration)
            };

            result.Usings = GenerateUsingStatements();

            return result;
        }

        private SyntaxList<UsingDirectiveSyntax> GenerateUsingStatements()
        {
            var usingSyntaxFinal = new List<UsingDirectiveSyntax>();
            
            _usingSyntax.Add(UsingDirective(IdentifierName("Quartz")));
            _usingSyntax.Add(UsingDirective(IdentifierName("System")));
            _usingSyntax.Add(UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Threading")), IdentifierName("Tasks"))));
            _usingSyntax.Add(UsingDirective(QualifiedName(QualifiedName(IdentifierName("HomeCenter"), IdentifierName("Model")), IdentifierName("Core"))));
            _usingSyntax.Add(UsingDirective(QualifiedName(QualifiedName(IdentifierName("Microsoft"), IdentifierName("Extensions")), IdentifierName("Logging"))));

            foreach (var usingSyntax in _usingSyntax)
            {
                // if using is not exists in current class or we already not added it to generated
                if (!(_context.CompilationUnitUsings?.Any(u => u.Name.ToString() == usingSyntax.Name.ToString()) ?? false) && !usingSyntaxFinal.Any(u => u.Name.ToString() == usingSyntax.Name.ToString()))
                {
                    usingSyntaxFinal.Add(usingSyntax);
                }
            }

            return List(usingSyntaxFinal);
        }

        private static NamespaceDeclarationSyntax AddNamespace(ClassDeclarationSyntax classSyntax, ClassDeclarationSyntax classDeclaration)
        {
            return NamespaceDeclaration((classSyntax.Parent as NamespaceDeclarationSyntax)?.Name).AddMembers(classDeclaration);
        }

        private static ClassDeclarationSyntax GenerateClass(INamedTypeSymbol classSemantic, string className)
        {
            return ClassDeclaration(className).AddModifiers(Token(SyntaxKind.PublicKeyword))
                                                                              .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(classSemantic.Name)))))
                                                                              .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("ProxyClass"))))));
        }

        private ClassDeclarationSyntax AddReciveMapMethod(ClassDeclarationSyntax classSyntax, SemanticModel model, ClassDeclarationSyntax classDeclaration)
        {
            var methodDeclaration = MethodDeclaration(ParseTypeName("Task"), "ReceiveAsyncInternal")
                                                   .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword), Token(SyntaxKind.OverrideKeyword) }))
                                                   .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("context")).WithType(QualifiedName(IdentifierName("Proto"), IdentifierName("IContext"))))))
                                                   .WithBody(Block(
                                                       GenerateSystemMessagesHandler(),
                                                       FormatMessage(),
                                                       FillContextData(),
                                                       GenerateCommandHandlers(classSyntax, model),
                                                       GenerateQueryHandlers(classSyntax, model),
                                                       GenerateEventdHandlers(classSyntax, model),
                                                       GetUnsupportedMessage())
                                                    );

            classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            return classDeclaration;
        }

        private static LocalDeclarationStatementSyntax FormatMessage()
        {
            return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(Identifier("msg")).WithInitializer(EqualsValueClause(InvocationExpression(IdentifierName("FormatMessage")).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("context"), IdentifierName("Message")))))))))));
        }

        private ClassDeclarationSyntax AddSubscriptions(ClassDeclarationSyntax classSyntax, SemanticModel model, ClassDeclarationSyntax classDeclaration)
        {
            var commands = GetMethodList(classSyntax, model, "Command", "Subscribe");
            var events = GetMethodList(classSyntax, model, "Event", "Subscribe");
            var queries = GetMethodList(classSyntax, model, "Query", "Subscribe");
            
            if(commands.Count + queries.Count + events.Count > 0)
            {
                var baseClassInvoke = ExpressionStatement(AwaitExpression(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, BaseExpression(), IdentifierName("OnStarted"))).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName("context"))))), IdentifierName("ConfigureAwait"))).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression)))))));

                List<StatementSyntax> statementSyntaxes = new List<StatementSyntax>
                {
                    baseClassInvoke
                };

                foreach (var method in commands)
                {
                    var registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("Subscribe")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(method.Parameter.Type.Name))))));

                    statementSyntaxes.Add(registration);
                }

                foreach (var method in events)
                {
                    var registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("Subscribe")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(method.Parameter.Type.Name))))));

                    statementSyntaxes.Add(registration);
                }

                foreach (var method in queries)
                {
                    ExpressionStatementSyntax registration;
                    if (method.ReturnType is INamedTypeSymbol namedSymbol && namedSymbol.TypeArguments.Length > 0)
                    {
                        var arg = namedSymbol.TypeArguments[0];

                        registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("Subscribe")).WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { IdentifierName(method.Parameter.Type.Name), Token(SyntaxKind.CommaToken), IdentifierName(arg.Name) })))));
                    }
                    else if(!string.IsNullOrWhiteSpace(method.ReturnType.Name))
                    {
                        registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("Subscribe")).WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { IdentifierName(method.Parameter.Type.Name), Token(SyntaxKind.CommaToken), IdentifierName(method.ReturnType.Name) })))));
                    }
                    //if (method.ReturnType is IArrayTypeSymbol arraySymbol)
                    //{
                    //    registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("Subscribe")).WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { IdentifierName(method.Parameter.Type.Name), Token(SyntaxKind.CommaToken), IdentifierName(method.ReturnType.Name) })))));
                    //}
                    else
                    {
                        registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("Subscribe")).WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { IdentifierName(method.Parameter.Type.Name), Token(SyntaxKind.CommaToken), IdentifierName(method.ReturnType.ToString()) })))));
                    }
                    //


                    statementSyntaxes.Add(registration);
                }

                var subscriptions = MethodDeclaration(IdentifierName("Task"), Identifier("OnStarted")).WithModifiers(TokenList(new[] { Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword), Token(SyntaxKind.AsyncKeyword) })).WithParameterList(ParameterList(SingletonSeparatedList<ParameterSyntax>(Parameter(Identifier("context")).WithType(QualifiedName(IdentifierName("Proto"), IdentifierName("IContext")))))).WithBody(Block(statementSyntaxes));

                classDeclaration = classDeclaration.AddMembers(subscriptions);

            }

            return classDeclaration;
        }

        private ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol classSemantic, string className)
        {
            IMethodSymbol baseConstructor;
            if (classSemantic.Constructors.Length == 1)
            {
                baseConstructor = classSemantic.Constructors.FirstOrDefault();
            }
            else if (classSemantic.Constructors.Count(p => p.Parameters.Length > 0) == 1)
            {
                baseConstructor = classSemantic.Constructors.FirstOrDefault(p => p.Parameters.Length > 0);
            }
            else
            {
                throw new Exception("Unsupported constructor count");
            }

            var constructorDecclaration = GenerateConstructor(className, baseConstructor);
            classDeclaration = classDeclaration.AddMembers(constructorDecclaration);

            return classDeclaration;
        }

        private ConstructorDeclarationSyntax GenerateConstructor(string className, IMethodSymbol baseConstructor)
        {
            var parList = new List<SyntaxNodeOrToken>();
            var baseList = new List<SyntaxNodeOrToken>();
            foreach (var par in baseConstructor.Parameters)
            {
                var parType = par.Type as INamedTypeSymbol;

                if (parType.IsGenericType)
                {
                    var argumentList = new List<SyntaxNodeOrToken>();
                    foreach (var arg in parType.TypeArguments)
                    {
                        argumentList.Add(IdentifierName(arg.Name));
                        argumentList.Add(Token(SyntaxKind.CommaToken));
                    }

                    parList.Add(Parameter(Identifier(par.Name)).WithType(GenericName(Identifier(par.Type.Name)).WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(argumentList.Take(argumentList.Count - 1))))));
                }
                else
                {
                    parList.Add(Parameter(Identifier(par.Name)).WithType(IdentifierName(par.Type.Name)));
                }
                parList.Add(Token(SyntaxKind.CommaToken));

                baseList.Add(Argument(IdentifierName(par.Name)));
                baseList.Add(Token(SyntaxKind.CommaToken));
            }

            var constructorDecclaration = ConstructorDeclaration(Identifier(className)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            var parameters = ParameterList(SeparatedList<ParameterSyntax>(parList.Take(parList.Count - 1)));

            // TODO add this basing on DI attribute
            if (!CheckIfExists(parList, "IScheduler"))
            {
                parameters = parameters.AddParameters(Parameter(Identifier("scheduler")).WithType(IdentifierName("IScheduler")));
            }
            if (!CheckIfExists(parList, "IMessageBroker"))
            {
                parameters = parameters.AddParameters(Parameter(Identifier("messageBroker")).WithType(IdentifierName("IMessageBroker")));
            }
            if (!CheckIfExists(parList, "ILogger"))
            {
                parameters = parameters.AddParameters(Parameter(Identifier("logger")).WithType(GenericName(Identifier("ILogger")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(className))))));
                
            }

            constructorDecclaration = constructorDecclaration.WithParameterList(parameters);

            if (baseList.Count > 0)
            {
                var arguments = ArgumentList(SeparatedList<ArgumentSyntax>(baseList.Take(baseList.Count - 1)));
                constructorDecclaration = constructorDecclaration.WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, arguments));
            }

            var logger = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("Logger"), IdentifierName("logger")));
            var broker = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("MessageBroker"), IdentifierName("messageBroker")));
            var scheduler = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("Scheduler"), IdentifierName("scheduler")));

            constructorDecclaration = constructorDecclaration.WithBody(Block(logger, broker, scheduler));
            return constructorDecclaration;
        }

        private static bool CheckIfExists(List<SyntaxNodeOrToken> parList, string parameter)
        {
            return parList.Any(x => x.ToString().IndexOf(parameter) > -1);
        }

        public StatementSyntax GenerateSystemMessagesHandler()
        {
            return IfStatement(AwaitExpression(InvocationExpression(IdentifierName("HandleSystemMessages")).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("context")))))), ReturnStatement());
        }

        public StatementSyntax FillContextData()
        {
            return IfStatement(IsPatternExpression(IdentifierName("msg"), DeclarationPattern(IdentifierName("HomeCenter.Model.Messages.ActorMessage"), SingleVariableDesignation(Identifier("ic")))), Block(SingletonList<StatementSyntax>(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("ic"), IdentifierName("Context")), IdentifierName("context"))))));
        }

        private StatementSyntax GetQueryInvocationBody(string handlerName, string paramName, string returnType)
        {
            LocalDeclarationStatementSyntax varibleExpression;

            if (returnType == "Task")
            {
                varibleExpression = LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("result")).WithInitializer(EqualsValueClause(AwaitExpression(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, InvocationExpression(IdentifierName(handlerName)).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(paramName))))), IdentifierName("ConfigureAwait"))).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression)))))))))));
            }
            else
            {
                varibleExpression = LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(Identifier("result")).WithInitializer(EqualsValueClause(InvocationExpression(IdentifierName(handlerName)).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(paramName))))))))));
            }

            return Block(varibleExpression, ExpressionStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("context"), IdentifierName("Respond"))).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("result")))))), ReturnStatement());
        }

        private StatementSyntax GetCommandInvocationBody(string handlerName, string paramName, string returnType)
        {
            ExpressionStatementSyntax varibleExpression;

            if (returnType == "Void")
            {
                varibleExpression = ExpressionStatement(InvocationExpression(IdentifierName(handlerName)).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName(paramName))))));
            }
            else
            {
                varibleExpression = ExpressionStatement(AwaitExpression(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, InvocationExpression(IdentifierName(handlerName)).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(paramName))))), IdentifierName("ConfigureAwait"))).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression)))))));
            }

            return Block(varibleExpression, ReturnStatement());
        }

        private StatementSyntax GenerateCommandHandlers(ClassDeclarationSyntax classSyntax, SemanticModel model)
        {
            int param_num = 0;

            var methods = GetMethodList(classSyntax, model, "Command");

            if (methods.Count > 0)
            {
                var list = new List<IfStatementSyntax>();
                foreach (var method in methods)
                {
                    list.Add(GetIfCommand(ref param_num, method));
                }

                return GenerateIfStatement(list);
            }

            return EmptyStatement();
        }

        private StatementSyntax GenerateEventdHandlers(ClassDeclarationSyntax classSyntax, SemanticModel model)
        {
            int param_num = 0;

            var methods = GetMethodList(classSyntax, model, "Event");

            if (methods.Count > 0)
            {
                var list = new List<IfStatementSyntax>();
                foreach (var method in methods)
                {
                    list.Add(GetIfEvent(ref param_num, method));
                }

                return GenerateIfStatement(list);
            }

            return EmptyStatement();
        }

        private IfStatementSyntax GetIfEvent(ref int param_num, MethodDescription method)
        {
            return IfStatement(IsPatternExpression(IdentifierName("msg"), DeclarationPattern(IdentifierName(method.Parameter.Type.Name), SingleVariableDesignation(Identifier($"{"event_"}{param_num}")))), GetCommandInvocationBody(method.Method.Identifier.ValueText, $"{"event_"}{param_num++}", method.ReturnType.Name));
        }

        private static IfStatementSyntax GenerateIfStatement(List<IfStatementSyntax> list)
        {
            if (list.Count == 1) return list[0];

            IfStatementSyntax ifStatment = list[list.Count - 2].WithElse(ElseClause(list[list.Count - 1]));
            var text = ifStatment.NormalizeWhitespace().ToFullString();
            for (int i = list.Count - 3; i >= 0; i--)
            {
                ifStatment = list[i].WithElse(ElseClause(ifStatment));
            }

            return ifStatment;
        }

        private IfStatementSyntax GetIfCommand(ref int param_num, MethodDescription method)
        {
            return IfStatement(IsPatternExpression(IdentifierName("msg"), DeclarationPattern(IdentifierName(method.Parameter.Type.Name), SingleVariableDesignation(Identifier($"{"command_"}{param_num}")))), GetCommandInvocationBody(method.Method.Identifier.ValueText, $"{"command_"}{param_num++}", method.ReturnType.Name));
        }

        private StatementSyntax GenerateQueryHandlers(ClassDeclarationSyntax classSyntax, SemanticModel model)
        {
            int param_num = 0;
            var methods = GetMethodList(classSyntax, model, "Query");

            if (methods.Count > 0)
            {
                var list = new List<IfStatementSyntax>();
                foreach (var method in methods)
                {
                    list.Add(GetIfQuery(ref param_num, method));
                }

                return GenerateIfStatement(list);
            }

            return EmptyStatement();
        }

        private IfStatementSyntax GetIfQuery(ref int param_num, MethodDescription method)
        {
            return IfStatement(IsPatternExpression(IdentifierName("msg"), DeclarationPattern(IdentifierName(method.Parameter.Type.Name), SingleVariableDesignation(Identifier($"{"query_"}{param_num}")))), GetQueryInvocationBody(method.Method.Identifier.ValueText, $"{"query_"}{param_num++}", method.ReturnType.Name));
        }

        private List<MethodDescription> GetMethodList(ClassDeclarationSyntax classSyntax, SemanticModel model, string parameterType, string attributeType = null)
        {
            var result = GetMethodListInner(classSyntax, model, parameterType, attributeType);

            if (_context.Compilation != null && model.GetDeclaredSymbol(classSyntax)?.BaseType?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is ClassDeclarationSyntax subClassSyntax)
            {
                var semanticModel = _context.Compilation.GetSemanticModel(subClassSyntax.SyntaxTree);

                // add usings from base class
                _usingSyntax.AddRange(subClassSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>());

                var sub = GetMethodListInner(subClassSyntax, semanticModel, parameterType, attributeType);
                result.AddRange(sub);
            }

            return result;
        }

        private List<MethodDescription> GetMethodListInner(ClassDeclarationSyntax classSyntax, SemanticModel model, string parameterType, string attributeType)
        {
            var filter = classSyntax.DescendantNodes()
                                .OfType<MethodDeclarationSyntax>()
                                .Where(m => m.ParameterList.Parameters.Count == 1 && !m.Modifiers.Any(x => x.ValueText == "private"));
            
            if (attributeType != null)
            {
                filter = filter.Where(m => m.AttributeLists.Any(a => a.Attributes.Any(x => x.Name.ToString() == attributeType)));
            }

           
            var result = filter.Select(c => new MethodDescription
            {
                Method = c,
                Parameter = model.GetDeclaredSymbol(c.ParameterList.Parameters.FirstOrDefault()),
                ReturnType = model.GetTypeInfo(c.ReturnType).Type
                // TODO write recursive base type check
            }).Where(x => x.Parameter.Type.BaseType?.Name == parameterType || x.Parameter.Type.BaseType?.BaseType?.Name == parameterType || x.Parameter.Type.Name == parameterType).ToList();

            return result;
        }

        private static StatementSyntax GetUnsupportedMessage()
        {
            return ExpressionStatement(AwaitExpression(InvocationExpression(IdentifierName("UnhandledMessage")).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName("msg")))))));
        }

        private class MethodDescription
        {
            public MethodDeclarationSyntax Method { get; set; }
            public IParameterSymbol Parameter { get; set; }
            public ITypeSymbol ReturnType { get; set; }
        }
    }

}