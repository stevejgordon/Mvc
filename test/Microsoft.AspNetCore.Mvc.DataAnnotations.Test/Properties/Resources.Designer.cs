// <auto-generated />
namespace Microsoft.AspNetCore.Mvc.DataAnnotations.Test
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNetCore.Mvc.DataAnnotations.Test.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// Comparing {0} to {1}.
        /// </summary>
        internal static string CompareAttributeTestResource
        {
            get { return GetString("CompareAttributeTestResource"); }
        }

        /// <summary>
        /// Comparing {0} to {1}.
        /// </summary>
        internal static string FormatCompareAttributeTestResource(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("CompareAttributeTestResource"), p0, p1);
        }

        /// <summary>
        /// description from resources
        /// </summary>
        internal static string DisplayAttribute_Description
        {
            get { return GetString("DisplayAttribute_Description"); }
        }

        /// <summary>
        /// description from resources
        /// </summary>
        internal static string FormatDisplayAttribute_Description()
        {
            return GetString("DisplayAttribute_Description");
        }

        /// <summary>
        /// name from resources
        /// </summary>
        internal static string DisplayAttribute_Name
        {
            get { return GetString("DisplayAttribute_Name"); }
        }

        /// <summary>
        /// name from resources
        /// </summary>
        internal static string FormatDisplayAttribute_Name()
        {
            return GetString("DisplayAttribute_Name");
        }

        /// <summary>
        /// prompt from resources
        /// </summary>
        internal static string DisplayAttribute_Prompt
        {
            get { return GetString("DisplayAttribute_Prompt"); }
        }

        /// <summary>
        /// prompt from resources
        /// </summary>
        internal static string FormatDisplayAttribute_Prompt()
        {
            return GetString("DisplayAttribute_Prompt");
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            System.Diagnostics.Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
