﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Helpers
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class ExpressionSyntaxExtensions
    {
        public static bool TryGetMethodSymbolByTypeNameAndMethodName(
            this ExpressionSyntax invocationExpression,
            SemanticModel semanticModel,
            string fullyQualifiedName,
            string methodName,
            out IMethodSymbol methodSymbol)
        {
            methodSymbol = ModelExtensions.GetSymbolInfo(semanticModel, invocationExpression).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            var typeMetadata = semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedName);
            return typeMetadata.Equals(methodSymbol.ReceiverType) && (methodSymbol.Name == methodName);
        }

        public static bool TryGetFieldSymbolByTypeNameAndMethodName(
            this ExpressionSyntax invocationExpression,
            SemanticModel semanticModel,
            string fullyQualifiedName,
            string propertyName,
            out IFieldSymbol propertySymbol)
        {
            propertySymbol = ModelExtensions.GetSymbolInfo(semanticModel, invocationExpression).Symbol as IFieldSymbol;
            if (propertySymbol == null)
            {
                return false;
            }

            var typeMetadata = semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedName);
            return typeMetadata.Equals(propertySymbol.ContainingType) && (propertySymbol.Name == propertyName);
        }

        public static bool IsInsideAsyncCode(this ExpressionSyntax invocationExpression, ref SyntaxNode enclosingMethodOrFunctionDeclaration)
        {
            foreach (var syntaxNode in invocationExpression.Ancestors())
            {
                var methodDeclaration = syntaxNode as MethodDeclarationSyntax;
                if (methodDeclaration != null)
                {
                    enclosingMethodOrFunctionDeclaration = syntaxNode;
                    return HasAsyncMethodModifier(methodDeclaration);
                }

                // This handles also AnonymousMethodExpressionSyntax since AnonymousMethodExpressionSyntax inherits from AnonymousFunctionExpressionSyntax
                var anonymousFunction = syntaxNode as AnonymousFunctionExpressionSyntax;
                if (anonymousFunction != null)
                {
                    enclosingMethodOrFunctionDeclaration = syntaxNode;
                    return IsAsyncAnonymousFunction(anonymousFunction);
                }
            }

            return false;
        }

        public static bool IsInsideAsyncCode(this ExpressionSyntax invocationExpression)
        {
            SyntaxNode enclosingMethodOrFunctionDeclaration = null;
            return invocationExpression.IsInsideAsyncCode(ref enclosingMethodOrFunctionDeclaration);
        }

        private static bool HasAsyncMethodModifier(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword);

        private static bool IsAsyncAnonymousFunction(AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax) =>
            anonymousFunctionExpressionSyntax.AsyncKeyword.Kind() == SyntaxKind.AsyncKeyword;
    }
}