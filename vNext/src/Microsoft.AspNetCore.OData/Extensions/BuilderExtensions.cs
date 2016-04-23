using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class BuilderExtensions
    {
        //public static IRouteBuilder MapODataRoute<T>(this IRouteBuilder builder, string prefix) where T : class
        //{
        //    builder.Routes.Add(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T))));
        //    return builder;
        //}

	    public static IServiceCollection AddOData<T>([NotNull] this IServiceCollection services,
			Action<ODataConventionModelBuilder> after = null)
			where T : class
	    {
		    services.AddOData();
			var type = typeof(T);
			var assemblyNames = new AssemblyNames(type.GetTypeInfo().Assembly.FullName);
			var model = DefaultODataModelProvider.BuildEdmModel(type, assemblyNames, after);
		    services.AddSingleton(model);
		    services.AddSingleton(assemblyNames);
			return services;
	    }

	    public static IApplicationBuilder UseOData(
            [NotNull] this IApplicationBuilder app, 
            string prefix
            ) 
        {
            //var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            //AssemblyProviderManager.Register(defaultAssemblyProvider);
			var router = new ODataRoute(
				prefix,
				app.ApplicationServices.GetService<IEdmModel>());
		    ODataRoute.Instance = router;
			return app.UseRouter(router);
        }
    }
}
