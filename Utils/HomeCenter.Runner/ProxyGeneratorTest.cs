using CodeGeneration.Roslyn;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Runner.Codegen;
using HomeCenter.Utils.ConsoleExtentions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class ProxyGeneratorTest
    {
        public async Task<string> Generate(string code)
        {
            var generator = new CommandBuilerGenerator();
            var models = await GetModels(code);

            var syntaxTree = models.syntaxTree;
            var semanticModel = models.semanticModel;
            var classToDecorate = models.syntaxTree.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

            if (classToDecorate.Count == 0) return "No class found to decorate";

            NamespaceDeclarationSyntax oldNamespace = classToDecorate[0].Parent as NamespaceDeclarationSyntax;
            NamespaceDeclarationSyntax newNamespace = oldNamespace;
            var classList = new List<MemberDeclarationSyntax>();

            foreach (var classModel in classToDecorate)
            {
                var classSemantic = semanticModel.GetDeclaredSymbol(classModel);


                if (HasBaseType(classSemantic, "DeviceActor"))
                {
                   // ExternAliasDirectiveSyntax - Represents an ExternAlias directive syntax, e.g. "extern alias MyAlias;" with specifying "/r:MyAlias=SomeAssembly.dll " on the compiler command line.

                   var proxy = new TransformationContext(classModel, semanticModel, models.compilation, "", null, null);
                    var result = generator.Generate(proxy);

                    foreach (var res in result.Members)
                    {
                        ConsoleEx.WriteOKLine($"{classSemantic.Name}:");
                        ConsoleEx.Write($"{res.NormalizeWhitespace().ToFullString()}");

                        classList.Add(res);
                    }
                }
            }


            foreach (var proxyClass in classList)
            {
                newNamespace = newNamespace.AddMembers(proxyClass);
            }

            syntaxTree = syntaxTree.ReplaceNode(oldNamespace, newNamespace);

            Veryfy(syntaxTree);

            return syntaxTree.NormalizeWhitespace().ToFullString();
        }

        private bool HasBaseType(INamedTypeSymbol type, string baseType)
        {
            if (type.BaseType == null) return false;
            if (type.BaseType.Name == baseType) return true;

            return HasBaseType(type.BaseType, baseType);
        }

        

        private static void Veryfy(CompilationUnitSyntax syntaxTree)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var tasklib = MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);
            var runtime = MetadataReference.CreateFromFile(typeof(FileAttributes).GetTypeInfo().Assembly.Location);
            var console = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
            var generator = MetadataReference.CreateFromFile(typeof(ProxyGenerator).Assembly.Location);
            var model = MetadataReference.CreateFromFile(typeof(Query).Assembly.Location);
            var eventAggregator = MetadataReference.CreateFromFile(typeof(IEventAggregator).Assembly.Location);
            var logger = MetadataReference.CreateFromFile(typeof(ILogger).Assembly.Location);

            var netStandard = MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.0.0\ref\netcoreapp2.0\netstandard.dll");

            var externalRefs = new Assembly[] { typeof(IContext).Assembly, typeof(Proto.Mailbox.UnboundedMailbox).Assembly, typeof(Router).Assembly };
            var external = externalRefs.Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();

            var comp = CSharpCompilation.Create("Final").WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                                        .AddSyntaxTrees(syntaxTree.SyntaxTree)
                                                        .AddReferences(mscorlib, tasklib, netStandard, runtime, console, generator, model, eventAggregator, logger)
                                                        .AddReferences(external);

            var result = comp.Emit("final.dll");

            if (result.Success)
            {
                ConsoleEx.WriteOK("Success");
            }
            else
            {
                foreach (var error in result.Diagnostics)
                {
                    ConsoleEx.WriteError(error.ToString());
                }
            }
        }

        private async Task<(CompilationUnitSyntax syntaxTree, SemanticModel semanticModel, CSharpCompilation compilation)> GetModels(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var syntaxTree = await tree.GetRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var model = MetadataReference.CreateFromFile(typeof(Model.Components.Component).Assembly.Location);
            var comp = CSharpCompilation.Create("Demo").AddSyntaxTrees(tree).AddReferences(mscorlib, model);

            var semanticModel = comp.GetSemanticModel(tree);

            return (syntaxTree, semanticModel, comp);
        }
    }
}