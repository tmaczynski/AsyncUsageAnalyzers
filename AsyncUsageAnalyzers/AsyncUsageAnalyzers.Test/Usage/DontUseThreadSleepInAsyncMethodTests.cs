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

    public class DontUseThreadSleepInAsyncMethodTests : DiagnosticVerifier
    {
        [Fact]
        public async Task TestThreadSleepInSimpleMethodAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public async Task<int> Method1Async()
    {
        Thread.Sleep(1000);
        System.Threading.Thread.Sleep(1000);
        global::System.Threading.Thread.Sleep(1000);
        
        return await Task.FromResult(0); 
    }
}";

            var expected = new []
            {
                this.CSharpDiagnostic().WithArguments("Method1Async").WithLocation(9, 9),
                this.CSharpDiagnostic().WithArguments("Method1Async").WithLocation(10, 9),
                this.CSharpDiagnostic().WithArguments("Method1Async").WithLocation(11, 9)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestTaskDelayInSimpleMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
using System.Threading;

class ClassA
{
    public async void Method1Async()
    {
        await Task.Delay(1000);
        await Task.FromResult(0); 
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepInAsyncMethodAnalyzer();
        }
    }
}
