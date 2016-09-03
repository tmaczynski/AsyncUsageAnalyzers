﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

using Microsoft.CodeAnalysis.CodeFixes;

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

    public abstract class DontUseThreadSleepTestsBase : CodeFixVerifier
    {
        protected abstract DiagnosticResult[] TestThreadSleepInAsyncMethodExpectedResult { get; }

        [Fact]
        public async Task TestThreadSleepInAsyncMethodAsync()
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

            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public async Task<int> Method1Async()
    {
        await System.Threading.Tasks.Task.Delay(1000);
        await System.Threading.Tasks.Task.Delay(1000);
        await System.Threading.Tasks.Task.Delay(1000);
        
        return await Task.FromResult(0); 
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInAsyncMethodExpectedResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        protected abstract DiagnosticResult[] TestThreadSleepInLambdaExpectedResult { get; }

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

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInLambdaExpectedResult, CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract DiagnosticResult[] TestThreadSleepInAsyncLambdaExpectedResult { get; }

        [Fact]
        public async Task TestThreadSleepInAsyncLambdaAsync()
        {
            string testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public void BadExample()
    {
        Func<Task> testFunc = async () =>
        {
            Thread.Sleep(1);
            await Task.FromResult(1);
        };
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInAsyncLambdaExpectedResult, CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract DiagnosticResult[] TestThreadSleepInAsyncAnonymousMethodExpectedResult { get; }

        [Fact]
        public async Task TestThreadSleepInAsyncAnonymousMethodAsync()
        {
            string testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate Task<int> SampleDelegate();
    SampleDelegate AnonymousMethod1 = async delegate ()
    {
        Thread.Sleep(0);
        return await Task.FromResult(0);
    };
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInAsyncAnonymousMethodExpectedResult, CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract DiagnosticResult[] TestThreadSleepInAnonymousMethodExpectedResult { get; }

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

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInAnonymousMethodExpectedResult, CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract DiagnosticResult[] TestThreadSleepStaticImportExpectedResult { get; }

        [Fact]
        public async Task TestThreadSleepStaticImportInAsyncMethodAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Thread;

class ClassA
{
    public async Task<int> Method1Async()
    {
        Sleep(1000);
        
        return await Task.FromResult(0); 
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepStaticImportExpectedResult, CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract DiagnosticResult[] TestThreadSleepInMethod { get; }

        [Fact]
        public async Task TestThreadSleepInMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
using System.Threading;

class ClassA
{
    public void Method1Async()
    {
        Thread.Sleep(1000);
        System.Threading.Thread.Sleep(1000);
        global::System.Threading.Thread.Sleep(1000);
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInMethod, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestUsingTaskDelayInSimpleAsynMethodIsOKAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
using System.Threading;

class ClassA
{
    public async Task<int> Method1Async()
    {
        await Task.Delay(1000);
        return await Task.FromResult(0); 
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DontUseThreadSleepCodeUniversalCodeFixProvider();
        }
    }
}
