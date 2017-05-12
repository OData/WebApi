using Microsoft.AspNet.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Framework.Internal;
using Microsoft.Extensions.OptionsModel;
using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.Mvc.Infrastructure;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class ODataServiceCollectionExtensions
    {
        public static ODataServiceBuilder AddOData(
            [NotNull] this IServiceCollection services)
        {
            services.AddScoped<ODataProperties>();
            services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();
            services.Configure<MvcOptions>(options =>
            {
                options.InputFormatters.Insert(0, new ModernInputFormatter());

                // BUG: the options.OutputFormatters.Insert(0, new ModernOutputFormatter()); 
                //      line has been uncommented and as a result odata works without it the
                //      exception shown below in thrown
                //  
                //  An exception of type 'Microsoft.OData.Core.ODataContentTypeException' occurred in Microsoft.OData.Core.dll 
                //  but was not handled in user code Additional information: A supported MIME type could not be found that matches 
                //  the content type of the response. None of the supported type(s)  ...

                var outputFormatters = ODataOutputFormatters.Create();
                foreach (var outputFormatter in outputFormatters)
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }
                options.OutputFormatters.Insert(0, new ModernOutputFormatter());

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
