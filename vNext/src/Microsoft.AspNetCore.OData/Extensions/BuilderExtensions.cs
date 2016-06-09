using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Extensions
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
