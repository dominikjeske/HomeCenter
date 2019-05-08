using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.CodeGeneration
{
    public class TestBuilderCodeGenerator : IRichCodeGenerator
    {
        private readonly AttributeData _attributeData;

        public TestBuilderCodeGenerator(AttributeData attributeData)
        {
            _attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var par = _attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            var from = _attributeData.ConstructorArguments[1].Value as string;
            var to = _attributeData.ConstructorArguments[2].Value as string;

            var proxy = new TestBuilerGenerator().Generate(context, par, from, to);
            return Task.FromResult(proxy);
        }
    }
}