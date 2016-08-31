// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using System.Linq;

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    internal abstract class DontUseThreadSleepCodeUniversalCodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableArray<string> FixableDiagnostics =
                ImmutableArray.Create(DontUseThreadSleepAnalyzer.DiagnosticId, DontUseThreadSleepInAsyncCodeAnalyzer.DiagnosticId);

        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;

        // TODO: consider overriding GetFixAllProvider();

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Code Fix for " /* ReadabilityResources.SA1139CodeFix*/,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            InvocationExpressionSyntax expression = (InvocationExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan);

            var arguments = expression.ArgumentList.Arguments;

            var newExpression = expression;

            SyntaxNode newRoot = root.ReplaceNode(expression, newExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static InvocationExpressionSyntax GenerateTaskDeleyExpression(
            SeparatedSyntaxList<ArgumentSyntax> methodArgumentList) =>
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("System"),
                                        SyntaxFactory.IdentifierName("Threading")),
                                    SyntaxFactory.IdentifierName("Tasks")),
                                SyntaxFactory.IdentifierName("Task")),
                            SyntaxFactory.IdentifierName("Delay")))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(1 /* TODO: change it to methodArgumentList */))))));

    }
}
