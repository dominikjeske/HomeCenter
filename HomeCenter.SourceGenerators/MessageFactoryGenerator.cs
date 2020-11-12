using HomeCenter.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                var factoryModel = GetModel(proxy, context.GeneratorExecutionContext.Compilation);

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

        private MessageFactoryModel GetModel(ClassDeclarationSyntax classSyntax, Compilation compilation)
        {
            var root = classSyntax.GetCompilationUnit();

            var proxyModel = new MessageFactoryModel
            {
                ClassName = classSyntax.GetClassName(),

                ClassModifier = classSyntax.GetClassModifier(),

                Usings = root.GetUsings(),

                Namespace = root.GetNamespace(),

                Commands = GetCommands(compilation),

                Events = GetEvents(compilation)
            };

            return proxyModel;
        }

        private List<CommandDescriptor> GetCommands(Compilation compilation)
        {
            return compilation.GetSymbolsWithName(x => x.IndexOf(nameof(Command)) > -1, SymbolFilter.Type)
                              .OfType<INamedTypeSymbol>()
                              .Where(y => y.BaseType.Name == nameof(Command) && !y.IsAbstract)
                              .Select(c => new CommandDescriptor
                              {
                                  Name = c.Name,
                                  Namespace = c.ContainingNamespace.ToString()
                              })
                              .ToList();
        }

        private List<EventDescriptor> GetEvents(Compilation compilation)
        {
            return compilation.GetSymbolsWithName(x => x.IndexOf(nameof(Event)) > -1, SymbolFilter.Type)
                              .OfType<INamedTypeSymbol>()
                              .Where(y => y.BaseType.Name == nameof(Event) && !y.IsAbstract)
                              .Select(c => new EventDescriptor
                              {
                                  Name = c.Name,
                                  Namespace = c.ContainingNamespace.ToString()
                              })
                              .ToList();
        }
    }
}