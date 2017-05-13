// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Usage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Diagnostics;
    using AsyncUsageAnalyzers.Usage;
    using TestHelper;
    using Microsoft.CodeAnalysis.CodeFixes;

    internal class PropagateCancellationTokenUnitTests : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new PropagateCancellationTokenAnalyzer();
        }
    }
}
