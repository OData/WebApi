using System;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class ODataServiceCollectionExtensions
    {
        public static ODataServiceBuilder AddOData(
            [NotNull] this IServiceCollection services)
        {
            services.AddScoped<ODataProperties>();
            services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();
            services.AddMvcCore(options =>
            {
                options.InputFormatters.Insert(0, new ModernInputFormatter());

                foreach (var outputFormatter in ODataOutputFormatters.Create())
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }
                //options.OutputFormatters.Insert(0, new ModernOutputFormatter());
            });

            services.AddSingleton<IActionSelector, ODataActionSelector>();
            services.AddSingleton<IODataRoutingConvention, DefaultODataRoutingConvention>();
            services.AddSingleton<IETagHandler, DefaultODataETagHandler>();
            services.AddSingleton<IODataPathHandler, DefaultODataPathHandler>();
            return new ODataServiceBuilder(services);
        }

        public static void AddApiContext<T>(
           [NotNull] this ODataServiceBuilder builder,
           [NotNull] string prefix)
            where T : class
        {
            builder.Register<T>(prefix);
        }

        public static void ConfigureOData(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<ODataOptions> setupAction)
        {
            services.Configure(setupAction);
        }
    }
}
