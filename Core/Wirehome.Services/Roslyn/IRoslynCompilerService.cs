using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Wirehome.Core.Services.Roslyn
{
    public interface IRoslynCompilerService
    {
        IEnumerable<Result<string>> CompileAssemblies(string sourceDictionary, bool generatePdb = false);
    }
}