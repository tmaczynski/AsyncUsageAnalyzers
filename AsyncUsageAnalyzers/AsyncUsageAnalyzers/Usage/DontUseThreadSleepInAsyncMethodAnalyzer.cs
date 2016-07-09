// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Usage
{
    using AsyncUsageAnalyzers.Helpers;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
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
            Analyzer analyzer = new Analyzer(
                context.Compilation.GetOrCreateGeneratedDocumentCache(),
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
                this.cancellationTokenType = compilation.GetTypeByMetadataName("System.Threading.Thread");
            }

            internal void HandleMethodDeclaration(SymbolAnalysisContext context)
            {
                var symbol = (IMethodSymbol)context.Symbol;
                // TODO: decide whether to subscribe for methods or method calls
                // http://stackoverflow.com/questions/29614112/how-to-get-invoked-method-name-in-roslyn
                // begin my section
                //var methodRoot = symbol.Construct()
                //methodRoot.SyntaxTree
                // end my section

                if (this.cancellationTokenType == null)
                {
                    return;
                }

                if (symbol.DeclaredAccessibility == Accessibility.Private || symbol.DeclaredAccessibility == Accessibility.NotApplicable)
                {
                    return;
                }

                if (symbol.IsImplicitlyDeclared)
                {
                    return;
                }

                switch (symbol.MethodKind)
                {
                    case MethodKind.LambdaMethod:
                    case MethodKind.Constructor:
                    case MethodKind.Conversion:
                    case MethodKind.UserDefinedOperator:
                    case MethodKind.PropertyGet:
                    case MethodKind.PropertySet:
                    case MethodKind.StaticConstructor:
                    case MethodKind.BuiltinOperator:
                    case MethodKind.Destructor:
                    case MethodKind.EventAdd:
                    case MethodKind.EventRaise:
                    case MethodKind.EventRemove:
                        // These can't be async
                        return;

                    case MethodKind.DelegateInvoke:
                        // Not sure if this is reachable
                        return;

                    case MethodKind.ExplicitInterfaceImplementation:
                        // These are ignored
                        return;

                    case MethodKind.ReducedExtension:
                    case MethodKind.Ordinary:
                    case MethodKind.DeclareMethod:
                    default:
                        break;
                }

                if (!symbol.IsInAnalyzedSource(this.generatedDocumentCache, context.CancellationToken))
                {
                    return;
                }

                if (!symbol.HasAsyncSignature())
                {
                    return;
                }

                if (symbol.IsOverrideOrImplementation())
                {
                    return;
                }

                if (symbol.IsTestMethod())
                {
                    return;
                }

                foreach (var parameterSymbol in symbol.Parameters)
                {
                    if (parameterSymbol.RefKind == RefKind.Out)
                    {
                        continue;
                    }

                    INamedTypeSymbol parameterType = parameterSymbol.Type as INamedTypeSymbol;
                    if (this.cancellationTokenType.Equals(parameterType))
                    {
                        return;
                    }
                }

                foreach (var parameterSymbol in symbol.Parameters)
                {
                    if (parameterSymbol.RefKind == RefKind.Out)
                    {
                        continue;
                    }

                    foreach (var member in parameterSymbol.Type.GetMembers(nameof(CancellationToken)))
                    {
                        if (member.Kind != SymbolKind.Property)
                        {
                            continue;
                        }

                        if (member.DeclaredAccessibility != Accessibility.Public)
                        {
                            continue;
                        }

                        var propertySymbol = (IPropertySymbol)member;
                        if (this.cancellationTokenType.Equals(propertySymbol.Type))
                        {
                            return;
                        }
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));

            }
        }
    }
}