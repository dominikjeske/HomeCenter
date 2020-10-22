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
    public class CommandBuilerGenerator
    {
        private TransformationContext _context;

        public RichGenerationResult Generate(TransformationContext context)
        {
            _context = context;
            var classSyntax = (ClassDeclarationSyntax)context.ProcessingNode;
            var model = context.SemanticModel;
            ClassDeclarationSyntax classDeclaration = null;

            var classSemantic = model.GetDeclaredSymbol(classSyntax);
            var className = $"{classSemantic.Name}";

            try
            {
                classDeclaration = GenerateClass(classSemantic, className);

                classDeclaration = AddPublish(classSyntax, model, classDeclaration, "PublishEvent", "Event");
                classDeclaration = AddBuildMethod(classSyntax, model, classDeclaration, "HomeCenter.Model.Messages.Commands.Command", "CreateCommand", "Command");
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

            return result;
        }

        private static ClassDeclarationSyntax GenerateClass(INamedTypeSymbol classSemantic, string className)
        {
            return ClassDeclaration(className).AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));
        }

        private static NamespaceDeclarationSyntax AddNamespace(ClassDeclarationSyntax classSyntax, ClassDeclarationSyntax classDeclaration)
        {
            return NamespaceDeclaration((classSyntax.Parent as NamespaceDeclarationSyntax)?.Name).AddMembers(classDeclaration);
        }

        private ClassDeclarationSyntax AddPublish(ClassDeclarationSyntax classSyntax, SemanticModel model, ClassDeclarationSyntax classDeclaration, string methodName, string typeName)
        {
            var methodDeclaration = MethodDeclaration(GenericName(Identifier("System.Threading.Tasks.Task")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("HomeCenter.Model.Messages.Events.Event")))), Identifier(methodName))
                                           .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword) }))
                                           .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[]
                                           {
                                                       Parameter(Identifier("source")).WithType(IdentifierName("ActorMessage")),
                                                       Token(SyntaxKind.CommaToken),
                                                       Parameter(Identifier("destination")).WithType(IdentifierName("ActorMessage")),
                                                       Token(SyntaxKind.CommaToken),
                                                       Parameter(Identifier("messageBroker")).WithType(IdentifierName("HomeCenter.Model.Core.IMessageBroker")),
                                                       Token(SyntaxKind.CommaToken),
                                                       Parameter(Identifier("routingFilter")).WithType(IdentifierName("HomeCenter.Broker.RoutingFilter"))
                                            })))
                                            .WithBody(Block(
                                                GenPublishIf(classSyntax, model, typeName),
                                                GenPublishLastReturn()
                                                )
                                            );

            classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            return classDeclaration;
        }

        private StatementSyntax GenPublishIf(ClassDeclarationSyntax classSyntax, SemanticModel model, string typeName)
        {
            var commands = model.Compilation
                                  .GetSymbolsWithName(x => x.IndexOf(typeName) > -1, SymbolFilter.Type)
                                  .OfType<INamedTypeSymbol>()
                                  .Where(y => y.BaseType.Name == typeName && !y.IsAbstract);

            var list = new List<IfStatementSyntax>();
            foreach (var command in commands)
            {
                list.Add(GenIfEvent(command.Name, command.ContainingNamespace.ToString()));
            }

            return GenerateIfStatement(list);
        }

        public StatementSyntax GenPublishLastReturn()
        {
            return Block(LocalDeclarationStatement(VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("ev")).WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("HomeCenter"), SyntaxFactory.IdentifierName("Model")), SyntaxFactory.IdentifierName("Messages")), SyntaxFactory.IdentifierName("Events")), SyntaxFactory.IdentifierName("Event"))).WithArgumentList(SyntaxFactory.ArgumentList())))))),
                SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("ev"), SyntaxFactory.IdentifierName("SetProperties"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("source")))))), SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("ev"), SyntaxFactory.IdentifierName("SetProperties"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("destination")))))),
                ExpressionStatement(AwaitExpression(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("messageBroker"), IdentifierName("Publish"))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] { Argument(IdentifierName("ev")), Token(SyntaxKind.CommaToken), Argument(IdentifierName("routingFilter")) }))))),
                ReturnStatement(IdentifierName("ev")));
        }

        private IfStatementSyntax GenIfEvent(string commandName, string commandNamespace)
        {
            return IfStatement(BinaryExpression(SyntaxKind.EqualsExpression, SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("destination"), SyntaxFactory.IdentifierName("Type")),
                LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(commandName))),
                Block(SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("@event")).WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(QualifiedName(IdentifierName(commandNamespace), IdentifierName(commandName))).WithArgumentList(SyntaxFactory.ArgumentList())))))),
                ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("@event"), SyntaxFactory.IdentifierName("SetProperties"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("source")))))),
                ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("@event"), SyntaxFactory.IdentifierName("SetProperties"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("destination")))))),
                ExpressionStatement(AwaitExpression(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("messageBroker"), IdentifierName("Publish"))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] { Argument(IdentifierName(@"@event")), Token(SyntaxKind.CommaToken), Argument(IdentifierName("routingFilter")) }))))),
                ReturnStatement(IdentifierName("@event"))));
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

        private ClassDeclarationSyntax AddBuildMethod(ClassDeclarationSyntax classSyntax, SemanticModel model, ClassDeclarationSyntax classDeclaration, string returnType, string methodName, string typeName)
        {
            var methodDeclaration = MethodDeclaration(ParseTypeName(returnType), methodName)
                                                   .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword) }))
                                                   .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(SyntaxFactory.Parameter(SyntaxFactory.Identifier("message")).WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))))))
                                                   .WithBody(Block(
                                                       GenerateIfMap(classSyntax, model, typeName),
                                                       GenerateLastReturn(returnType)
                                                      )
                                                    );

            classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            return classDeclaration;
        }

        private StatementSyntax GenerateIfMap(ClassDeclarationSyntax classSyntax, SemanticModel model, string typeName)
        {
            var commands = model.Compilation
                                  .GetSymbolsWithName(x => x.IndexOf(typeName) > -1, SymbolFilter.Type)
                                  .OfType<INamedTypeSymbol>()
                                  .Where(y => y.BaseType.Name == typeName && !y.IsAbstract);

            var list = new List<IfStatementSyntax>();
            foreach (var command in commands)
            {
                list.Add(GetIfCommand(command.Name, command.ContainingNamespace.ToString()));
            }

            return GenerateIfStatement(list);
        }

        public ReturnStatementSyntax GenerateLastReturn(string returnType)
        {
            return ReturnStatement(ObjectCreationExpression(SyntaxFactory.IdentifierName(returnType)).WithArgumentList(ArgumentList()));
        }

        private IfStatementSyntax GetIfCommand(string commandName, string commandNamespace)
        {
            return IfStatement(BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName("message"), LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(commandName))), Block(SingletonList<StatementSyntax>(ReturnStatement(ObjectCreationExpression(QualifiedName(IdentifierName(commandNamespace), IdentifierName(commandName))).WithArgumentList(ArgumentList())))));
        }
    }
}