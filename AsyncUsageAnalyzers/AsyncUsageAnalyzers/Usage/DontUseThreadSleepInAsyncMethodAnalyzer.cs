// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using System.Threading;

namespace AsyncUsageAnalyzers.Usage
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using System.Collections.Immutable;
    using System.Collections.Concurrent;

    /// <summary>
    /// This analyzer reports a diagnostic if an asynchronous if Thread.Sleep() method is called inside async method.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DontUseThreadSleepInAsyncMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets ID for diagnostics produced by the <see cref="DontUseThreadSleepInAsyncMethodAnalyzer"/> analyzer.
        /// </summary>
        /// <value>
        /// ID for diagnostics produced by the <see cref="DontUseThreadSleepInAsyncMethodAnalyzer"/> analyzer.
        /// </value>
        public const string DiagnosticId = "DontUseThreadSleepInAsyncMethod";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(UsageResources.IncludeCancellationParameterTitle),
                UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncMethodMessageFormat),
                UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly string Category = "AsyncUsage.CSharp.Usage";

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncMethodDescription),
                UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly string HelpLink =
            "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/DontUseThreadSleepInAsyncMethod.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
                AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<CompilationStartAnalysisContext> CompilationStartAction = HandleCompilationStart;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(CompilationStartAction);
        }

        private static void HandleCompilationStart(CompilationStartAnalysisContext context)
        {
            Analyzer analyzer = new Analyzer(context.Compilation.GetOrCreateGeneratedDocumentCache(),
                context.Compilation);
            context.RegisterSymbolAction(analyzer.HandleMethodDeclaration, SymbolKind.Method);
        }

        // TODO: remove code duplication between this and IncludeCancellationParameterAnalyzer's Analyzer inner class
        private class Analyzer
        {
            private readonly ConcurrentDictionary<SyntaxTree, bool> generatedDocumentCache;
            private INamedTypeSymbol cancellationTokenType;

            public Analyzer(ConcurrentDictionary<SyntaxTree, bool> generatedDocumentCache, Compilation compilation)
            {
                this.generatedDocumentCache = generatedDocumentCache;
                this.cancellationTokenType = compilation.GetTypeByMetadataName(typeof(CancellationToken).FullName);
            }

            internal void HandleMethodDeclaration(SymbolAnalysisContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}