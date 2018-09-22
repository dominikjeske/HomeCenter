using HomeCenter.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Proto;
using Proto.Router;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{

    public class ProxyGeneratorTest
    {
        public async Task<string> Generate(string code, Assembly[] externalReferences)
        {
            var generator = new ProxyGenerator();
            var models = await GetModels(code);

            var syntaxTree = models.syntaxTree;
            var semanticModel = models.semanticModel;
            var classToDecorate = models.syntaxTree.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

            if (classToDecorate.Count == 0) return "No class found to decorate";

            NamespaceDeclarationSyntax oldNamespace = classToDecorate[0].Parent as NamespaceDeclarationSyntax;
            NamespaceDeclarationSyntax newNamespace = oldNamespace;
            List<ClassDeclarationSyntax> classList = new List<ClassDeclarationSyntax>();

            foreach (var classModel in classToDecorate)
            {
                var classSemantic = semanticModel.GetDeclaredSymbol(classModel);
                if (classSemantic.BaseType.Name == "Actor")
                {
                    var proxyClass = generator.GenerateProxy(classModel, semanticModel);

                    ConsoleWriter.WriteOK($"{classSemantic.Name}:");
                    ConsoleWriter.Write($"{proxyClass.NormalizeWhitespace().ToFullString()}");

                    classList.Add(proxyClass);
                }
            }

            foreach (var proxyClass in classList)
            {
                newNamespace = newNamespace.AddMembers(proxyClass);
            }

            syntaxTree = syntaxTree.ReplaceNode(oldNamespace, newNamespace);

            Veryfy(syntaxTree, externalReferences);

            return syntaxTree.NormalizeWhitespace().ToFullString();
        }

        private static void Veryfy(CompilationUnitSyntax syntaxTree, Assembly[] externalReferences)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var tasklib = MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);
            var runtime = MetadataReference.CreateFromFile(typeof(FileAttributes).GetTypeInfo().Assembly.Location);
            var console = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
            var self = MetadataReference.CreateFromFile(typeof(ProxyGeneratorTest).Assembly.Location);

            var netStandard = MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.0.0\ref\netcoreapp2.0\netstandard.dll");

            var external = externalReferences.Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();

            var comp = CSharpCompilation.Create("Final").WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                                        .AddSyntaxTrees(syntaxTree.SyntaxTree)
                                                        .AddReferences(mscorlib, tasklib, netStandard, runtime, console, self)
                                                        .AddReferences(external);

            var result = comp.Emit("final.dll");

            if (result.Success)
            {
                ConsoleWriter.WriteOK("Success");
            }
            else
            {
                foreach (var error in result.Diagnostics)
                {
                    ConsoleWriter.WriteError(error.ToString());
                }
            }
        }

        private async Task<(CompilationUnitSyntax syntaxTree, SemanticModel semanticModel)> GetModels(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var syntaxTree = await tree.GetRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var model = MetadataReference.CreateFromFile(typeof(Model.Components.Component).Assembly.Location);
            var comp = CSharpCompilation.Create("Demo").AddSyntaxTrees(tree).AddReferences(mscorlib, model);

            var semanticModel = comp.GetSemanticModel(tree);

            return (syntaxTree, semanticModel);
        }
    }
}
