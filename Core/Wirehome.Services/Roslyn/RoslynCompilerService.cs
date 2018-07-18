using CSharpFunctionalExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Wirehome.ComponentModel.Adapters;
using Wirehome.Core.ComponentModel.Configuration;
using Wirehome.Core.Extensions;
using Wirehome.Core.Utils;
using Wirehome.Model.Core;
using Wirehome.Model.Extensions;

namespace Wirehome.Core.Services.Roslyn
{
    public class RoslynCompilerService : IRoslynCompilerService
    {
        private const string AdapterInfoFileName = "AdapterInfo.json";
        private const string CommonAdapterDirectory = "Common";

        public IEnumerable<Result<string>> CompileAssemblies(string sourceDictionary, bool generatePdb = false)
        {
            var assemblies = new List<Result<string>>();
            var modelAssemblies = AssemblyHelper.GetReferencedAssemblies(typeof(Adapter));
            var servicesAssemblies = AssemblyHelper.GetReferencedAssemblies(typeof(WirehomeController));
            var references = modelAssemblies.Union(servicesAssemblies).Distinct();
            var result = new List<Result<string>>();

            foreach (string adapterDictionary in Directory.GetDirectories(sourceDictionary))
            {
                var adapterInfoPath = Path.Combine(adapterDictionary, AdapterInfoFileName);
                if (!File.Exists(adapterInfoPath)) continue;

                var adapterDescription = JsonConvert.DeserializeObject<AdapterInfoDTO>(File.ReadAllText(adapterInfoPath));
                var adapterAssembly = Path.Combine(adapterDictionary, $"{adapterDescription.Name}.dll");
                AssemblyName currentAssembly = null;
                if (File.Exists(adapterAssembly))
                {
                    currentAssembly = AssemblyName.GetAssemblyName(adapterAssembly);
                }

                if (currentAssembly?.Version.Differ(adapterDescription.Version) ?? true)
                {
                    result.Add(GenerateAssembly(adapterDescription.Name, adapterDescription.Version, adapterDictionary, references,
                                                adapterDescription.CommonReferences, generatePdb)
                              );
                }
                else
                {
                    result.Add(Result.Ok(adapterAssembly));
                }
            }

            return result;
        }

        private SyntaxTree GenerateAssemblyVersion(string title, Version version)
        {
            StringBuilder asmInfo = new StringBuilder();

            asmInfo.AppendLine("using System.Reflection;");
            asmInfo.AppendLine($"[assembly: AssemblyTitle(\"{title}\")]");
            asmInfo.AppendLine($"[assembly: AssemblyVersion(\"{version.Major}.{version.Minor}.{version.Build}.0\")]");
            asmInfo.AppendLine($"[assembly: AssemblyFileVersion(\"{version.Major}.{version.Minor}.{version.Build}.0\")]");
            asmInfo.AppendLine("[assembly: AssemblyProduct(\"Wirehome\")]");
            asmInfo.AppendLine($"[assembly: AssemblyInformationalVersion(\"{version.Major}.{version.Minor}.{version.Build}.0\")]");

            return CSharpSyntaxTree.ParseText(asmInfo.ToString(), encoding: Encoding.Default);
        }

        private Result<string> GenerateAssembly(string adapterName, Version assemblyVersion, string sourceDictionary,
                                                IEnumerable<string> dependencies, IEnumerable<string> commons, bool generatePdb = false)
        {
            var syntaxTrees = ParseSourceCode(sourceDictionary, commons).ToList();
            syntaxTrees.Add(GenerateAssemblyVersion(adapterName, assemblyVersion));
            var references = ParseDependencies(dependencies);
            var assemblyName = $"{adapterName}.dll";

            var compilation = CSharpCompilation.Create(assemblyName)
                                               .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                               .AddReferences(references)
                                               .AddSyntaxTrees(syntaxTrees);

            var path = Path.Combine(sourceDictionary, assemblyName);
            var pdbPath = generatePdb ? Path.Combine(sourceDictionary, $"{adapterName}.pdb") : null;

            var compilationResult = compilation.Emit(path, pdbPath: pdbPath);

            if (!compilationResult.Success && File.Exists(path))
            {
                File.Delete(path);
            }

            return compilationResult.Success ? Result.Ok(path) : Result.Fail<string>(ReadCompilationErrors(compilationResult));
        }

        private string ReadCompilationErrors(Microsoft.CodeAnalysis.Emit.EmitResult compilationResult)
        {
            var sb = new StringBuilder();
            foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
            {
                sb.AppendLine($"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: {codeIssue.Location.GetLineSpan().ToString()}, Severity: {codeIssue.Severity.ToString()}");
            }
            return sb.ToString();
        }

        private IEnumerable<PortableExecutableReference> ParseDependencies(IEnumerable<string> dependencies)
        {
            var references = new List<PortableExecutableReference>
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),                      //System.Private.CoreLib.dll
                MetadataReference.CreateFromFile(typeof(FileAttributes).GetTypeInfo().Assembly.Location),              //System.Runtime.dll
                MetadataReference.CreateFromFile(typeof(NetworkCredential).GetTypeInfo().Assembly.Location),           //System.Net.Primitives.dll
                MetadataReference.CreateFromFile(typeof(SecureStringMarshal).GetTypeInfo().Assembly.Location),         //System.Runtime.InteropServices.dll
                MetadataReference.CreateFromFile(typeof(System.Collections.BitArray).GetTypeInfo().Assembly.Location), //System.Collections.dll
                MetadataReference.CreateFromFile(typeof(StringReader).GetTypeInfo().Assembly.Location),                //System.Runtime.Extensions.dll
                MetadataReference.CreateFromFile(typeof(XDocument).GetTypeInfo().Assembly.Location),                   //System.Private.Xml.Linq.dll
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),                  //System.Linq.dll
                MetadataReference.CreateFromFile(typeof(XmlReader).GetTypeInfo().Assembly.Location),                   //System.Private.Xml.dll
                MetadataReference.CreateFromFile(typeof(Uri).GetTypeInfo().Assembly.Location)                          //System.Private.Uri.dll
            };

            dependencies.ForEach(dep => references.Add(MetadataReference.CreateFromFile(dep)));
            return references;
        }

        private IEnumerable<SyntaxTree> ParseSourceCode(string sourceDir, IEnumerable<string> commons, string filter = "*.cs")
        {
            var sources = Directory.GetFiles(sourceDir, filter, SearchOption.AllDirectories)
                                   .Select(file => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(file)))
                                   .ToList();

            foreach (var common in commons)
            {
                var commonPath = Path.Combine(Path.Combine(Path.GetDirectoryName(sourceDir), CommonAdapterDirectory), common);
                sources.Add(SyntaxFactory.ParseSyntaxTree(File.ReadAllText(commonPath)));
            }

            return sources;
        }
    }
}