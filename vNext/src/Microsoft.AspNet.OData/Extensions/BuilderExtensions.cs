using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
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

        public static IApplicationBuilder UseOData<T>([NotNull] this IApplicationBuilder app, string prefix) where T : class
        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);

            return app.UseRouter(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T))));
        }

        public static IApplicationBuilder InitializeODataBuilder([NotNull] this IApplicationBuilder app)        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);

            return app;
        }
    }
}
