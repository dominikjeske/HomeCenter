using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace HomeCenter.CodeGeneration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(TestBuilderCodeGenerator))]
    [Conditional("CodeGeneration")]
    public class TestBuilderAttribute : Attribute
    {
        public TestBuilderAttribute(Type builderContentType, string replaceFrom, string repleaceTo)
        {
            
        }

    }
}