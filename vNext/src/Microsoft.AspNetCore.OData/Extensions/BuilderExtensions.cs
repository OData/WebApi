using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class BuilderExtensions
    {
	    public static IServiceCollection ConfigureODataOutputFormatter<TOutputFormatter>(
			[NotNull] this IServiceCollection services)
		    where TOutputFormatter : class, IOutputFormatter, new()
	    {
		    return
			    services.ConfigureODataOutputFormatterProvider<DefaultODataOutputFormatterProvider<TOutputFormatter>>();
	    }

		public static IServiceCollection ConfigureODataOutputFormatterProvider<TOutputFormatterProvider>(
			[NotNull] this IServiceCollection services)
		    where TOutputFormatterProvider : class, IODataOutputFormatterProvider
		{
			return services.AddTransient<IODataOutputFormatterProvider, TOutputFormatterProvider>();
	    }

		public static IServiceCollection ConfigureODataOutputFormatterProvider(
			[NotNull] this IServiceCollection services,
			Func<IServiceProvider, IODataOutputFormatterProvider> resolver)
		{
			return services.AddTransient(resolver);
	    }

		public static IServiceCollection ConfigureODataSerializerProvider<TODataSerializerProvider>(
			[NotNull] this IServiceCollection services)
		    where TODataSerializerProvider : ODataSerializerProvider
		{
			return services.AddTransient<ODataSerializerProvider, TODataSerializerProvider>();
		}

		public static IServiceCollection ConfigureODataSerializerProvider(
			[NotNull] this IServiceCollection services,
			Func<IServiceProvider, ODataSerializerProvider> provider)
		{
			return services.AddTransient(provider);
		}

		public static IServiceCollection AddOData<TODataService>(
			[NotNull] this IServiceCollection services,
			Action<ODataConventionModelBuilder> after = null
			)
			where TODataService : class 
	    {
			services.AddOData();
			var type = typeof(TODataService);
			var assemblyNames = new AssembliesResolver(type.GetTypeInfo().Assembly);
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
