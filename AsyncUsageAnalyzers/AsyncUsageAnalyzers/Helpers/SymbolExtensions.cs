// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Helpers
{
    using System.Collections.Concurrent;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    internal static class SymbolExtensions
    {
        private static readonly SymbolDisplayFormat SymbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        public static bool IsInAnalyzedSource(this ISymbol symbol, ConcurrentDictionary<SyntaxTree, bool> generatedHeaderCache, CancellationToken cancellationToken)
        {
            if (symbol.Locations.IsDefaultOrEmpty)
            {
                return false;
            }

            Location location = symbol.Locations[0];
            if (!location.IsInSource || location.SourceTree.IsGeneratedDocument(generatedHeaderCache, cancellationToken))
            {
                return false;
            }

            return true;
        }

        public static bool IsOfType(this ITypeSymbol typeSymbol, string fullyQualifiedName) =>
            fullyQualifiedName == typeSymbol.ToDisplayString(SymbolDisplayFormat);
    }
}
