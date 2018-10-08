using Microsoft.EntityFrameworkCore.Design;

namespace AspNetCoreODataSample.DynamicModels.Web.Utils
{
    /// <summary>
    /// A simple pluralizer which adds an "s" at the end to pluralize the terms
    /// </summary>
    public class SimplePluralizer : IPluralizer
    {
        public string Pluralize(string identifier)
        {
            if (identifier == null) return null;
            return identifier + "s";
        }

        public string Singularize(string identifier)
        {
            if (identifier == null) return null;
            return identifier.EndsWith("s") ? identifier.Substring(0, identifier.Length - 1) : identifier;
        }
    }
}
