using Microsoft.AspNet.OData.Formatters;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;
using System;
using Microsoft.AspNet.Mvc;

namespace Microsoft.AspNet.OData
{
    public static class ODataServiceCollectionExtensions
    {
        public static IServiceCollection AddOData(
            [NotNull] this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();
            services.ConfigureMvc(options =>
            {
                // use descriptor?
                options.OutputFormatters.Insert(0, new ODataOutputFormatter());
            });

            return services;
        }

        public static void ConfigureOData(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<ODataOptions> setupAction)
        {
            services.Configure(setupAction);
        }
    }
}
