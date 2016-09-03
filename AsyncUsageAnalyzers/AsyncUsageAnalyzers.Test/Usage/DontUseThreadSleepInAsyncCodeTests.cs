// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Test.Usage
{
    using AsyncUsageAnalyzers.Usage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class DontUseThreadSleepInAsyncCodeTests : DontUseThreadSleepTestsBase
    {
        public override DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments) =>
            diagnostic.WithArguments(arguments);

        protected override DiagnosticResult[] TestThreadSleepInAsyncLambdaExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(UsageResources.AsyncAnonymousFunctionsAndMethods, string.Empty).WithLocation(12, 13)
            };

        protected override DiagnosticResult[] TestThreadSleepInLambdaExpectedResult => EmptyDiagnosticResults;

        protected override DiagnosticResult[] TestThreadSleepInAsyncAnonymousMethodExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(UsageResources.AsyncAnonymousFunctionsAndMethods, string.Empty /* TODO: change it */).WithLocation(11, 9)
            };

        protected override DiagnosticResult[] TestThreadSleepInAnonymousMethodExpectedResult => EmptyDiagnosticResults;

        protected override DiagnosticResult[] TestThreadSleepStaticImportExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(string.Format(UsageResources.MethodFormat, "Method1Async")).WithLocation(10, 9)
            };

        protected override DiagnosticResult[] TestThreadSleepInMethod => EmptyDiagnosticResults;

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepInAsyncCodeAnalyzer();
        }
    }
}
