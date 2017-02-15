// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Evolution;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public class MvcRazorCompilation : RazorCompilation
    {
        private static readonly RazorSourceDocument DefaultImports = GetDefaultImports();

        public MvcRazorCompilation(
            RazorEngine engine,
            RazorProjectItem projectItem,
            IEnumerable<RazorProjectItem> imports)
            : base(engine, projectItem)
        {
            Imports = imports;
        }

        public RazorSourceDocument GlobalImports { get; set; } = DefaultImports;

        public IEnumerable<RazorProjectItem> Imports { get; }

        public override RazorCodeDocument CreateCodeDocument()
        {
            var codeDocument = base.CreateCodeDocument();
            codeDocument.SetRelativePath(ProjectItem.Path);
            return codeDocument;
        }

        protected override IEnumerable<RazorSourceDocument> GetImports()
        {
            var imports = new List<RazorSourceDocument>()
            {
                GlobalImports,
            };

            foreach (var item in Imports)
            {
                if (item.Exists)
                {
                    imports.Add(CreateSourceDocument(item));
                }
            }

            return imports;
        }

        private static RazorSourceDocument GetDefaultImports()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine("@using System");
                writer.WriteLine("@using System.Linq");
                writer.WriteLine("@using System.Collections.Generic");
                writer.WriteLine("@using Microsoft.AspNetCore.Mvc");
                writer.WriteLine("@using Microsoft.AspNetCore.Mvc.Rendering");
                writer.WriteLine("@using Microsoft.AspNetCore.Mvc.ViewFeatures");
                writer.WriteLine("@inject Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> Html");
                writer.WriteLine("@inject Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json");
                writer.WriteLine("@inject Microsoft.AspNetCore.Mvc.IViewComponentHelper Component");
                writer.WriteLine("@inject Microsoft.AspNetCore.Mvc.IUrlHelper Url");
                writer.WriteLine("@inject Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider");
                writer.WriteLine("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper, Microsoft.AspNetCore.Mvc.Razor");
                writer.Flush();

                stream.Position = 0;
                return RazorSourceDocument.ReadFrom(stream, filename: null, encoding: Encoding.UTF8);
            }
        }
    }
}
