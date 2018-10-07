using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using System.Threading.Tasks;
using Validation;

namespace HomeCenter.CodeGeneration
{
    public class ProxyCodeGenerator : IRichCodeGenerator
    {
        private readonly AttributeData _attributeData;

        public ProxyCodeGenerator(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));
            _attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var proxy = new ProxyGenerator().Generate(context);
            return Task.FromResult(proxy);
        }
    }
}