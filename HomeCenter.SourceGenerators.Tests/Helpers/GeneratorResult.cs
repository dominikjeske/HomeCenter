using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace HomeCenter.SourceGenerators.Tests
{
    internal class GeneratorResult
    {
        public GeneratorResult(Compilation Compilation, ImmutableArray<Diagnostic> Diagnostics, string? generatedCode)
        {
            if (generatedCode is null) throw new ArgumentNullException(nameof(generatedCode));

            this.Compilation = Compilation;
            this.Diagnostics = Diagnostics;
            GeneratedCode = generatedCode;
        }

        public Compilation Compilation { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public string GeneratedCode { get; }
    }
}