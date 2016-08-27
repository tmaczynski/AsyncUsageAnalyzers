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
    /// This analyzer reports a diagnostic if Thread.Sleep() method is called inside async method.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DontUseThreadSleepInAsyncCodeAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DontUseThreadSleepInAsyncCodeAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DontUseThreadSleepInAsyncCode";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncCodeTitle),
                UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncCodeMessageFormat),
                UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly string Category = "AsyncUsage.CSharp.Usage";

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncCodeDescription),
                UsageResources.ResourceManager, typeof(UsageResources));

        private static readonly string HelpLink =
            "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/DontUseThreadSleepInAsyncCode.md";

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

            context.RegisterSyntaxNodeAction(HandleInvocationExpessionAction, SyntaxKind.InvocationExpression);
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

            var semanticModel = context.SemanticModel;
            var fullyQualifiedName = "System.Threading.Thread";
            var methodName = "Sleep";

            IMethodSymbol methodSymbol;
            if (!invocationExpression.TryGetMethodSymbolByTypeNameAndMethodName(semanticModel, fullyQualifiedName, methodName, out methodSymbol))
            {
                return;
            }

            foreach (var syntaxNode in invocationExpression.Ancestors())
            {
                var methodDeclaration = syntaxNode as MethodDeclarationSyntax;
                if (methodDeclaration != null)
                {
                    if (HasAsyncMethodModifier(methodDeclaration))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), UsageResources.Method, methodDeclaration.Identifier));
                    }
                    else
                    {
                        return;
                    }
                }

                // This handles also AnonymousMethodExpressionSyntax since AnonymousMethodExpressionSyntax inherits from AnonymousMethodExpressionSyntax
                var anonymousFunctionDeclaration = syntaxNode as AnonymousFunctionExpressionSyntax;
                if (anonymousFunctionDeclaration != null)
                {
                    if (anonymousFunctionDeclaration.AsyncKeyword.Kind() != SyntaxKind.None)
                    {
                        // AsyncKeyword is expected
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), UsageResources.AnonymousMethod /* TODO: change the name of resource */, string.Empty /* TODO: change it  */));
                    }
                    else
                    {
                        // SyntaxKind.None is expected a message is non-async.
                        // If it's some unexpected SyntaxKind, so that this analysis rise not false positive results
                        break;
                    }
                }
            }
        }

        private static bool HasAsyncMethodModifier(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword);
        }

        private static bool HasAsyncAnonymousFunctionModifier(AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax)
        {
            return anonymousFunctionExpressionSyntax.AsyncKeyword != null;
        }
    }
}