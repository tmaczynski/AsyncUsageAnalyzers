// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Recommendations;
    using Microsoft.CodeAnalysis.SemanticModelWorkspaceService;

    /// <summary>
    /// Implements a code fix for <see cref="PropagateCancellationTokenAnalyzer"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropagateCancellationTokenCodeFixProvider))]
    [Shared]
    internal class PropagateCancellationTokenCodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableArray<string> FixableDiagnostics =
            ImmutableArray.Create(PropagateCancellationTokenAnalyzer.DiagnosticId);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return CustomFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var document = context.Document;
                var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
                var workspace = document.Project.Solution.Workspace;

                var recommendedSymbols =
                    await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, diagnostic.Location.SourceSpan.Start, workspace).ConfigureAwait(false);

                var recommendedSymbolsList = recommendedSymbols.ToList();

                foreach (var recommendedSymbol in recommendedSymbols)
                {
                    var recommendedTypeSymbol = recommendedSymbol as IParameterSymbol;
                    if (recommendedTypeSymbol != null
                        && PropagateCancellationTokenAnalyzer.IsCancellationToken(recommendedTypeSymbol.Type)
                        && recommendedTypeSymbol.CanBeReferencedByName)
                    {
                        // this code is over-complicated, but it might be useful in other cases
                        // 
                        // var displayParts = recommendedTypeSymbol.ToMinimalDisplayParts(semanticModel, diagnostic.Location.SourceSpan.Start, SymbolDisplayFormat.MinimallyQualifiedFormat);
                        // if (!displayParts.Any())
                        // {
                        //     return;
                        // }
                        //
                        // var displayString = displayParts.Where(x => x.Kind == SymbolDisplayPartKind.ParameterName).First();

                        var displayString = recommendedTypeSymbol.Name;

                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Propagate Cancellation Tokens",
                                cancellationToken => GetTransformedDocumentAsync(displayString, context.Document, diagnostic, cancellationToken)),
                            diagnostic);
                    }

                    var localSymbol = recommendedSymbol as ILocalSymbol;
                    if (localSymbol != null
                        && PropagateCancellationTokenAnalyzer.IsCancellationToken(localSymbol.Type)
                        && localSymbol.CanBeReferencedByName)
                    {
                        var displayString = localSymbol.Name;

                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Propagate Cancellation Tokens",
                                cancellationToken => GetTransformedDocumentAsync(displayString, context.Document, diagnostic, cancellationToken)),
                            diagnostic);
                    }
                }
            }
        }

        private static async Task<Document> GetTransformedDocumentAsync(string displayString, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
            ExpressionSyntax expression = root.FindNode(diagnostic.Location.SourceSpan).DescendantNodesAndSelf().OfType<ExpressionSyntax>().First();

            var newExpression = SyntaxFactory.ParseExpression(displayString);

            var newRoot = root.ReplaceNode(expression, newExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
