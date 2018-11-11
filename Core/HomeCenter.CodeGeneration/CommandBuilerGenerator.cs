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
        private List<UsingDirectiveSyntax> _usingSyntax = new List<UsingDirectiveSyntax>();

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

                classDeclaration = AddBuildMethod(classSyntax, model, classDeclaration);

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

           // result.Usings = GenerateUsingStatements();

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

        private ClassDeclarationSyntax AddBuildMethod(ClassDeclarationSyntax classSyntax, SemanticModel model, ClassDeclarationSyntax classDeclaration)
        {
            var methodDeclaration = MethodDeclaration(ParseTypeName("Command"), "CreateCommand")
                                                   .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword) }))
                                                   .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("command")).WithType(QualifiedName(IdentifierName("HomeCenter.Messages"), IdentifierName("ProtoCommand"))))))
                                                   .WithBody(Block(
                                                       GenerateIfMap(classSyntax, model),
                                                       GenerateLastReturn()
                                                      )
                                                    );

            classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            return classDeclaration;
        }

        private StatementSyntax GenerateIfMap(ClassDeclarationSyntax classSyntax, SemanticModel model)
        {
            var commands = model.Compilation
                                  .GetSymbolsWithName(x => x.IndexOf("Command") > -1, SymbolFilter.Type)
                                  .OfType<INamedTypeSymbol>()
                                  .Where(y => y.BaseType.Name == "Command");


            var list = new List<IfStatementSyntax>();
            foreach (var command in commands)
            {
                list.Add(GetIfCommand(command.Name, command.ContainingNamespace.ToString()));
            }

            return GenerateIfStatement(list);

        }

        public ReturnStatementSyntax GenerateLastReturn()
        {
            return ReturnStatement(ObjectCreationExpression(QualifiedName(IdentifierName("HomeCenter.Model.Messages.Commands"), IdentifierName("Command"))).WithArgumentList(ArgumentList()));
        }


        private IfStatementSyntax GetIfCommand(string commandName, string commandNamespace)
        {
            return IfStatement(BinaryExpression(SyntaxKind.EqualsExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("command"), IdentifierName("Type")), LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(commandName))), Block(SingletonList<StatementSyntax>(ReturnStatement(ObjectCreationExpression(QualifiedName(IdentifierName(commandNamespace), IdentifierName(commandName))).WithArgumentList(ArgumentList())))));
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
    }

}