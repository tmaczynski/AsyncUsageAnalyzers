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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Immutable;

    /// <summary>
    /// This analyzer reports a diagnostic if System.Threading.Thread.Sleep() method is called.
    /// </summary>
    public class DontUseThreadSleepAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DontUseThreadSleepAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DontUseThreadSleep";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepTitle), UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly string Category = "AsyncUsage.CSharp.Usage";

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepDescription), UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly string HelpLink =
            "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/DontUseThreadSleep.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<SyntaxNodeAnalysisContext> HandleInvocationExpessionAction = HandleMethodDeclaration;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            // Code below requires Microsoft.CodeAnalysis to be upgraded from version 1.0.0.0 to a version that supports that operations
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleInvocationExpessionAction, SyntaxKind.InvocationExpression);
        }

        private static void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            // This check aims at increasing the performance.
            // Thanks to it, getting a semantic model in not necessary in majority of cases
            if (!invocationExpression.Expression.GetText().ToString().Contains("Sleep"))
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var fullyQualifiedName = "System.Threading.Thread";
            var methodName = "Sleep";

            IMethodSymbol methodSymbol;
            if (!invocationExpression.TryGetMethodSymbolByTypeNameAndMethodName(semanticModel, fullyQualifiedName, methodName, out methodSymbol))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation()));
        }
    }
}
