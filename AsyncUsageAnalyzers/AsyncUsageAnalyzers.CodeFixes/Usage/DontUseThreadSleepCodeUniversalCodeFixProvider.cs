// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using System.Linq;
using Microsoft.CodeAnalysis.Text;

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

    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic /* TODO: check it */, Name = nameof(DontUseThreadSleepCodeUniversalCodeFixProvider))]
    [Shared]
    internal class DontUseThreadSleepCodeUniversalCodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableArray<string> FixableDiagnostics =
                ImmutableArray.Create(DontUseThreadSleepAnalyzer.DiagnosticId, DontUseThreadSleepInAsyncCodeAnalyzer.DiagnosticId /* TODO: adjust fix for this analysis */);

        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;


        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return CustomFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == DontUseThreadSleepInAsyncCodeAnalyzer.DiagnosticId)
                {
                    RegisterCodeFixForDiagnosic(context, diagnostic);
                }
                else if (diagnostic.Id == DontUseThreadSleepAnalyzer.DiagnosticId)
                {
                    var document = context.Document;

                    // TODO: check it awaiting stuff here does not cause a major slowdown
                    var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
                    var tokenWithDiagnostic = root.FindNode(TextSpan.FromBounds(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.End), getInnermostNodeForTie: true) as InvocationExpressionSyntax;
                    SyntaxNode methodOrFunctionNode = null;
                    if (DontUseThreadSleepInAsyncCodeAnalyzer.IsInsideAsyncCode(tokenWithDiagnostic, ref methodOrFunctionNode))
                    {
                        RegisterCodeFixForDiagnosic(context, diagnostic);
                    }
                }
            }
        }

        private static void RegisterCodeFixForDiagnosic(CodeFixContext context, Diagnostic diagnostic)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Code Fix for " /* ReadabilityResources.SA1139CodeFix*/,
                    cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken)),
                diagnostic);
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var firstNodeWithCorrectSpan = root
                .FindNode(diagnostic.Location.SourceSpan);
            InvocationExpressionSyntax expression = firstNodeWithCorrectSpan
                .DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .First();

            var arguments = expression.ArgumentList;

            var newExpression = GenerateTaskDelayExpression(arguments);

            SyntaxNode newRoot = root.ReplaceNode(expression, newExpression.WithTriviaFrom(expression));
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static AwaitExpressionSyntax GenerateTaskDelayExpression(
            ArgumentListSyntax methodArgumentList) =>
                SyntaxFactory.AwaitExpression(
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
                        .WithArgumentList(methodArgumentList));
    }
}
