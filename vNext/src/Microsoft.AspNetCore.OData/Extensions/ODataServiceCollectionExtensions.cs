// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods to add odata services.
    /// </summary>
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

                foreach (var outputFormatter in ODataOutputFormatters.Create(/*services.BuildServiceProvider()*/))
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }
                //options.OutputFormatters.Insert(0, new ModernOutputFormatter());
            });

            services.AddSingleton<IActionSelector, ODataActionSelector>();
            services.AddSingleton<IODataRoutingConvention, DefaultODataRoutingConvention>();
            services.AddSingleton<IETagHandler, DefaultODataETagHandler>();

            // Routing
            services.AddSingleton<IODataPathHandler, DefaultODataPathHandler>();

            // Assembly
            services.AddSingleton<IAssemblyProvider, DefaultAssemblyProvider>();

            // Serializer
            services.AddSingleton<ODataMetadataSerializer>();

            return new ODataServiceBuilder(services);
        }

        public static ODataServiceBuilder AddOData([NotNull] this IServiceCollection services,
            Action<IServiceCollection> configSerivces)
        {
            ODataServiceBuilder builder = services.AddOData();
            configSerivces(services); // for customers override services
            return builder;
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
