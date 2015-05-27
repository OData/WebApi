using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;


namespace Microsoft.AspNet.OData.Extensions
{
    public static class BuilderExtensions
    {
        public static IRouteBuilder MapODataRoute<T>(this IRouteBuilder builder, string prefix) where T : class
        {
            builder.Routes.Add(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T))));
            return builder;
        }

        public static IApplicationBuilder UseOData([NotNull] this IApplicationBuilder app)
        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);
            return app;
        }
    }
}
