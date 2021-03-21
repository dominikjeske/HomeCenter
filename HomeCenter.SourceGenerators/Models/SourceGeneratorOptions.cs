using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;

namespace HomeCenter.SourceGenerators
{
    internal class SourceGeneratorOptions<T> where T : ISourceGenerator
    {
        public bool EnableLogging { get; }
        public bool DetailedLogging { get; }
        public bool EnableDebug { get; }
        public string LogPath { get; } = string.Empty;
        public string IntermediateOutputPath { get; } = string.Empty;

        public List<AdditionalFilesOptions> AdditionalFilesOptions { get; } = new List<AdditionalFilesOptions>();

        public SourceGeneratorOptions(GeneratorExecutionContext context)
        {
            if (TryReadGlobalOption(context, "SourceGenerator_EnableLogging", out string? enableLogging)
                && bool.TryParse(enableLogging, out var enableLoggingValue))
            {
                EnableLogging = enableLoggingValue;
            }

            if (TryReadGlobalOption(context, "SourceGenerator_DetailedLog", out string? detailedLog)
                && bool.TryParse(detailedLog, out var detailedLogValue))
            {
                DetailedLogging = detailedLogValue;
            }

            if (TryReadGlobalOption(context, "SourceGenerator_LogPath", out string? logPath))
            {
                LogPath = logPath ?? string.Empty;
            }
            else
            {
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "obj");
                LogPath = Path.Combine(directory, $"{typeof(T).Name}.log");
            }

            if (TryReadGlobalOption(context, "SourceGenerator_EnableDebug", out string? enableDebug)
                && bool.TryParse(enableDebug, out var enableDebugValue))
            {
                EnableDebug = enableDebugValue;
            }

            if (TryReadGlobalOption(context, $"SourceGenerator_EnableDebug_{typeof(T).Name}", out string? debugThisGenerator)
                && bool.TryParse(debugThisGenerator, out var debugThisGeneratorValue))
            {
                EnableDebug = debugThisGeneratorValue;
            }

            if (TryReadGlobalOption(context, "IntermediateOutputPath", out var intermediate))
            {
                IntermediateOutputPath = intermediate ?? string.Empty;
            }

            foreach (var file in context.AdditionalFiles)
            {
                if (TryReadAdditionalFilesOption(context, file, "Type", out var type))
                {
                    AdditionalFilesOptions.Add(new SourceGenerators.AdditionalFilesOptions
                    {
                        Type = type,
                        AdditionalText = file
                    });
                }
            }
        }

        public bool TryReadGlobalOption(GeneratorExecutionContext context, string property, out string? value)
        {
            return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{property}", out value);
        }

        public bool TryReadAdditionalFilesOption(GeneratorExecutionContext context, AdditionalText additionalText, string property, out string? value)
        {
            return context.AnalyzerConfigOptions.GetOptions(additionalText).TryGetValue($"build_metadata.AdditionalFiles.{property}", out value);
        }
    }
}