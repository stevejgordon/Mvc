// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageLoader : IPageLoader
    {
        private const string PageImportsFileName = "_PageImports.cshtml";
        private readonly RazorEngine _razorEngine;
        private readonly RazorProject _razorProject;
        private readonly ICompilationService _compilationService;
        private readonly ICompilerCacheProvider _compilerCacheProvider;
        private readonly Func<string, CompilationResultSource> _getCompilationResultSourceDelegate;
        private readonly Func<MvcRazorCompilation, CompilationResult> _getCompilationResultDelegate;
        private readonly ILogger _logger;
        private ICompilerCache _compilerCache;

        public DefaultPageLoader(
            RazorEngine razorEngine,
            RazorProject razorProject,
            ICompilationService compilationService,
            ICompilerCacheProvider compilerCacheProvider,
            ILogger<DefaultPageLoader> logger)
        {
            _razorEngine = razorEngine;
            _razorProject = razorProject;
            _compilationService = compilationService;
            _compilerCacheProvider = compilerCacheProvider;
            _getCompilationResultSourceDelegate = GetCompilationSource;
            _getCompilationResultDelegate = GetCompilationResult;
            _logger = logger;
        }

        private ICompilerCache CompilerCache
        {
            get
            {
                if (_compilerCache == null)
                {
                    _compilerCache = _compilerCacheProvider.Cache;
                }

                return _compilerCache;
            }
        }

        public Type Load(PageActionDescriptor actionDescriptor)
        {
            var item = _razorProject.GetItem(actionDescriptor.RelativePath);
            if (!item.Exists)
            {
                throw new InvalidOperationException($"File {actionDescriptor.RelativePath} was not found.");
            }

            var cacheResult = CompilerCache.GetOrAdd(actionDescriptor.RelativePath, _getCompilationResultSourceDelegate);
            return cacheResult.CompiledType;
        }

        private CompilationResultSource GetCompilationSource(string path)
        {
            var projectItem = _razorProject.GetItem(path);
            var viewStartItems = _razorProject.FindHierarchicalItems(path, PageImportsFileName);
            var compilation = new MvcRazorCompilation(_razorEngine, projectItem, viewStartItems);
            return new CompilationResultSource(compilation, GetCompilationResult);
        }

        private CompilationResult GetCompilationResult(MvcRazorCompilation razorCompilation)
        {
            var projectItem = razorCompilation.ProjectItem;
            var codeDocument = razorCompilation.CreateCodeDocument();
            var csharpDocument = razorCompilation.CreateCSharpDocument(codeDocument);

            CompilationResult compilationResult;
            if (csharpDocument.Diagnostics.Count > 0)
            {
                compilationResult = CompilationResultFactory.FromRazorErrors(
                    _razorProject, projectItem.Path,
                    csharpDocument.Diagnostics);
            }
            else
            {
                compilationResult = _compilationService.Compile(codeDocument, csharpDocument);
            }

            return compilationResult;
        }
    }
}