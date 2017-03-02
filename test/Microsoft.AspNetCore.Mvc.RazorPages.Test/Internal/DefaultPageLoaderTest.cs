// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageLoaderTest
    {
        [Fact]
        public void Load_SetsPageAndModel()
        {
            // Arrange
            var actionDescriptor = new Mock<PageActionDescriptor>();
            var loader = GetPageLoader();

            // Act
            var compiledPageActionDescriptor = loader.Load(actionDescriptor.Object);
            
            // Assert
            throw new NotImplementedException();
        }

        private static RazorProjectItem GetProjectItem(string basePath, string path, string content)
        {
            var testFileInfo = new TestFileInfo
            {
                Content = content,
            };

            return new DefaultRazorProjectItem(testFileInfo, basePath, path);
        }

        private static RazorCompilationService GetRazorCompilationService(ICompilationService compilationService)
        {
            var engine = new Mock<RazorEngine>();
            var project = new Mock<RazorProject>();
            var fileProviderAccessor = new Mock<IRazorViewEngineFileProviderAccessor>();
            var loggerFactory = new Mock<ILoggerFactory>();

            return new RazorCompilationService(compilationService, engine.Object, project.Object, fileProviderAccessor.Object, loggerFactory.Object);
        }

        private DefaultPageLoader GetPageLoader()
        {
            var compilationService = new Mock<ICompilationService>();
            var razorCompilationService = GetRazorCompilationService(compilationService.Object);
            var razorProjectItem = GetProjectItem("/", "/Test.cshtml", @"@page ""url/here""");

            var razorProject = new Mock<RazorProject>();
            razorProject.Setup(p => p.GetItem(It.IsAny<string>())).Returns(
                razorProjectItem);
            var logger = new Mock<ILogger<DefaultPageLoader>>();

            return new DefaultPageLoader(
                razorCompilationService,
                compilationService.Object,
                razorProject.Object,
                logger.Object);
        }

        private class Page
        {
            public Model Model { get; }
        }

        private class Model
        {

        }
    }
}
