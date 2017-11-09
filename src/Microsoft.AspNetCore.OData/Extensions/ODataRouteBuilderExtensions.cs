// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this IRouteBuilder builder, string routeName,
            string routePrefix, Action<IContainerBuilder> configureAction)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            // Build and configure the root container.
            IPerRouteContainer perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            if (perRouteContainer == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            // Create an service provider for this route. Add the default services to the custom configuration actions.
            IServiceProvider serviceProvider = perRouteContainer.CreateODataRootContainer(routeName, ConfigureDefaultServices(configureAction));

            // Resolve the path handler and set URI resolver to it.
            IODataPathHandler pathHandler = serviceProvider.GetRequiredService<IODataPathHandler>();

            // If settings is not on local, use the global configuration settings.
            ODataOptions options = builder.ServiceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;
            if (pathHandler != null && pathHandler.UrlKeyDelimiter == null)
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            // Resolve some required services and create the route constraint.
            ODataPathRouteConstraint routeConstraint = new ODataPathRouteConstraint(routeName);

            // Get constraint resolver.
            IInlineConstraintResolver inlineConstraintResolver = builder
                .ServiceProvider
                .GetRequiredService<IInlineConstraintResolver>();

            // Resolve HTTP handler, create the OData route and register it.
            routePrefix = RemoveTrailingSlash(routePrefix);
            ODataRoute route = new ODataRoute(builder.DefaultHandler, routeName, routePrefix, routeConstraint, inlineConstraintResolver);
            builder.Routes.Add(route);

            return route;
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this IRouteBuilder builder, string routeName,
            string routePrefix, IEdmModel model)
        {
            return builder.MapODataServiceRoute(routeName, routePrefix, containerBuilder =>
                containerBuilder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)
                       .AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, builder)));
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this IRouteBuilder builder, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions)
        {
            return builder.MapODataServiceRoute(routeName, routePrefix, containerBuilder =>
                containerBuilder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable()));
        }

        /// <summary>
        /// Remote the trailing slash from a route prefix string.
        /// </summary>
        /// <param name="routePrefix">The route prefix string.</param>
        /// <returns>The route prefix string without a strainling slash.</returns>
        private static string RemoveTrailingSlash(string routePrefix)
        {
            if (!String.IsNullOrEmpty(routePrefix))
            {
                int prefixLastIndex = routePrefix.Length - 1;
                if (routePrefix[prefixLastIndex] == '/')
                {
                    // Remove the last trailing slash if it has one.
                    routePrefix = routePrefix.Substring(0, routePrefix.Length - 1);
                }
            }

            return routePrefix;
        }

        /// <summary>
        /// Configure the default services.
        /// </summary>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>A configuring action to add the services to the root container.</returns>
        private static Action<IContainerBuilder> ConfigureDefaultServices(Action<IContainerBuilder> configureAction)
        {
            return (builder =>
            {
                // Add platform-specific services here. Add Configuration first as other services may rely on it.
                builder.AddService<IODataPathTemplateHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);

                // Add the default webApi services.
                builder.AddDefaultWebApiServices();

                // Add custom actions.
                configureAction?.Invoke(builder);
            });
        }
    }
}
