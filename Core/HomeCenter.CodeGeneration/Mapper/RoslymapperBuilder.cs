using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

[assembly: InternalsVisibleTo("HomeCenter.Runner")]

namespace HomeCenter.CodeGeneration
{
    internal class RoslymapperBuilder
    {
        private const string DEST_MEMBER = "destination";
        private const string SRC_MEMBER = "source";
        private const string MAPPER_INTERFACE = "IMapper";
        private const string MAPPER_METHOD = "Map";
        private const string BASE_ADAPTER = "BaseAdapter";
        private const string CONFIG_METHOD = "Configure";
        private const string DIAGNOSTIC_CODE = "Roslymapper 666";

        private TransformationContext _context;

        public RichGenerationResult Build(TransformationContext context)
        {
            _context = context;
            var sourceClass = (ClassDeclarationSyntax)context.ProcessingNode;
            var semanticModel = context.SemanticModel;
            ClassDeclarationSyntax generatedClass = null;

            var classSemantic = semanticModel.GetDeclaredSymbol(sourceClass);
            var className = $"{classSemantic.Name}";

            try
            {
                var mapperBase = sourceClass?.BaseList?.Types.Select(baseType => semanticModel.GetTypeInfo(baseType.Type)).FirstOrDefault(m => m.Type.Name == MAPPER_INTERFACE);
                if (!mapperBase.HasValue)
                {
                    throw new Exception($"Class {sourceClass.Identifier} should inherit from interface IMapper");
                }

                var mapperInterface = mapperBase.Value.Type as INamedTypeSymbol;
                var sourceType = mapperInterface.TypeArguments[0] as INamedTypeSymbol;
                var destinationType = mapperInterface.TypeArguments[1] as INamedTypeSymbol;

                var mappingConfig = ReadConfiguration(sourceClass, semanticModel);

                generatedClass = GeneratePartialClass(className, sourceType, destinationType);
                var mapMethod = GenearteMap(sourceType, destinationType, mappingConfig);

                generatedClass = generatedClass.AddMembers(mapMethod);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString(), "");
                generatedClass = ClassDeclaration(className).WithCloseBraceToken(Token(TriviaList(Comment(string.Format("/*{0}*/", e))), SyntaxKind.CloseBraceToken, TriviaList()));
            }

            var namespaveDeclaration = AddToSourceClassNamespace(sourceClass, generatedClass);

            var result = new RichGenerationResult
            {
                Members = List<MemberDeclarationSyntax>().Add(namespaveDeclaration)
            };

            return result;
        }

