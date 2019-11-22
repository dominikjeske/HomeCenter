using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace HomeCenter.CodeGeneration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(RoslymapperGenerator))]
    [Conditional("CodeGeneration")]
    public class AdapterAttribute : Attribute
    {
    }
}