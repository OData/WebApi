using Microsoft.AspNet.OData.Formatters;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;
using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class ODataServiceCollectionExtensions
    {
        public static ODataServiceBuilder AddOData(
            [NotNull] this IServiceCollection services)
        {
            services.AddScoped<ODataProperties>();
            services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();
            services.ConfigureMvc(options =>
            {
                // use descriptor?
                options.OutputFormatters.Insert(0, new ODataOutputFormatter());
            });

            services.AddSingleton<IActionSelector, ODataActionSelector>();
            services.AddSingleton<IODataRoutingConvention, DefaultODataRoutingConvention>();
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
