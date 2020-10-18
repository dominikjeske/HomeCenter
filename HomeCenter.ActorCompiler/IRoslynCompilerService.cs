using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace HomeCenter.Services.Roslyn
{
    public interface IRoslynCompilerService
    {
        IEnumerable<Result<string>> CompileAssemblies(string sourceDictionary, bool generatePdb = false);
    }
}