// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}
