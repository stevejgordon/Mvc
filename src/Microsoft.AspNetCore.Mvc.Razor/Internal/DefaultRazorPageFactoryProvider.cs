// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Represents a <see cref="IRazorPageFactoryProvider"/> that creates <see cref="RazorPage"/> instances
    /// from razor files in the file system.
    /// </summary>
    public class DefaultRazorPageFactoryProvider : IRazorPageFactoryProvider
    {
        private readonly MvcRazorTemplateEngine _templateEngine;
        private readonly ICompilationService _compilationService;
        private readonly ICompilerCacheProvider _compilerCacheProvider;
        private readonly Func<string, CompilerCacheContext> _getCacheContext;
        private readonly Func<CompilerCacheContext, CompilationResult> _getCompilationResultDelegate;

        private ICompilerCache _compilerCache;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultRazorPageFactoryProvider"/>.
        /// </summary>
        /// <param name="templateEngine">The <see cref="MvcRazorTemplateEngine"/>.</param>
        /// <param name="compilationService">The <see cref="ICompilationService"/>.</param>
        /// <param name="compilerCacheProvider">The <see cref="ICompilerCacheProvider"/>.</param>
        public DefaultRazorPageFactoryProvider(
            MvcRazorTemplateEngine templateEngine,
            ICompilationService compilationService,
            ICompilerCacheProvider compilerCacheProvider)
        {
            _templateEngine = templateEngine;
            _compilationService = compilationService;
            _compilerCacheProvider = compilerCacheProvider;
            _getCacheContext = GetCacheContext;
            _getCompilationResultDelegate = GetCompilationResult;
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

        /// <inheritdoc />
        public RazorPageFactoryResult CreateFactory(string relativePath)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            var result = CompilerCache.GetOrAdd(relativePath, _getCacheContext);
            if (result.Success)
            {
                var compiledType = result.CompiledType;

                var newExpression = Expression.New(compiledType);
                var pathProperty = compiledType.GetTypeInfo().GetProperty(nameof(IRazorPage.Path));

                // page.Path = relativePath;
                var propertyBindExpression = Expression.Bind(pathProperty, Expression.Constant(relativePath));
                var objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);
                var pageFactory = Expression
                    .Lambda<Func<IRazorPage>>(objectInitializeExpression)
                    .Compile();
                return new RazorPageFactoryResult(pageFactory, result.ExpirationTokens, result.IsPrecompiled);
            }
            else
            {
                return new RazorPageFactoryResult(result.ExpirationTokens);
            }
        }

        private CompilerCacheContext GetCacheContext(string path)
        {
            var item = _templateEngine.Project.GetItem(path);
            var imports = _templateEngine.Project.FindHierarchicalItems(
                path, 
                MvcRazorTemplateEngine.MvcViewsTemplateEngineOptions.ImportsFileName);

            return new CompilerCacheContext(item, imports, GetCompilationResult);
        }

        private CompilationResult GetCompilationResult(CompilerCacheContext cacheContext)
        {
            var projectItem = cacheContext.ProjectItem;
            var templateEngineResult = _templateEngine.GenerateCode(projectItem.Path, MvcRazorTemplateEngine.MvcViewsTemplateEngineOptions);
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