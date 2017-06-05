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

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<CompilationStartAnalysisContext> CompilationStartAction = HandleCompilationStart;
        private static readonly Action<SyntaxNodeAnalysisContext> HandleSimpleMemberAccessAction = HandleHandleSimpleMemberAccess;
        private static readonly Action<SyntaxNodeAnalysisContext> HandleDefaultExpressionAction = HandleDefaultExpression;

        private static readonly string CancellationTokenEnclosingNamespace = "System.Threading";
        private static readonly string CancellationTokenClassName = "CancellationToken";
        private static readonly string CancellationTokenFullyQualifiedName = $"{CancellationTokenEnclosingNamespace}.{CancellationTokenClassName}";
        private static readonly string NonePropertyName = "None";

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
            context.RegisterSyntaxNodeActionHonorExclusions(HandleSimpleMemberAccessAction, SyntaxKind.SimpleMemberAccessExpression);
            context.RegisterSyntaxNodeActionHonorExclusions(HandleDefaultExpressionAction, SyntaxKind.DefaultExpression);
        }

        private static void HandleDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var defaultExpression = (DefaultExpressionSyntax)context.Node;
            var defaultExpressionType = defaultExpression.Type;

            var semanticModel = context.SemanticModel;

            // TODO: make code below an extension method
            var symbol = ModelExtensions.GetSymbolInfo(semanticModel, defaultExpressionType).Symbol as ITypeSymbol;
            if (symbol == null || symbol.Name != CancellationTokenClassName)
            {
                return;
            }

            string methodName;
            if (IsMethodArgumentAndAnotherCancellationTokenIsInScope(defaultExpression, semanticModel, out methodName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, defaultExpression.GetLocation(), UsageResources.DefaultCancellationToken, methodName));
            }
        }

        private static void HandleHandleSimpleMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (MemberAccessExpressionSyntax)context.Node;

            var semanticModel = context.SemanticModel;

            // This check aims at increasing the performance.
            // Thanks to it, using semantic model in not necessary in majority of cases.
            if (!invocationExpression.Expression.GetText().ToString().Contains(CancellationTokenClassName))
            {
                return;
            }

            IPropertySymbol propertySymbol;
            if (!invocationExpression.TryGetPropertySymbolByTypeNameAndMethodName(semanticModel, CancellationTokenFullyQualifiedName, NonePropertyName, out propertySymbol))
            {
                return;
            }

            string methodName;
            if (IsMethodArgumentAndAnotherCancellationTokenIsInScope(invocationExpression, semanticModel, out methodName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), UsageResources.CancellationTokenNone, methodName));
            }
        }

        private static bool IsMethodArgumentAndAnotherCancellationTokenIsInScope(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            out string methodName)
        {
            return IsMethodCallArgument(expression, out methodName)
                   && HasAnotherCancellationTokenInScope(semanticModel, expression);
        }

        private static bool HasAnotherCancellationTokenInScope(
            SemanticModel semanticModel,
            SyntaxNode node)
        {
            var symbols = semanticModel.LookupSymbols(node.SpanStart).ToList();

            // TODO: don't call ToList() in final version
            var localSymbols = symbols.OfType<ILocalSymbol>().ToList();
            var parameters = symbols.OfType<IParameterSymbol>().ToList();

            return localSymbols.Any() || parameters.Any();
        }

        private static bool IsMethodCallArgument(ExpressionSyntax expression, out string methodName)
        {
            var enclosingArg = expression.Parent as ArgumentSyntax;
            var enclosingArgList = enclosingArg?.Parent as ArgumentListSyntax;
            var enclosingInvocationExpr = enclosingArgList?.Parent as InvocationExpressionSyntax;
            if (enclosingInvocationExpr == null)
            {
                methodName = null;
                return false;
            }

            methodName = enclosingInvocationExpr.Expression.ToString();
            return true;
        }
    }
}
