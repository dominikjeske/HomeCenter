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

    public class TestBuilerGenerator
    {
        private TransformationContext _context;
        private IEnumerable<IPropertySymbol> _properties;
        private IDictionary<string, string> _namesMap;
        private string className;
        private INamedTypeSymbol _builderContentType;

        public RichGenerationResult Generate(TransformationContext context, INamedTypeSymbol builderContentType, string from, string to)
        {
            _context = context;
            _builderContentType = builderContentType;

            var classSyntax = (ClassDeclarationSyntax)context.ProcessingNode;
            var semanticModel = context.SemanticModel;
            ClassDeclarationSyntax classDeclaration = null;
            
            try
            {
                _properties = builderContentType.GetMembers().Where(p => p.Kind == SymbolKind.Property && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>();
                GenerateNameMap(from, to);

                var classSemantic = semanticModel.GetDeclaredSymbol(classSyntax);
                className = $"{classSemantic.Name}";

            
                classDeclaration = GenerateClass(className);
                classDeclaration = AddMembers(classDeclaration);


                foreach(var property in _properties)
                {
                    var newMethod = AddWithMethod(_namesMap[property.Name], property.Type.Name);
                    classDeclaration = classDeclaration.AddMembers(newMethod);
                }

                classDeclaration = classDeclaration.AddMembers(AddBuildMethod());
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

        private void GenerateNameMap(string from, string to)
        {
            _namesMap = new Dictionary<string, string>();
            foreach (var property in _properties)
            {
                string destinationName = property.Name;
                if (!string.IsNullOrWhiteSpace(from))
                {
                    destinationName = destinationName.Replace(from, to);
                }
                _namesMap.Add(property.Name, destinationName);
            }
        }

        private ClassDeclarationSyntax GenerateClass(string className)
        {
            return ClassDeclaration(className).AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));
        }

        private ClassDeclarationSyntax AddMembers(ClassDeclarationSyntax classDeclarationSyntax)
        {
            List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>();

            foreach(IPropertySymbol property in _properties)
            {
                var field = FieldDeclaration(VariableDeclaration(IdentifierName(property.Type.Name)).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier($"_{_namesMap[property.Name]}"))))).WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
                members.Add(field);
            }

            classDeclarationSyntax = classDeclarationSyntax.WithMembers(List(members.ToArray()));

            return classDeclarationSyntax;
        }

        private NamespaceDeclarationSyntax AddNamespace(ClassDeclarationSyntax classSyntax, ClassDeclarationSyntax classDeclaration)
        {
            return NamespaceDeclaration((classSyntax.Parent as NamespaceDeclarationSyntax)?.Name).AddMembers(classDeclaration);
        }

        private MethodDeclarationSyntax AddWithMethod(string methodName, string inputType)
        {
            var parName = $"par{methodName}";

            return MethodDeclaration(IdentifierName(className), Identifier($"Wit{methodName}")).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))).WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(parName)).WithType(IdentifierName(inputType))))).WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName($"_{methodName}"), IdentifierName(parName))), ReturnStatement(ThisExpression())));
        }

        private MethodDeclarationSyntax AddBuildMethod()
        {
            var instanceBuilder = LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("instance")).WithInitializer(EqualsValueClause(ObjectCreationExpression(IdentifierName(_builderContentType.Name)).WithArgumentList(ArgumentList()))))));

            var assignments = new List<ExpressionStatementSyntax>();
            foreach(var property in _properties)
            {
                var propertyAssign = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("instance"), IdentifierName(property.Name)), IdentifierName($"_{_namesMap[property.Name]}")));
                assignments.Add(propertyAssign);
            }

            return MethodDeclaration(IdentifierName(_builderContentType.Name), Identifier("Build")).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))).WithBody(Block(instanceBuilder, Block(assignments), ReturnStatement(IdentifierName("instance"))));
        }
        
    }
}