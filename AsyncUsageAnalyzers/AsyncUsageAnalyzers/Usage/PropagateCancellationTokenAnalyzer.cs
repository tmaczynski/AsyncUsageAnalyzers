// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer which reports a diagnostic if <see cref="CancellationToken.None"/>.
    /// is explicitly provided in a method call, but another cancellation token is available in the current context.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropagateCancellationTokenAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="PropagateCancellationTokenAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "PropagateCancellationToken";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(UsageResources.PropagateCancellationTokenTitle), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(UsageResources.PropagateCancellationTokenMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string Category = "AsyncUsage.CSharp.Usage";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(UsageResources.PropagateCancellationTokenDescription), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/PropagateCancellationToken.md";

        private static readonly Action<CompilationStartAnalysisContext> CompilationStartAction = HandleCompilationStart;
        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<SyntaxNodeAnalysisContext> HandleInvocationAction = HandleInvocation;

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
            context.RegisterSyntaxNodeActionHonorExclusions(HandleInvocationAction, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (MemberAccessExpressionSyntax)context.Node;

            var semanticModel = context.SemanticModel;
            var enclosingNamespace = "System.Threading";
            var className = "CancellationToken";
            var fullyQualifiedName = $"{enclosingNamespace}.{className}";
            var propertyName = "None";

            // This check aims at increasing the performance.
            // Thanks to it, getting a semantic model in not necessary in majority of cases.
            if (!invocationExpression.Expression.GetText().ToString().Contains(className))
            {
                return;
            }

            IPropertySymbol propertySymbol;
            if (!invocationExpression.TryGetPropertySymbolByTypeNameAndMethodName(semanticModel, fullyQualifiedName, propertyName, out propertySymbol))
            {
                return;
            }

            var enclosingArg = invocationExpression.Parent as ArgumentSyntax;
            var enclosingArgList = enclosingArg?.Parent as ArgumentListSyntax;
            var enclosingInvocationExpr = enclosingArgList?.Parent as InvocationExpressionSyntax;
            if (enclosingInvocationExpr == null)
            {
                return;
            }

            var methodName = enclosingInvocationExpr.Expression.ToString();
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), methodName));
        }
    }
}