        private static MappingConfig ReadConfiguration(ClassDeclarationSyntax sourceClass, SemanticModel semanticModel)
        {
            var config = new MappingConfig();
            IConfigureMapping<object, object> mappingContract;
            IMemberConfig<object, object, object> memberContract;
            var sourceNameResolverType = "HomeCenter.Model.Mapper.SimpleResolver, HomeCenter.Model, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            var destinationNameResolverType = "HomeCenter.Model.Mapper.SimpleResolver, HomeCenter.Model, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            var propertiesComparerType = "HomeCenter.Model.Mapper.IgnoreCaseComparer, HomeCenter.Model, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            var configurationMethod = sourceClass.Members.OfType<MethodDeclarationSyntax>().FirstOrDefault(m => m.Identifier.ToString() == CONFIG_METHOD);

            if (configurationMethod != null)
            {
                var methodBlock = configurationMethod.ChildNodes().OfType<BlockSyntax>().FirstOrDefault();
                if (methodBlock != null)
                {
                    foreach (var configExpression in methodBlock.ChildNodes().OfType<ExpressionStatementSyntax>())
                    {
                        var mapConfig = new MemberConfig();

                        foreach (var configInvocation in configExpression.DescendantNodes().OfType<InvocationExpressionSyntax>())
                        {
                            if (configInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
                            {
                                var name = memberAccess?.Name?.Identifier.ValueText;

                                if (name == nameof(memberContract.Ignore))
                                {
                                    mapConfig.Ignore = true;
                                }
                                else if (name == nameof(memberContract.WithDefault))
                                {
                                    var defaultValue = configInvocation.ArgumentList.Arguments[0].ChildNodes().FirstOrDefault();

                                    if (defaultValue != null && defaultValue is ExpressionSyntax exp)
                                    {
                                        mapConfig.DefaultValue = exp;
                                    }
                                }
                                else if (name == nameof(memberContract.WithValue))
                                {
                                    var arg = configInvocation.ArgumentList.Arguments[0].ChildNodes().FirstOrDefault();
                                    if (arg != null && arg is SimpleLambdaExpressionSyntax sle)
                                    {
                                        mapConfig.Value = sle;
                                    }
                                }
                                else if (name == nameof(mappingContract.ForMember))
                                {
                                    var arg = configInvocation.ArgumentList.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
                                    var member = arg?.Name?.Identifier.ValueText;

                                    if (!string.IsNullOrWhiteSpace(member))
                                    {
                                        mapConfig.Name = member;
                                    }
                                }
                                else if (name == nameof(mappingContract.IgnoreSourceMember))
                                {
                                    var arg = configInvocation.ArgumentList.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
                                    var member = arg?.Name?.Identifier.ValueText;

                                    if (!string.IsNullOrWhiteSpace(member))
                                    {
                                        config.IgnoredSources.Add(member);
                                    }
                                }
                                else if (name == nameof(mappingContract.WithSourceResolver))
                                {
                                    var arg = configInvocation.Expression.DescendantNodes().OfType<TypeArgumentListSyntax>().FirstOrDefault();
                                    if (arg != null)
                                    {
                                        var type = semanticModel.GetTypeInfo(arg.Arguments[0]);
                                        sourceNameResolverType = GetQualifiedTypeName(type.Type);
                                    }
                                }
                                else if (name == nameof(mappingContract.WithDestinationResolver))
                                {
                                    var arg = configInvocation.Expression.DescendantNodes().OfType<TypeArgumentListSyntax>().FirstOrDefault();
                                    if (arg != null)
                                    {
                                        var type = semanticModel.GetTypeInfo(arg.Arguments[0]);
                                        destinationNameResolverType = GetQualifiedTypeName(type.Type);
                                    }
                                }
                                else if (name == nameof(mappingContract.WithPropertiesComparer))
                                {
                                    var arg = configInvocation.Expression.DescendantNodes().OfType<TypeArgumentListSyntax>().FirstOrDefault();
                                    if (arg != null)
                                    {
                                        var type = semanticModel.GetTypeInfo(arg.Arguments[0]);
                                        propertiesComparerType = GetQualifiedTypeName(type.Type);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(mapConfig.Name))
                        {
                            config.Members.Add(mapConfig.Name, mapConfig);
                        }
                    }

                    config.SourceNameResolver = (INameResolver)Activator.CreateInstance(Type.GetType(sourceNameResolverType));
                    config.DestinationNameResolver = (INameResolver)Activator.CreateInstance(Type.GetType(destinationNameResolverType));
                    config.PropertiesComparer = (IPropertiesComparer)Activator.CreateInstance(Type.GetType(propertiesComparerType));
                }
            }

            return config;
        }

        private static string GetQualifiedTypeName(ISymbol symbol)
        {
            return symbol.ContainingNamespace
                + "." + symbol.Name
                + ", " + symbol.ContainingAssembly;
        }

        private ClassDeclarationSyntax GeneratePartialClass(string className, INamedTypeSymbol sourceType, INamedTypeSymbol destinationType)
        {
            return ClassDeclaration(className).AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
                                              .WithBaseList(
                                                BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                                                         SimpleBaseType(GenericName(Identifier(BASE_ADAPTER))
                                                            .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                ParseTypeName(sourceType.Name),
                                                                Token(SyntaxKind.CommaToken),
                                                                ParseTypeName(destinationType.Name)
                                                            })))))));
        }

        private NamespaceDeclarationSyntax AddToSourceClassNamespace(ClassDeclarationSyntax sourceClass, ClassDeclarationSyntax generatedClass)
        {
            return NamespaceDeclaration((sourceClass.Parent as NamespaceDeclarationSyntax)?.Name).AddMembers(generatedClass);
        }

        public MethodDeclarationSyntax GenearteMap(INamedTypeSymbol sourceType, INamedTypeSymbol destinationType, MappingConfig mappingConfig)
        {
            var list = new List<StatementSyntax>();
            list.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                            .WithVariables(SingletonSeparatedList(
                                    VariableDeclarator(Identifier(DEST_MEMBER)).WithInitializer(EqualsValueClause(ObjectCreationExpression(ParseTypeName(destinationType.Name)).WithArgumentList(ArgumentList())))))));
            list.AddRange(MapProperties(sourceType, destinationType, mappingConfig));
            list.Add(ReturnStatement(IdentifierName(DEST_MEMBER)));

            return MethodDeclaration(ParseTypeName(destinationType.Name), Identifier(MAPPER_METHOD))
                  .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                  .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(SRC_MEMBER)).WithType(ParseTypeName(sourceType.Name)))))
                  .WithBody(Block(list));
        }

        private static IEnumerable<ExpressionStatementSyntax> MapProperties(INamedTypeSymbol source, INamedTypeSymbol destination, MappingConfig mappingConfig)
        {
            var list = new List<ExpressionStatementSyntax>();

            var sourceProperties = source.GetMembers().Where(m => m is IPropertySymbol).ToDictionary(k => mappingConfig.SourceNameResolver.Resolve(k.Name), v => v);
            var destinationProperties = destination.GetMembers().Where(m => m is IPropertySymbol);

            foreach (var dest in destinationProperties)
            {
                var memberConfig = mappingConfig.Members.GetValueOrDefault(dest.Name, MemberConfig.Empty);

                if (memberConfig.Ignore) continue;

                ExpressionSyntax sourceExpression = null;
                if (memberConfig.Value != null)
                {
                    var par = memberConfig.Value.Parameter.Identifier.Text;
                    sourceExpression = memberConfig.Value.Body as ExpressionSyntax;

                    //Add source resolver support and default
                    var sourceIdentifires = memberConfig.Value.Body.DescendantNodes().OfType<IdentifierNameSyntax>().Where(i => i.Identifier.Text == par).ToList();
                    foreach (var si in sourceIdentifires)
                    {
                        sourceExpression = sourceExpression.ReplaceNode(si, SyntaxFactory.IdentifierName(SRC_MEMBER));
                    }
                }
                else
                {
                    var resolvedDestination = mappingConfig.DestinationNameResolver.Resolve(dest.Name);
                    var resolverSource = sourceProperties.Keys.SingleOrDefault(s => mappingConfig.PropertiesComparer.CanMap(s, resolvedDestination));

                    if (resolverSource == null)
                    {
                        Logger.Error($"Cannot map property {resolvedDestination} to any source field", DIAGNOSTIC_CODE);
                        continue;
                    }

                    var sourceProperty = sourceProperties[resolverSource];

                    sourceExpression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(SRC_MEMBER), IdentifierName(sourceProperty.Name));
                }

                if (memberConfig.DefaultValue != null)
                {
                    sourceExpression = BinaryExpression(SyntaxKind.CoalesceExpression, sourceExpression, memberConfig.DefaultValue);
                }

                var mapExpression = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(DEST_MEMBER), IdentifierName(dest.Name)),
                                            sourceExpression
                                            ));
                list.Add(mapExpression);
            }

            return list.ToArray();
        }
    }

    internal class MappingConfig
    {
        public Dictionary<string, MemberConfig> Members { get; } = new Dictionary<string, MemberConfig>();
        public List<string> IgnoredSources { get; } = new List<string>();
        public INameResolver SourceNameResolver { get; set; }
        public INameResolver DestinationNameResolver { get; set; }
        public IPropertiesComparer PropertiesComparer { get; set; }
    }

    public interface IConfigureMapping<Source, Destination>
    {
        void WithSourceResolver<T>() where T : class, INameResolver;

        void WithDestinationResolver<T>() where T : class, INameResolver;

        void WithPropertiesComparer<T>() where T : IPropertiesComparer;

        void IgnoreSourceMember(Func<Source, object> mapping);

        IMemberConfig<Source, Destination, Element> ForMember<Element>(Func<Destination, Element> mapping);
    }

    public interface IMemberConfig<Source, Destination, Element>
    {
        IMemberConfig<Source, Destination, Element> Ignore();

        IMemberConfig<Source, Destination, Element> WithDefault(Element defaultValue);

        IMemberConfig<Source, Destination, Element> WithValue(Func<Source, Element> valueResolver);
    }

    public class MemberConfig
    {
        public static MemberConfig Empty = new MemberConfig();
        public string Name { get; set; }
        public bool Ignore { get; set; }
        public ExpressionSyntax DefaultValue { get; set; }

        public SimpleLambdaExpressionSyntax Value { get; set; }
    }

    public static class CollectionsExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue def = default)
        {
            if (dic.ContainsKey(key)) return dic[key];
            return def;
        }
    }
}