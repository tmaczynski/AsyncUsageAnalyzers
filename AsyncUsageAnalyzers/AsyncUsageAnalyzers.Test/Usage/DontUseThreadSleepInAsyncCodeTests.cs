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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class DontUseThreadSleepInAsyncCodeTests : DontUseThreadSleepCommonTests
    {
        public override DiagnosticResult[] TestThreadSleepInAsyncMethodExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(UsageResources.Method, "Method1Async").WithLocation(9, 9),
                this.CSharpDiagnostic().WithArguments(UsageResources.Method, "Method1Async").WithLocation(10, 9),
                this.CSharpDiagnostic().WithArguments(UsageResources.Method, "Method1Async").WithLocation(11, 9)
            };

        public override DiagnosticResult[] TestThreadSleepInAsyncLambdaExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(UsageResources.Method, "Method1Async").WithLocation(12, 13)
            };

        public override DiagnosticResult[] TestThreadSleepInAsyncAnonymousMethodExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(UsageResources.AnonymousMethod, string.Empty /* TODO: change it */).WithLocation(11, 9)
            };

        public override DiagnosticResult[] TestThreadSleepStaticImportExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(UsageResources.Method, "Method1Async").WithLocation(10, 9)
            };

        public override DiagnosticResult[] TestThreadSleepInNonAsyncMethod => EmptyDiagnosticResults;

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepInAsyncCodeAnalyzer();
        }
    }
}
