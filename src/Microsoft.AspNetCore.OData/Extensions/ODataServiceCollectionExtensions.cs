// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData;

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

            // Add OData options.
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<DefaultQuerySettings>, DefaultQuerySettingsSetup>());

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
        /// <param name="configuration">The server configuration.</param>
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
        /// <param name="configuration">The server configuration.</param>
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

        /// <summary>
        /// Configure the <see cref="ODataOptions" /> options.
        /// </summary>
        /// <param name="builder">The <see cref="IODataBuilder" /> to add configuration to.</param>
        /// <param name="setupAction">An <see cref="Action{ODataOptions}"/> to configure the provided <see cref="ODataOptions"/>.</param>
        /// <returns>An <see cref="IODataBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IODataBuilder ConfigureODataOptions(this IODataBuilder builder, Action<ODataOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.Services.Configure(setupAction);
            return builder;
        }

        /// <summary>
        /// Configure the <see cref="DefaultQuerySettings" /> options.
        /// </summary>
        /// <param name="builder">The <see cref="IODataBuilder" /> to add configuration to.</param>
        /// <param name="setupAction">An <see cref="Action{DefaultQuerySettings}"/> to configure the provided <see cref="DefaultQuerySettings"/>.</param>
        /// <returns>An <see cref="IODataBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IODataBuilder ConfigureDefaultQuerySettings(this IODataBuilder builder, Action<DefaultQuerySettings> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.Services.Configure(setupAction);
            return builder;
        }


        /// <summary>
        /// Get the default assembly resolver.
        /// </summary>
        /// <param name="provider">The server configuration.</param>
        internal static IWebApiAssembliesResolver GetWebApiAssembliesResolver(this IServiceProvider provider)
        {
            if (provider == null)
            {
                throw Error.ArgumentNull(nameof(provider));
            }

            ApplicationPartManager applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            return new WebApiAssembliesResolver(applicationPartManager);
        }

    }
}
