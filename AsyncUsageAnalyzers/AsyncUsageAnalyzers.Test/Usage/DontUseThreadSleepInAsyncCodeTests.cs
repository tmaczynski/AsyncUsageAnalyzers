// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using System.Threading;

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
    using System.Threading.Tasks;

    public class DontUseThreadSleepInAsyncCodeTests : DontUseThreadSleepTestsBase
    {
        public override DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments) =>
            diagnostic.WithArguments(arguments);



        protected override DiagnosticResult[] TestThreadSleepInAnonymousMethodExpectedResult => EmptyDiagnosticResults;

        protected override DiagnosticResult[] TestThreadSleepStaticImportExpectedResult =>
            new[]
            {
                this.CSharpDiagnostic().WithArguments(string.Format(UsageResources.MethodFormat, "Method1Async")).WithLocation(10, 9)
            };

        [Fact]
        public async Task TestThreadSleepNonAsyncCodeAsync()
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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override DiagnosticResult[] TestThreadSleepInMethod => EmptyDiagnosticResults;

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepInAsyncCodeAnalyzer();
        }
    }
}
