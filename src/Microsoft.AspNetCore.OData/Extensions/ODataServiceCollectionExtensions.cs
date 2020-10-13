// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNet.OData.Extensions
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
        /// <returns>An <see cref="IODataBuilder"/> that can be used to further configure the OData services.</returns>
        public static IODataBuilder AddOData(this IServiceCollection services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            // Setup per-route dependency injection. When routes are added, additional
            // per-route classes will be injected, such as IEdmModel and IODataRoutingConventions.
            services.AddSingleton<IPerRouteContainer, PerRouteContainer>();

            // Add OData and query options. Opting not to use IConfigurationOptions in favor of
            // fluent extensions APIs to IRouteBuilder.
            services.AddSingleton<ODataOptions>();
            services.AddSingleton<DefaultQuerySettings>();

            // Add the batch path mapping class to store batch route names and prefixes.
            services.AddSingleton<ODataBatchPathMapping>();

            // Configure MvcCore to use formatters. The OData formatters do go into the global service
            // provider and get picked up by the AspNetCore MVC framework. However, they ignore non-OData
            // requests so they won't be used for non-OData formatting.
            services.AddMvcCore(options =>
            {
                // Add OData input formatters at index 0, which overrides the built-in json and xml formatters.
                // Add in reverse order at index 0 to preserve order from the factory in the final list.
                foreach (ODataInputFormatter inputFormatter in ODataInputFormatterFactory.Create().Reverse())
                {
                    options.InputFormatters.Insert(0, inputFormatter);
                }

                // Add OData output formatters at index 0, which overrides the built-in json and xml formatters.
                // Add in reverse order at index 0 to preserve order from the factory in the final list.
                foreach (ODataOutputFormatter outputFormatter in ODataOutputFormatterFactory.Create().Reverse())
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }

                // Add the value provider.
                options.ValueProviderFactories.Insert(0, new ODataValueProviderFactory());
            });

#if NETSTANDARD2_0
            // Add our action selector. The ODataActionSelector creates an ActionSelector in it's constructor
            // and pass all non-OData calls to this inner selector.
            services.AddSingleton<IActionSelector, ODataActionSelector>();
#else
            // We need to decorate the ActionSelector.
            var selector = services.First(s => s.ServiceType == typeof(IActionSelector) && s.ImplementationType != null);
            services.Remove(selector);
            services.Add(new ServiceDescriptor(selector.ImplementationType, selector.ImplementationType, ServiceLifetime.Singleton));

            // Add our action selector. The ODataActionSelector creates an ActionSelector in it's constructor
            // and pass all non-OData calls to this inner selector.
            services.AddSingleton<IActionSelector>(s =>
            {
                return new ODataActionSelector(
                    (IActionSelector)s.GetRequiredService(selector.ImplementationType),
                    (IModelBinderFactory)s.GetRequiredService(typeof(IModelBinderFactory)),
                    (IModelMetadataProvider)s.GetRequiredService(typeof(IModelMetadataProvider)));
            });

            services.AddSingleton<ODataEndpointRouteValueTransformer>();

            // OData Endpoint selector policy
            services.AddSingleton<MatcherPolicy, ODataEndpointSelectorPolicy>();

            // LinkGenerator
            var linkGenerator = services.First(s => s.ServiceType == typeof(LinkGenerator) && s.ImplementationType != null);
            services.Remove(linkGenerator);
            services.Add(new ServiceDescriptor(linkGenerator.ImplementationType, linkGenerator.ImplementationType, ServiceLifetime.Singleton));

            services.AddSingleton<LinkGenerator>(s =>
            {
                return new ODataEndpointLinkGenerator((LinkGenerator)s.GetRequiredService(linkGenerator.ImplementationType));
            });
#endif

            // Add the ActionContextAccessor; this allows access to the ActionContext which is needed
            // during the formatting process to construct a IUrlHelper.
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            return new ODataBuilder(services);
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="services">The services collection.</param>
        public static IServiceCollection AddODataQueryFilter(this IServiceCollection services)
        {
            return AddODataQueryFilter(services, new EnableQueryAttribute());
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public static IServiceCollection AddODataQueryFilter(this IServiceCollection services, IActionFilter queryFilter)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterProvider>(new QueryFilterProvider(queryFilter)));
            return services;
        }
    }
}
