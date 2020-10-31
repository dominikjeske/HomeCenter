using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace HomeCenter.SourceGenerators.Tests
{
    internal record GeneratorResult(Compilation Compilation, ImmutableArray<Diagnostic> Diagnostics, string generatedCode);
}