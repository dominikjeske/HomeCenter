using System;

namespace HomeCenter.SourceGenerators
{
    internal class GeneratedSource
    {
        public GeneratedSource(string sourceCode, string? fileName)
        {
            if (sourceCode is null) throw new ArgumentNullException(nameof(sourceCode));
            if (fileName is null) throw new ArgumentNullException(nameof(fileName));

            SourceCode = sourceCode;
            FileName = fileName;
        }

        public string SourceCode { get; }
        public string FileName { get; }
    }
}