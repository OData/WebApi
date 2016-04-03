using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Framework.Internal;
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

        public static IApplicationBuilder UseOData<T>(
            [NotNull] this IApplicationBuilder app, 
            string prefix,
            Action<ODataConventionModelBuilder> after = null) where T : class
        {
            //var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            //AssemblyProviderManager.Register(defaultAssemblyProvider);
	        var type = typeof (T);

			var model = DefaultODataModelProvider.BuildEdmModel(type, after);

			var router = new ODataRoute(prefix, model);

			return app.UseRouter(router);
        }
    }
}
