using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.Routing;


namespace Microsoft.AspNet.OData
{
    public static class BuilderExtensions
    {
        public static IRouteBuilder MapODataRoute<T>(this IRouteBuilder builder, string prefix) where T : class
        {
            builder.Routes.Add(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T))));
            return builder;
        }
    }
}
