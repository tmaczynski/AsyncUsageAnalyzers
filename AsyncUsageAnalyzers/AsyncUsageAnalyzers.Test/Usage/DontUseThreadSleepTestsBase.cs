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
        /// <summary>
        /// Return a new diagnostic using with updated arguments or leaves a diagnostic intact.
        /// </summary>
        /// <param name="diagnostic">a diagnostic to be modified</param>
        /// <param name="arguments">arguments which can be used to update diagnostic/param>
        /// <returns></returns>
        public abstract DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments);

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
            var expectedResults = new[]
                {
                    this.CSharpDiagnostic().WithArguments(string.Format(UsageResources.MethodFormat, "Method1Async")).WithLocation(9, 9),
                    this.CSharpDiagnostic().WithArguments(string.Format(UsageResources.MethodFormat, "Method1Async")).WithLocation(10, 9),
                    this.CSharpDiagnostic().WithArguments(string.Format(UsageResources.MethodFormat, "Method1Async")).WithLocation(11, 9)
                }
                .Select(diag => this.OptionallyAddArgumentsToDiagnostic(diag, string.Format(UsageResources.MethodFormat, "Method1Async")))
                .ToArray();

            await this.VerifyCSharpDiagnosticAsync(testCode, expectedResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
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

            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public void BadExample()
    {
        Func<Task> testFunc = async () =>
        {
            await System.Threading.Tasks.Task.Delay(1);
            await Task.FromResult(1);
        };
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInAsyncLambdaExpectedResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
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

            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate Task<int> SampleDelegate();
    SampleDelegate AnonymousMethod1 = async delegate ()
    {
        await System.Threading.Tasks.Task.Delay(0);
        return await Task.FromResult(0);
    };
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepInAsyncAnonymousMethodExpectedResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
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

            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Thread;

class ClassA
{
    public async Task<int> Method1Async()
    {
        await System.Threading.Tasks.Task.Delay(1000);
        
        return await Task.FromResult(0); 
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, this.TestThreadSleepStaticImportExpectedResult, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
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
            await this.VerifyCSharpFixAsync(
                    testCode,
                    testCode /* source code should not be changed as there's no automatic code fix */,
                    cancellationToken: CancellationToken.None,
                    numberOfFixAllIterations: -this.TestThreadSleepInMethod.Length)
                .ConfigureAwait(false);
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
