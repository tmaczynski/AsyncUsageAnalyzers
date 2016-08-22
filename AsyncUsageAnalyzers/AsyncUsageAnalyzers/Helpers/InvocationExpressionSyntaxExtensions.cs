// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsyncUsageAnalyzers.Helpers
{
    internal static class InvocationExpressionSyntaxExtensions
    {
        public static bool TryGetMethodSymbolByTypeNameAndMethodName(
            this InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel,
            string fullyQualifiedName,
            string methodName,
            out IMethodSymbol methodSymbol)
        {
            methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            var threadTypeMetadata = semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedName);
            if (!threadTypeMetadata.Equals(methodSymbol.ReceiverType))
            {
                return false;
            }

            if (methodSymbol.Name != methodName)
            {
                return false;
            }

            return true;
        }
    }
}
