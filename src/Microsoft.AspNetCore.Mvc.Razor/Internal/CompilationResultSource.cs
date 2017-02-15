// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public struct CompilationResultSource
    {
        public CompilationResultSource(
            MvcRazorCompilation razorCompilation, 
            Func<MvcRazorCompilation, CompilationResult> compile)
        {
            RazorCompilation = razorCompilation;
            Compile = compile;
        }

        public MvcRazorCompilation RazorCompilation { get; }

        public Func<MvcRazorCompilation, CompilationResult> Compile { get; }
    }
}
