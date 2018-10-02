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
        public ClassDeclarationSyntax GenerateProxy(ClassDeclarationSyntax classSyntax, SemanticModel model)
        {
            var classSemantic = model.GetDeclaredSymbol(classSyntax);
            var className = $"{classSemantic.Name}Proxy";

            try
            {
                var classDeclaration = ClassDeclaration(className);

                classDeclaration = classDeclaration.AddModifiers(Token(SyntaxKind.PublicKeyword))
                                                   .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(classSemantic.Name)))));

                var methodDeclaration = MethodDeclaration(ParseTypeName("Task"), "ReceiveAsync")
                                       .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword), Token(SyntaxKind.OverrideKeyword) }))
                                       .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("context")).WithType(QualifiedName(IdentifierName("Proto"), IdentifierName("IContext"))))))
                                       .WithBody(Block(
                                           GenerateSystemMessagesHandler(),
                                           LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("msg")).WithInitializer(EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("context"), IdentifierName("Message"))))))),
                                           FillContextData(),
                                           GenerateCommandHandlers(classSyntax, model),
                                           GenerateQueryHandlers(classSyntax, model),
                                           GetUnsupportedMessage())
                                        );

                classDeclaration = classDeclaration.AddMembers(methodDeclaration);

                //TODO select constructor
                var baseConstructor = classSemantic.Constructors.FirstOrDefault(p => p.Parameters.Length > 0);

                if (baseConstructor != null)
                {
                    ConstructorDeclarationSyntax constructorDecclaration = BenerateConstructor(className, baseConstructor);
                    classDeclaration = classDeclaration.AddMembers(constructorDecclaration);
                }

                var methods = GetMethodList(classSyntax, model, "Command", "Subscibe");

                if (methods.Count > 0)
                {
                    var baseClassInvoke = ExpressionStatement(AwaitExpression(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, BaseExpression(), IdentifierName("OnStarted"))).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName("context"))))), IdentifierName("ConfigureAwait"))).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression)))))));

                    List<StatementSyntax> statementSyntaxes = new List<StatementSyntax>();
                    statementSyntaxes.Add(baseClassInvoke);

                    foreach (var method in methods)
                    {
                        var registration = ExpressionStatement(InvocationExpression(GenericName(Identifier("SubscribeForCommand")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(method.Parameter.Type.Name))))));

                        statementSyntaxes.Add(registration);
                    }

                    var subscriptions = MethodDeclaration(IdentifierName("Task"), Identifier("OnStarted")).WithModifiers(TokenList(new[] { Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword), Token(SyntaxKind.AsyncKeyword) })).WithParameterList(ParameterList(SingletonSeparatedList<ParameterSyntax>(Parameter(Identifier("context")).WithType(QualifiedName(IdentifierName("Proto"), IdentifierName("IContext")))))).WithBody(Block(statementSyntaxes));

                    classDeclaration = classDeclaration.AddMembers(subscriptions);
                }

                return classDeclaration;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString(), "");
                return ClassDeclaration(className).WithCloseBraceToken(Token(TriviaList(Comment($"//{e}")), SyntaxKind.CloseBraceToken, TriviaList()));
            }
        }

        private static ConstructorDeclarationSyntax BenerateConstructor(string className, IMethodSymbol baseConstructor)
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

            var parameters = ParameterList(SeparatedList<ParameterSyntax>(parList.Take(parList.Count - 1)));
            var arguments = ArgumentList(SeparatedList<ArgumentSyntax>(baseList.Take(baseList.Count - 1)));

            var constructorDecclaration = ConstructorDeclaration(Identifier(className)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))).WithParameterList(parameters).WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, arguments)).WithBody(Block());
            return constructorDecclaration;
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
                var ifStatement = GetIfCommand(ref param_num, methods[0]);

                foreach (var method in methods.Skip(1))
                {
                    ifStatement = ifStatement.WithElse(ElseClause(GetIfCommand(ref param_num, method)));
                }

                return ifStatement;
            }

            return EmptyStatement();
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
                var ifStatement = GetIfQuery(ref param_num, methods[0]);

                foreach (var method in methods.Skip(1))
                {
                    ifStatement = ifStatement.WithElse(ElseClause(GetIfQuery(ref param_num, method)));
                }

                return ifStatement;
            }

            return EmptyStatement();
        }

        private IfStatementSyntax GetIfQuery(ref int param_num, MethodDescription method)
        {
            return IfStatement(IsPatternExpression(IdentifierName("msg"), DeclarationPattern(IdentifierName(method.Parameter.Type.Name), SingleVariableDesignation(Identifier($"{"query_"}{param_num}")))), GetQueryInvocationBody(method.Method.Identifier.ValueText, $"{"query_"}{param_num++}", method.ReturnType.Name));
        }

        private static List<MethodDescription> GetMethodList(ClassDeclarationSyntax classSyntax, SemanticModel model, string parameterType, string attributeType = null)
        {
            var filter = classSyntax.DescendantNodes()
                              .OfType<MethodDeclarationSyntax>()
                              .Where(m => m.ParameterList.Parameters.Count == 1);
            if (attributeType != null)
            {
                filter = filter.Where(m => m.AttributeLists.Any(a => a.Attributes.Any(x => x.Name.ToString() == attributeType)));
            }

            var result = filter.Select(c => new MethodDescription
            {
                Method = c,
                Parameter = model.GetDeclaredSymbol(c.ParameterList.Parameters.FirstOrDefault()),
                ReturnType = model.GetTypeInfo(c.ReturnType).Type
            })
                              .Where(x => x.Parameter.Type.BaseType?.Name == parameterType)
                              .ToList();

            return result;
        }

        private static StatementSyntax GetUnsupportedMessage()
        {
            return ExpressionStatement(AwaitExpression(InvocationExpression(IdentifierName("UnhandledCommand")).WithArgumentList(ArgumentList(SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName("context")))))));
        }

        private class MethodDescription
        {
            public MethodDeclarationSyntax Method { get; set; }
            public IParameterSymbol Parameter { get; set; }
            public ITypeSymbol ReturnType { get; set; }
        }
    }
}