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
        public override DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments) =>
            diagnostic;

        [Fact]
        public async Task TestThreadSleepInLambdaAsync()
        {
            string testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public void BadExample()
    {
        Func<int,int> testFunc = (x) =>
        {
            Thread.Sleep(0);
            return x;
        };
    }
}";
            var expectedResult = new[]
            {
                this.CSharpDiagnostic().WithLocation(12, 13)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expectedResult, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestThreadSleepInAnonymousMethodAsync()
        {
            string testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate void SampleDelegate();
    SampleDelegate AnonymousMethod1 = delegate ()
    {
        Thread.Sleep(0);
    };
}";
            var result =
                new[]
                {
                    this.CSharpDiagnostic().WithLocation(11, 9)
                };

            await this.VerifyCSharpDiagnosticAsync(testCode, result, CancellationToken.None).ConfigureAwait(false);
        }

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
