// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Usage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Usage;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class PropagateCancellationTokenUnitTests : CodeFixVerifier
    {
        public static IEnumerable<object[]> CancelationTokenNoneEquivalentsWithDiagnosticParameters
        {
            get
            {
                yield return new[] { "CancellationToken.None", UsageResources.CancellationTokenNone };
                yield return new[] { "default(CancellationToken)", UsageResources.DefaultCancellationToken };
            }
        }

        public static IEnumerable<object[]> CancelationTokenNoneEquivalents
            => CancelationTokenNoneEquivalentsWithDiagnosticParameters.Select(x => new[] { x.First() });

        [Theory]
        [MemberData(nameof(CancelationTokenNoneEquivalentsWithDiagnosticParameters))]
        public async Task TestCancellationTokenNoneRisesDiagnosticIfTheresAnotherTokenInParameterAsync(string cancellationTokenString, string diagnosticParameter)
        {
            var testCodeTemplate = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    async Task<int> CalledAsyncMethodShortAsync(CancellationToken ct) =>
        await Task.FromResult(0);

    async Task CallingAsyncMethodShortAsync(CancellationToken ct) =>
        await CalledAsyncMethodShortAsync(##);

    async Task<int> CalledAsyncMethodLongAsync(CancellationToken ct)
    {
        return await Task.FromResult(0);
    }

    async Task CallingAsyncMethodLongAsync(CancellationToken ct)
    {
        await CalledAsyncMethodLongAsync(##);
    }
}";

            var fixedCode = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    async Task<int> CalledAsyncMethodShortAsync(CancellationToken ct) =>
        await Task.FromResult(0);

    async Task CallingAsyncMethodShortAsync(CancellationToken ct) =>
        await CalledAsyncMethodShortAsync(ct);

    async Task<int> CalledAsyncMethodLongAsync(CancellationToken ct)
    {
        return await Task.FromResult(0);
    }

    async Task CallingAsyncMethodLongAsync(CancellationToken ct)
    {
        await CalledAsyncMethodLongAsync(ct);
    }
}";

            var testCode = testCodeTemplate.Replace("##", cancellationTokenString);

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments(diagnosticParameter, "CalledAsyncMethodShortAsync").WithLocation(11, 43),
                this.CSharpDiagnostic().WithArguments(diagnosticParameter, "CalledAsyncMethodLongAsync").WithLocation(20, 42)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CancelationTokenNoneEquivalentsWithDiagnosticParameters))]
        public async Task TestCancellationTokenNoneRisesDiagnosticIfTheresAnotherTokenAsLocalVariableAsync(string cancellationTokenString, string diagnosticParameter)
        {
            var testCodeTemplate = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    async Task<int> CalledAsyncMethodAsync(CancellationToken ct)
    {
        return await Task.FromResult(0);
    }

    async Task CallingAsyncMethodAsync()
    {
        var ct = CancellationToken.None;
        await CalledAsyncMethodAsync(##);
    }
}";

            var fixedCode = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    async Task<int> CalledAsyncMethodAsync(CancellationToken ct)
    {
        return await Task.FromResult(0);
    }

    async Task CallingAsyncMethodAsync()
    {
        var ct = CancellationToken.None;
        await CalledAsyncMethodAsync(ct);
    }
}";

            var testCode = testCodeTemplate.Replace("##", cancellationTokenString);

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments(diagnosticParameter, "CalledAsyncMethodAsync").WithLocation(15, 38),
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CancelationTokenNoneEquivalents))]
        public async Task TestCancellationTokenNoneDoesNotRiseDiagnosticIfTheresNoOtherCancellationTokenAsync(string cancellationTokenString)
        {
            var testCodeTemplate = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    async Task<int> CalledAsyncMethodShortAsync(CancellationToken ct) =>
        await Task.FromResult(0);

    async Task CallingAsyncMethodShortAsync() =>
        await CalledAsyncMethodShortAsync(##);

    async Task<int> CalledAsyncMethodLongAsync(CancellationToken ct)
    {
        return await Task.FromResult(0);
    }

    async Task CallingAsyncMethodLongAsync()
    {
        await CalledAsyncMethodLongAsync(##);
    }
}";
            var testCode = testCodeTemplate.Replace("##", cancellationTokenString);

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CancelationTokenNoneEquivalents))]
        public async Task TestCancellationTokenNoneDoesNotRiseDiagnosticIfTheresNoOtherCancellationTokenAndOtherVariablesAndFieldsAreInScopeAsync(string cancellationTokenString)
        {
            var testCodeTemplate = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class C
{
    String s = null;

    async Task<int> CalledAsyncMethodShortAsync(CancellationToken ct) =>
        await Task.FromResult(0);

    async Task CallingAsyncMethodShortAsync(int n) =>
        await CalledAsyncMethodShortAsync(##);

    async Task<int> CalledAsyncMethodLongAsync(CancellationToken ct)
    {
        return await Task.FromResult(0);
    }

    async Task CallingAsyncMethodLongAsync(int n)
    {
        await CalledAsyncMethodLongAsync(##);
    }
}";
            var testCode = testCodeTemplate.Replace("##", cancellationTokenString);

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CancelationTokenNoneEquivalents))]
        public async Task TestCancellationTokenNoneIsUsedNotAsArgumentAsync(string cancellationTokenString)
        {
            var testCodeTemplate = @"
using System.Threading;

class C
{
    CancellationToken cancellationTokenField = ##;

    CancellationToken cancellationTokenProperty => ##;

    void M1()
    {
        var cancellationTokenVariable = ##;
    }
}";
            var testCode = testCodeTemplate.Replace("##", cancellationTokenString);

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestDefaultCancellationTokenCanBeUsedAsOptionalArgumentAsync()
        {
            var testCode = @"
using System.Threading;

class C
{
    void M(CancellationToken ct = default(CancellationToken))
    { 
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new PropagateCancellationTokenAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PropagateCancellationTokenCodeFixProvider();
        }
    }
}
