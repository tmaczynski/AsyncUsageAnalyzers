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

    public class DontUseThreadSleepTests : DontUseThreadSleepTestsBase
    {
        protected override DiagnosticResult[] TestThreadSleepInAsyncMethodExpectedResult => new[]
            {
                this.CSharpDiagnostic().WithLocation(9, 9),
                this.CSharpDiagnostic().WithLocation(10, 9),
                this.CSharpDiagnostic().WithLocation(11, 9)
            };

        protected override DiagnosticResult[] TestThreadSleepInAsyncLambdaExpectedResult => new[]
            {
                this.CSharpDiagnostic().WithLocation(12, 13)
            };

        protected override DiagnosticResult[] TestThreadSleepInAsyncAnonymousMethodExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithLocation(11, 9)
            };

        protected override DiagnosticResult[] TestThreadSleepInAnonymousMethodExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithLocation(11, 9)
            };

        // TODO: update this
        protected override DiagnosticResult[] TestThreadSleepStaticImportExpectedResult => new[]
            {
                this.CSharpDiagnostic().WithLocation(10, 9)
            };

        protected override DiagnosticResult[] TestThreadSleepInMethod => new[]
            {
                this.CSharpDiagnostic().WithLocation(9, 9),
                this.CSharpDiagnostic().WithLocation(10, 9),
                this.CSharpDiagnostic().WithLocation(11, 9)
            };

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepAnalyzer();
        }
    }
}
