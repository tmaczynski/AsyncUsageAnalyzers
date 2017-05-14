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

    public class PropagateCancellationTokenUnitTests : DiagnosticVerifier
    {
        [Theory]
        [InlineData("CancellationToken.None")]
        [InlineData("default(CancellationToken)")]
        public async Task TestCancellationTokenNoneRisesDiagnosticIfTheresAnotherTokenAsync(string cancellationTokenString)
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
            var testCode = testCodeTemplate.Replace("##", cancellationTokenString);

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments("CalledAsyncMethodShortAsync").WithLocation(11, 43),
                this.CSharpDiagnostic().WithArguments("CalledAsyncMethodLongAsync").WithLocation(20, 42)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("CancellationToken.None")]
        [InlineData("default(CancellationToken)")]
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

        // TODO: add TestCancellationTokenNoneDoesNotRiseDiagnosticIfTheresNoOtherCancellationTokenAsync
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new PropagateCancellationTokenAnalyzer();
        }
    }
}
