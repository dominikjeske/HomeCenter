using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace HomeCenter.SourceGenerators.Tests
{
    public static class AssertExtensions
    {
        public static void AssertSourceCodesEquals(this string expected, string actual)
        {
            Assert.Equal(expected.TrimWhiteSpaces(), actual.TrimWhiteSpaces());
        }

        public static void AssertNoErrorInDiagnostics(this ImmutableArray<Diagnostic> actual)
        {
            Assert.False(actual.Any(d => d.Severity == DiagnosticSeverity.Error));
        }
    }

}