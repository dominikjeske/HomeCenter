using System;

namespace HomeCenter.SourceGenerators
{
    internal static class ExceptionExtensions
    {
        public static string GenerateErrorSourceCode(this Exception exception) => $"//{exception.Message}";
    }

}