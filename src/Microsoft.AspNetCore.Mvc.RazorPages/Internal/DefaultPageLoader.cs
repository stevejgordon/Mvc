// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageLoader : IPageLoader
    {
        private readonly MvcRazorTemplateEngine _templateEngine;
        private readonly ICompilationService _compilationService;
        private readonly ICompilerCacheProvider _compilerCacheProvider;
        private readonly Func<string, CompilerCacheContext> _getCacheContext;
        private readonly Func<CompilerCacheContext, CompilationResult> _getCompilationResultDelegate;
        private readonly ILogger _logger;
        private ICompilerCache _compilerCache;

        public DefaultPageLoader(
            MvcRazorTemplateEngine templateEngine,
            ICompilationService compilationService,
            ICompilerCacheProvider compilerCacheProvider,
            ILogger<DefaultPageLoader> logger)
        {
            _templateEngine = templateEngine;
            _compilationService = compilationService;
            _compilerCacheProvider = compilerCacheProvider;
            _getCacheContext = GetCacheContext;
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
            var item = _templateEngine.Project.GetItem(actionDescriptor.RelativePath);
            if (!item.Exists)
            {
                throw new InvalidOperationException($"File {actionDescriptor.RelativePath} was not found.");
            }

            var cacheResult = CompilerCache.GetOrAdd(actionDescriptor.RelativePath, _getCacheContext);
            return cacheResult.CompiledType;
        }

        private CompilerCacheContext GetCacheContext(string path)
        {
            var item = _templateEngine.Project.GetItem(path);
            var imports = _templateEngine.Project.FindHierarchicalItems(
                path,
                MvcRazorTemplateEngine.RazorPagesTemplateEngineOptions.ImportsFileName);

            return new CompilerCacheContext(item, imports, GetCompilationResult);
        }

        private CompilationResult GetCompilationResult(CompilerCacheContext cacheContext)
        {
            var projectItem = cacheContext.ProjectItem;
            var templateEngineResult = _templateEngine.GenerateCode(projectItem.Path, MvcRazorTemplateEngine.RazorPagesTemplateEngineOptions);
            var csharpDocument = templateEngineResult.CSharpDocument;

            CompilationResult compilationResult;
            if (csharpDocument.Diagnostics.Count > 0)
            {
                compilationResult = CompilationResultFactory.FromRazorErrors(
                    _templateEngine.Project,
                    projectItem.Path,
                    csharpDocument.Diagnostics);
            }
            else
            {
                compilationResult = _compilationService.Compile(templateEngineResult);
            }

            return compilationResult;
        }
    }
}