using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HomeCenter.SourceGenerators
{
    [Generator]
    public class MessageFactoryGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MessageFactorySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            using var sourceGenContext = SourceGeneratorContext<MessageFactoryGenerator>.Create(context);

            if (context.SyntaxReceiver is MessageFactorySyntaxReceiver actorSyntaxReciver)
            {
                foreach (var proxy in actorSyntaxReciver.CandidateProxies)
                {
                    var source = GenearteProxy(proxy, sourceGenContext);
                    context.AddSource(source.FileName, SourceText.From(source.SourceCode, Encoding.UTF8));
                }
            }
        }

        private GeneratedSource GenearteProxy(ClassDeclarationSyntax proxy, SourceGeneratorContext<MessageFactoryGenerator> context)
        {
            try
            {
                var factoryModel = GetModel(proxy, context.GeneratorExecutionContext.Compilation, context.GeneratorExecutionContext.CancellationToken);

                var templateString = ResourceReader.GetResource("MessageFactory.scriban");

                var result = TemplateGenerator.Execute(templateString, factoryModel);

                context.TryLogSourceCode(proxy, result);

                return new GeneratedSource(result, nameof(MessageFactoryGenerator));
            }
            catch (Exception ex)
            {
                context.TryLogException(proxy, ex);
                return context.GenerateErrorSourceCode(ex, proxy);
            }
        }

        private MessageFactoryModel GetModel(ClassDeclarationSyntax classSyntax, Compilation compilation, CancellationToken token)
        {
            var root = classSyntax.GetCompilationUnit();

            var proxyModel = new MessageFactoryModel
            {
                ClassName = classSyntax.GetClassName(),

                ClassModifier = classSyntax.GetClassModifier(),

                Usings = root.GetUsings(),

                Namespace = root.GetNamespace(),

                Commands = GetCommands(compilation, token),

                Events = GetEvents(compilation, token)
            };

            return proxyModel;
        }

        private List<CommandDescriptor> GetCommands(Compilation compilation, CancellationToken token)
        {
            return compilation.GetAllWithBaseClass("HomeCenter.Abstractions.Command", token).Select(c => new CommandDescriptor
            {
                Name = c.Name,
                Namespace = c.ContainingNamespace.ToString()
            }).ToList();
        }

        private List<EventDescriptor> GetEvents(Compilation compilation, CancellationToken token)
        {
            return compilation.GetAllWithBaseClass("HomeCenter.Abstractions.Event", token).Select(c => new EventDescriptor
            {
                Name = c.Name,
                Namespace = c.ContainingNamespace.ToString()
            }).ToList();
        }
    }
}