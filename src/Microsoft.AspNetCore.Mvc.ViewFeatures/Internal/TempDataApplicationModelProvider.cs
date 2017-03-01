// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class TempDataApplicationModelProvider : IApplicationModelProvider
    {
        /// <inheritdoc />
        public int Order { get { return -1000 + 10; } }

        /// <inheritdoc />
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        /// <inheritdoc />
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var controllerModel in context.Result.Controllers)
            {
                var properties = controllerModel.ControllerType.GetProperties();
                foreach (var property in properties)
                {
                    if (property.CustomAttributes.Select(
                        a => a.AttributeType == typeof(TempDataAttribute)).FirstOrDefault())
                    {
                        controllerModel.Filters.Add(new SaveTempDataPropertyFilterProvider());
                        break;
                    }
                }
            }
        }
    }
}
