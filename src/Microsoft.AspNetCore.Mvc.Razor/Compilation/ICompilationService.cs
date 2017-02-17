// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// Provides methods for compilation of a Razor page.
    /// </summary>
    public interface ICompilationService
    {
        /// <summary>
        /// Compiles a <see cref="RazorCSharpDocument"/>  and returns the result of compilation.
        /// </summary>
        /// <param name="templateEngineResult">The <see cref="RazorTemplateEngineResult"/>.</param>
        /// <returns>
        /// A <see cref="CompilationResult"/> representing the result of compilation.
        /// </returns>
        CompilationResult Compile(RazorTemplateEngineResult templateEngineResult);
    }
}
