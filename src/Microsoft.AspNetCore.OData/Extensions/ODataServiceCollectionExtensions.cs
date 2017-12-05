// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

            // Configure MvcCore to use formatters.
            services.AddMvcCore(options =>
            {
                // Add OData input formatters at index 0, which overrides the built-in json and xml formatters.
                foreach (ODataInputFormatter inputFormatter in ODataInputFormatterFactory.Create())
                {
                    options.InputFormatters.Insert(0, inputFormatter);
                }

                // Add OData output formatters at index 0, which overrides the built-in json and xml formatters.
                foreach (ODataOutputFormatter outputFormatter in ODataOutputFormatterFactory.Create())
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }
            });

            // Add our action selector.
            services.AddSingleton<IActionSelector, ODataActionSelector>();

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
