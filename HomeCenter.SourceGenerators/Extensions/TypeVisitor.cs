using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace HomeCenter.SourceGenerators
{
    internal class TypeVisitor : SymbolVisitor
    {
        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<INamedTypeSymbol> _exportedTypes;
        private readonly Func<INamedTypeSymbol, bool> _searchQuery;

        public TypeVisitor(Func<INamedTypeSymbol, bool> SearchQuery, CancellationToken cancellation)
        {
            _cancellationToken = cancellation;
            _exportedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            _searchQuery = SearchQuery;
        }

        public ImmutableArray<INamedTypeSymbol> GetResults() => _exportedTypes.ToImmutableArray();

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (INamespaceOrTypeSymbol namespaceOrType in symbol.GetMembers())
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (namespaceOrType.ToDisplayString().IndexOf("HomeCenter") > -1)
                {
                    namespaceOrType.Accept(this);
                }
            }
        }

        public override void VisitNamedType(INamedTypeSymbol type)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (_searchQuery(type))
            {
                _exportedTypes.Add(type);
            }

            foreach(var x in type.GetTypeMembers())
            {
                var xxx = x.Name;
                var vvv = x.TypeKind;
            }
        }
    }
}