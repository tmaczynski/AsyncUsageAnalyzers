// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Usage
{
    using System.Linq;
    using AsyncUsageAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer a base class for analyzers repoting usage of System.Threading.Thread.Sleep() method.
    /// </summary>
    public abstract class DontUseThreadSleepAnalyzerBase : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            // Code below requires Microsoft.CodeAnalysis to be upgraded from version 1.0.0.0 to a version that supports that operations
            var analyzer = this.GetAnalyzer();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(analyzer.HandleInvocation, SyntaxKind.InvocationExpression);
        }

        protected abstract AnalyzerBase GetAnalyzer();

        protected abstract class AnalyzerBase
        {
            protected abstract void ReportDiagnosticOnThreadSleepInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression);

            internal void HandleInvocation(SyntaxNodeAnalysisContext context)
            {
                var invocationExpression = (InvocationExpressionSyntax)context.Node;

                // This check aims at increasing the performance.
                // Thanks to it, getting a semantic model in not necessary in majority of cases.
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

                this.ReportDiagnosticOnThreadSleepInvocation(context, invocationExpression);
            }
         }
    }
}
