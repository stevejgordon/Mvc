// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Evolution;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Represents a <see cref="IRazorPageFactoryProvider"/> that creates <see cref="RazorPage"/> instances
    /// from razor files in the file system.
    /// </summary>
    public class DefaultRazorPageFactoryProvider : IRazorPageFactoryProvider
    {
        private const string ViewImportsFileName = "_ViewImports.cshtml";
        private readonly RazorEngine _razorEngine;
        private readonly RazorProject _razorProject;
        private readonly ICompilationService _compilationService;
        private readonly ICompilerCacheProvider _compilerCacheProvider;
        private readonly Func<string, CompilationResultSource> _getCompilationResultSourceDelegate;
        private readonly Func<MvcRazorCompilation, CompilationResult> _getCompilationResultDelegate;

        private ICompilerCache _compilerCache;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultRazorPageFactoryProvider"/>.
        /// </summary>
        /// <param name="razorEngine">The <see cref="RazorEngine"/>.</param>
        /// <param name="razorProject">The <see cref="RazorProject"/>.</param>
        /// <param name="compilationService">The <see cref="ICompilationService"/>.</param>
        /// <param name="compilerCacheProvider">The <see cref="ICompilerCacheProvider"/>.</param>
        public DefaultRazorPageFactoryProvider(
            RazorEngine razorEngine,
            RazorProject razorProject,
            ICompilationService compilationService,
            ICompilerCacheProvider compilerCacheProvider)
        {
            _razorEngine = razorEngine;
            _razorProject = razorProject;
            _compilationService = compilationService;
            _compilerCacheProvider = compilerCacheProvider;
            _getCompilationResultSourceDelegate = GetCompilationSource;
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

            var result = CompilerCache.GetOrAdd(relativePath, _getCompilationResultSourceDelegate);
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

        private CompilationResultSource GetCompilationSource(string path)
        {
            var projectItem = _razorProject.GetItem(path);
            var viewStartItems = _razorProject.FindHierarchicalItems(path, ViewImportsFileName);
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