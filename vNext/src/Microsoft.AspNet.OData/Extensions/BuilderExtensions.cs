using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Routing;
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

        public static IApplicationBuilder UseOData<T>(
            [NotNull] this IApplicationBuilder app, 
            string prefix,
            Action<ODataConventionModelBuilder> after = null) where T : class
        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);

            return app.UseRouter(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T), after)));
        }
    }
}
