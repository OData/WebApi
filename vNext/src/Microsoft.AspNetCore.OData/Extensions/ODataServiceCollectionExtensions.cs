using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNetCore.OData.Extensions
{
	public interface IODataOutputFormatterProvider
	{
		IOutputFormatter OutputFormatter { get; }
	}

	public class DefaultODataOutputFormatterProvider<TOutputFormatter> : IODataOutputFormatterProvider
		where TOutputFormatter : IOutputFormatter, new()
	{
		public IOutputFormatter OutputFormatter { get; } = new TOutputFormatter();
	}

	public static class ODataServiceCollectionExtensions
	{
		internal static ODataServiceBuilder AddOData([NotNull] this IServiceCollection services)

		{
			services.AddScoped<ODataProperties>();
			services.AddTransient<ODataOptionsSetup>();
			var currentServices = services.BuildServiceProvider();
			if (currentServices.GetService<IODataOutputFormatterProvider>() == null)
			{
				services.ConfigureODataOutputFormatter<ModernOutputFormatter>();
			}
			if (currentServices.GetService<ODataSerializerProvider>() == null)
			{
				services.ConfigureODataSerializerProvider<DefaultODataSerializerProvider>();
			}
			//            services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();
			services.Configure<MvcOptions>(options =>
			{
				options.InputFormatters.Insert(0, new ModernInputFormatter());

				var serviceProvider = services.BuildServiceProvider();
				var outputFormatters = ODataOutputFormatters.Create(
					serviceProvider.GetService<DefaultODataSerializerProvider>());
				foreach (var odataOutputFormatter in outputFormatters)
				{
					options.OutputFormatters.Insert(0, odataOutputFormatter);
				}
				options.OutputFormatters.Insert(0, serviceProvider.GetService<IODataOutputFormatterProvider>().OutputFormatter);
			});

			//services.AddSingleton<IActionSelector, ODataActionSelector>();
			services.AddSingleton<IActionSelector, ODataActionSelector>();
			services.AddSingleton<IODataRoutingConvention, DefaultODataRoutingConvention>();
			services.AddSingleton<IETagHandler, DefaultODataETagHandler>();
			services.AddSingleton<IODataPathHandler, DefaultODataPathHandler>();
			return new ODataServiceBuilder(services);
		}

		//public static void AddApiContext<T>(
		//   [NotNull] this ODataServiceBuilder builder,
		//   [NotNull] string prefix)
		//	where T : class
		//{
		//	builder.Register<T>(prefix);
		//}

		public static void ConfigureOData(
			[NotNull] this IServiceCollection services,
			[NotNull] Action<ODataOptions> setupAction)
		{
			services.Configure(setupAction);
		}
	}
}
