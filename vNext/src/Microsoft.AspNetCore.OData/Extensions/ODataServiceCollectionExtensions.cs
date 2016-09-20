// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods to add odata services.
    /// </summary>
    public static class ODataServiceCollectionExtensions
    {
        /// <summary>
        /// Adds essential OData services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>An <see cref="IODataCoreBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IODataCoreBuilder AddOData(this IServiceCollection services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            // Options
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());

            services.AddScoped<ODataProperties>();

            //services.AddTransient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>();

            // SerializerProvider
            services.AddSingleton<IODataSerializerProvider, DefaultODataSerializerProvider>();


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
            // services.AddSingleton<IODataRoutingConvention, DefaultODataRoutingConvention>();
            services.AddSingleton<IETagHandler, DefaultODataETagHandler>();

            // Routing Conventions
            // services.AddDefaultRoutingConventions();

            // Routing
            services.AddSingleton<IODataPathHandler, DefaultODataPathHandler>();
            services.AddSingleton<IODataPathTemplateHandler, DefaultODataPathHandler>();

            // Assembly
            services.AddSingleton<IAssemblyProvider, DefaultAssemblyProvider>();

            // Serializers
            AddDefaultSerializers(services);

            // Deserializers
            AddDefaultDeserializers(services);

            return new ODataCoreBuilder(services);
        }

        /// <summary>
        /// Adds essential OData services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{MvcOptions}"/> to configure the provided <see cref="ODataOptions"/>.</param>
        /// <returns>An <see cref="IODataCoreBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IODataCoreBuilder AddOData([NotNull] this IServiceCollection services,
            Action<ODataOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            IODataCoreBuilder builder = services.AddOData();
            builder.Services.Configure(setupAction);
            return builder;
        }

        public static IODataCoreBuilder AddOData([NotNull] this IServiceCollection services,
            Action<IServiceCollection> configSerivces)
        {
            IODataCoreBuilder builder = services.AddOData();
            configSerivces(services); // for customers override services
            return builder;
        }

        public static void AddApiContext<T>(
           [NotNull] this ODataCoreBuilder builder,
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

        private static IServiceCollection AddDefaultRoutingConventions(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, AttributeRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, MetadataRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, EntitySetRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, SingletonRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, EntityRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, NavigationRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, PropertyRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, DynamicPropertyRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, RefRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, ActionRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, FunctionRoutingConvention>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IODataRoutingConvention, UnmappedRequestRoutingConvention>());

            return services;
        }

        internal static void AddODataCoreServices(this IServiceCollection services)
        {
            
        }

        private static void AddDefaultSerializers(IServiceCollection services)
        {
            services.AddSingleton<ODataMetadataSerializer>();
            services.AddSingleton<ODataServiceDocumentSerializer>();

            services.AddSingleton<ODataEnumSerializer>();
            services.AddSingleton<ODataPrimitiveSerializer>();
            services.AddSingleton<ODataDeltaFeedSerializer>();
            services.AddSingleton<ODataResourceSetSerializer>();
            services.AddSingleton<ODataCollectionSerializer>();
            services.AddSingleton<ODataResourceSerializer>();
            services.AddSingleton<ODataServiceDocumentSerializer>();
            services.AddSingleton<ODataEntityReferenceLinkSerializer>();
            services.AddSingleton<ODataEntityReferenceLinksSerializer>();
            services.AddSingleton<ODataErrorSerializer>();
            services.AddSingleton<ODataRawValueSerializer>();
        }

        private static void AddDefaultDeserializers(IServiceCollection services)
        {
        }
    }
}
