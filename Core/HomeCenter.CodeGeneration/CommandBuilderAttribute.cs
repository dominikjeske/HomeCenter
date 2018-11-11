using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace HomeCenter.CodeGeneration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(CommandBuilderCodeGenerator))]
    [Conditional("CodeGeneration")]
    public class CommandBuilderAttribute : Attribute
    {
    }
}