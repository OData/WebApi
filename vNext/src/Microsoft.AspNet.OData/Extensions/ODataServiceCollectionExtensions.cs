using Microsoft.AspNet.OData.Formatter;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;
using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class ODataServiceCollectionExtensions
    {
        public static ODataServiceBuilder AddOData(
            [NotNull] this IServiceCollection services)
        {
            services.AddScoped<ODataProperties>();
            services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();
            services.AddMvc(options =>
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
