// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    /// <summary>
    /// A filter that saves properties with the <see cref="TempDataAttribute"/>. 
    /// </summary>
    public class SaveTempDataPropertyFilter : ISaveTempDataCallback, IActionFilter
    {
        public string Prefix { get; set; }

        public object Subject { get; set; }

        public IDictionary<PropertyInfo, object> OriginalValues { get; set; }

        private TempDataPropertyProvider _propertyProvider = new TempDataPropertyProvider();

        private readonly ITempDataDictionaryFactory _factory;

        /// <summary>
        /// Creates a new instance of <see cref="SaveTempDataPropertyFilter"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ITempDataDictionaryFactory"/>.</param>
        public SaveTempDataPropertyFilter(ITempDataDictionaryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Compares the originally saved value for the property in <see cref="ITempDataDictionary"/>
        /// to the newly assigned value; updating if necessary.
        /// </summary>
        /// <param name="tempData">The <see cref="ITempDataDictionary"/>.</param>
        public void OnTempDataSaving(ITempDataDictionary tempData)
        {
            if (Subject != null && OriginalValues != null)
            {
                foreach (var kvp in OriginalValues)
                {
                    var property = kvp.Key;
                    var originalValue = kvp.Value;

                    var newValue = property.GetValue(Subject);
                    if (newValue != null && !newValue.Equals(originalValue))
                    {
                        tempData[Prefix + property.Name] = newValue;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Prefix = TempDataPropertyProvider.Prefix;
            Subject = context.Controller;
            var tempData = _factory.GetTempData(context.HttpContext);
            OriginalValues = _propertyProvider.LoadAndTrackChanges(Subject, tempData);
        }

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}

