using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using System.Threading.Tasks;
using Validation;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HomeCenter.CodeGeneration
{
    public class ProxyCodeGenerator : ICodeGenerator
    {
        private readonly AttributeData _attributeData;

        public ProxyCodeGenerator(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));
            _attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var results = List<MemberDeclarationSyntax>();

            var applyToClass = (ClassDeclarationSyntax)context.ProcessingNode;

            var proxy = new ProxyGenerator().GenerateProxy(applyToClass, context.SemanticModel);
            results = results.Add(proxy);

            return Task.FromResult(results);
        }
    }
}