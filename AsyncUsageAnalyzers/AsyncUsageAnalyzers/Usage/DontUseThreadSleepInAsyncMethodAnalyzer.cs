// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using System.Linq.Expressions;

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
    using System.Collections.Concurrent;

    /// <summary>
    /// This analyzer reports a diagnostic if Thread.Sleep() method is called inside async method.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DontUseThreadSleepInAsyncMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DontUseThreadSleepInAsyncMethodAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DontUseThreadSleepInAsyncMethod";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncMethodTitle),
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

            context.RegisterSyntaxNodeAction(HandleInvocationExpessionAction, /* SyntaxKind.SimpleMemberAccessExpression, */SyntaxKind.InvocationExpression);
            // TODO: extend this analysis to async lambda expression and anonymous methods
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

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            var threadTypeMetadata = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Thread");
            if (!threadTypeMetadata.Equals(methodSymbol.ReceiverType))
            {
                return;
            }

            if (methodSymbol.Name == "Sleep")
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), "Method1Async" /* methodName, TODO: change it */));
            }
            

            /*
            if (!HasAsyncMethodModifier(methodDeclaration))
            {
                return;
            }
            */
            
            //

            //var invocationsWithinTheMethod = context.Node.DescendantNodes()
            //    .OfType<InvocationExpressionSyntax>();

            //// TODO: add check which uses threadTypeMetadata
            //var invocationsOfThreadSleep = invocationsWithinTheMethod
            //    .Where(invocation =>
            //    {
            //        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            //        // TODO: simplify this
            //        return methodSymbol != null
            //               && methodSymbol.Name == "Sleep"
            //               && methodSymbol.ContainingNamespace.Name == "Threading"
            //               && methodSymbol.ContainingNamespace.ContainingNamespace.Name == "System"
            //               && methodSymbol.ContainingNamespace.ContainingNamespace.ContainingNamespace == null;

            //    })
            //    .ToList();

            //foreach (var invocation in invocationsOfThreadSleep)
            //{
            //    //var methodName = methodDeclaration.Identifier.Text;
            //    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), string.Empty /* methodName */));
            //}
        }

        private static bool HasAsyncMethodModifier(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword);
        }
    }
}