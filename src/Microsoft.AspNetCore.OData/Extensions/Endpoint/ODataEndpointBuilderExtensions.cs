//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNet.OData.Batch;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Sets the <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="defaultQuerySettings">The default query settings.</param>
        public static IEndpointRouteBuilder SetDefaultQuerySettings(this IEndpointRouteBuilder builder, DefaultQuerySettings defaultQuerySettings)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.SetDefaultQuerySettings(defaultQuerySettings);
            return builder;
        }

        /// <summary>
        /// Gets the <see cref="DefaultQuerySettings"/> from route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        public static DefaultQuerySettings GetDefaultQuerySettings(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            return builder.ServiceProvider.GetDefaultQuerySettings();
        }

        /// <summary>
        /// Sets the MaxTop of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder MaxTop(this IEndpointRouteBuilder builder, int? maxTopValue)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.MaxTop(maxTopValue);
            return builder;
        }

        /// <summary>
        /// Sets the EnableExpand of <see cref="DefaultQuerySettings"/> in route builder,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static IEndpointRouteBuilder Expand(this IEndpointRouteBuilder builder, QueryOptionSetting setting)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Expand(setting);
            return builder;
        }

        /// <summary>
        /// Sets the EnableExpand to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder Expand(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Expand();
            return builder;
        }

        /// <summary>
        /// Sets the SelectType of <see cref="DefaultQuerySettings"/> in route builder,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// Todo: change QueryOptionSetting to SelectExpandType.
        /// </summary>
        public static IEndpointRouteBuilder Select(this IEndpointRouteBuilder builder, QueryOptionSetting setting)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Select(setting);
            return builder;
        }

        /// <summary>
        /// Sets the EnableSelect to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder Select(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Select();
            return builder;
        }

        /// <summary>
        /// Sets the EnableFilter of <see cref="DefaultQuerySettings"/> in route builder,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static IEndpointRouteBuilder Filter(this IEndpointRouteBuilder builder, QueryOptionSetting setting)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Filter(setting);
            return builder;
        }

        /// <summary>
        /// Sets the EnableFilter to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder Filter(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Filter();
            return builder;
        }

        /// <summary>
        /// Sets the EnableOrderBy of <see cref="DefaultQuerySettings"/> in route builder,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static IEndpointRouteBuilder OrderBy(this IEndpointRouteBuilder builder, QueryOptionSetting setting)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.OrderBy(setting);
            return builder;
        }

        /// <summary>
        /// Sets the EnableOrderBy to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder OrderBy(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.OrderBy();
            return builder;
        }

        /// <summary>
        /// Sets the EnableCount of <see cref="DefaultQuerySettings"/> in route builder,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static IEndpointRouteBuilder Count(this IEndpointRouteBuilder builder, QueryOptionSetting setting)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Count(setting);
            return builder;
        }

        /// <summary>
        /// Sets the EnableCount to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder Count(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.Count();
            return builder;
        }

        /// <summary>
        /// Sets the EnableSkipToken to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder SkipToken(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.SkipToken();
            return builder;
        }

        /// <summary>
        /// Sets the EnableSkipToken to true of <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        public static IEndpointRouteBuilder SkipToken(this IEndpointRouteBuilder builder, QueryOptionSetting setting)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.SkipToken(setting);
            return builder;
        }

        /// <summary>
        /// Sets the <see cref="DefaultQuerySettings"/> in route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="defaultOptions">The default options.</param>
        public static IEndpointRouteBuilder SetDefaultODataOptions(this IEndpointRouteBuilder builder, ODataOptions defaultOptions)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.ServiceProvider.SetDefaultODataOptions(defaultOptions);
            return builder;
        }

        /// <summary>
        /// Gets the <see cref="ODataOptions"/> from route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        public static ODataOptions GetDefaultODataOptions(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            return builder.ServiceProvider.GetDefaultODataOptions();
        }

        /// <summary>
        /// Enable the continue-on-error header.
        /// </summary>
        public static IEndpointRouteBuilder EnableContinueOnErrorHeader(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            defaultOptions.EnableContinueOnErrorHeader = true;
            return builder;
        }

        /// <summary>
        /// Check the continue-on-error header is enable or not.
        /// </summary>
        /// <returns></returns>
        public static bool HasEnabledContinueOnErrorHeader(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            return defaultOptions.EnableContinueOnErrorHeader;
        }

        /// <summary>
        /// Sets whether or not the null dynamic property to be serialized.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="serialize"><c>true</c> to serialize null dynamic property, <c>false</c> otherwise.</param>
        public static IEndpointRouteBuilder SetSerializeNullDynamicProperty(this IEndpointRouteBuilder builder, bool serialize)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            defaultOptions.NullDynamicPropertyIsEnabled = serialize;
            return builder;
        }

        /// <summary>
        /// Check the null dynamic property is enable or not.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <returns>a boolean value.</returns>
        public static bool HasEnabledNullDynamicProperty(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            return defaultOptions.NullDynamicPropertyIsEnabled;
        }

        /// <summary>
        /// Set the UrlKeyDelimiter in DefaultODataPathHandler.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="urlKeyDelimiter">The <see cref="ODataUrlKeyDelimiter"/></param>
        public static IEndpointRouteBuilder SetUrlKeyDelimiter(this IEndpointRouteBuilder builder, ODataUrlKeyDelimiter urlKeyDelimiter)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            if (urlKeyDelimiter == null)
            {
                throw Error.ArgumentNull("urlKeyDelimiter");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            defaultOptions.UrlKeyDelimiter = urlKeyDelimiter;
            return builder;
        }

        /// <summary>
        /// Get the UrlKeyDelimiter in DefaultODataPathHandler.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        internal static ODataUrlKeyDelimiter GetUrlKeyDelimiter(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            return defaultOptions.UrlKeyDelimiter;
        }

        /// <summary>
        /// Sets the <see cref="TimeZoneInfo"/> in route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="timeZoneInfo">The <see cref="TimeZoneInfo"/></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder SetTimeZoneInfo(this IEndpointRouteBuilder builder, TimeZoneInfo timeZoneInfo)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            if (timeZoneInfo == null)
            {
                throw Error.ArgumentNull("timeZoneInfo");
            }

            TimeZoneInfoHelper.TimeZone = timeZoneInfo;
            return builder;
        }

        /// <summary>
        /// Gets the <see cref="TimeZoneInfo"/> from route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <returns></returns>
        public static TimeZoneInfo GetTimeZoneInfo(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            return TimeZoneInfoHelper.TimeZone;
        }

        /// <summary>
        /// Set the CompatibilityOptions.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="options">The <see cref="CompatibilityOptions"/></param>
        public static IEndpointRouteBuilder SetCompatibilityOptions(this IEndpointRouteBuilder builder, CompatibilityOptions options)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            ODataOptions defaultOptions = builder.GetDefaultODataOptions();
            defaultOptions.CompatibilityOptions = options;
            return builder;
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <returns>The input <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapODataRoute(this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model)
        {
            return builder.MapODataRoute(
                routeName,
                routePrefix,
                containerBuilder => containerBuilder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)
                    .AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton,
                        sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, builder.ServiceProvider)));
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes. When the <paramref name="batchHandler"/> is
        /// non-<c>null</c>, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapODataRoute(this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ODataBatchHandler batchHandler)
        {
            return builder.MapODataRoute(routeName, routePrefix, containerBuilder =>
                containerBuilder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => batchHandler)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, builder.ServiceProvider)));
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapODataRoute(this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions)
        {
            return builder.MapODataRoute(routeName, routePrefix, containerBuilder =>
                containerBuilder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable()));
        }

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="batchHandler"/> is non-<c>null</c>, it will
        /// create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapODataRoute(this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler)
        {
            return builder.MapODataRoute(routeName, routePrefix, containerBuilder =>
                containerBuilder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable())
                       .AddService(ServiceLifetime.Singleton, sp => batchHandler));
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The input <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapODataRoute(this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            Action<IContainerBuilder> configureAction)
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
            IServiceProvider serviceProvider = builder.ServiceProvider;

            IPerRouteContainer perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
            if (perRouteContainer == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }

            // Make sure the MetadataController is registered with the ApplicationPartManager.
            ApplicationPartManager applicationPartManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(MetadataController).Assembly));

            // Create an service provider for this route. Add the default services to the custom configuration actions.
            Action<IContainerBuilder> builderAction = ConfigureDefaultServices(builder, configureAction);

            IServiceProvider subServiceProvider = perRouteContainer.CreateODataRootContainer(routeName, builderAction);

            // Resolve the path handler and set URI resolver to it.
            IODataPathHandler pathHandler = subServiceProvider.GetRequiredService<IODataPathHandler>();

            // If settings is not on local, use the global configuration settings.
            ODataOptions options = serviceProvider.GetRequiredService<ODataOptions>();
            if (pathHandler != null && pathHandler.UrlKeyDelimiter == null)
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            // Resolve HTTP handler, create the OData route and register it.
            routePrefix = RemoveTrailingSlash(routePrefix);

            // If a batch handler is present, register the route with the batch path mapper. This will be used
            // by the batching middleware to handle the batch request. Batching still requires the injection
            // of the batching middleware via UseODataBatching().
            ODataBatchHandler batchHandler = subServiceProvider.GetService<ODataBatchHandler>();
            if (batchHandler != null)
            {
                // TODO: for the $batch, need refactor/test it for more.
                batchHandler.ODataRouteName = routeName;

                string batchPath = String.IsNullOrEmpty(routePrefix)
                    ? '/' + ODataRouteConstants.Batch
                    : '/' + routePrefix + '/' + ODataRouteConstants.Batch;

                ODataBatchPathMapping batchMapping = builder.ServiceProvider.GetRequiredService<ODataBatchPathMapping>();
                batchMapping.IsEndpointRouting = true;
                batchMapping.AddRoute(routeName, batchPath);
            }

            builder.MapDynamicControllerRoute<ODataEndpointRouteValueTransformer>(
                ODataEndpointPattern.CreateODataEndpointPattern(routeName, routePrefix));

            perRouteContainer.AddRoute(routeName, routePrefix);

            return builder;
        }

        /// <summary>
        /// Enables dependency injection support for HTTP routes.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the container to.</param>
        public static void EnableDependencyInjection(this IEndpointRouteBuilder builder)
        {
            builder.EnableDependencyInjection(null);
        }

        /// <summary>
        /// Enables dependency injection support for HTTP routes.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the container to.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        public static void EnableDependencyInjection(this IEndpointRouteBuilder builder,
            Action<IContainerBuilder> configureAction)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            IPerRouteContainer perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            if (perRouteContainer == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }

            if (perRouteContainer.HasODataRootContainer(null))
            {
                throw Error.InvalidOperation(SRResources.CannotReEnableDependencyInjection);
            }

            // Get the per-route container and create a new non-route container.
            perRouteContainer.CreateODataRootContainer(null, ConfigureDefaultServices(builder, configureAction));
        }

        /// <summary>
        /// Remote the trailing slash from a route prefix string.
        /// </summary>
        /// <param name="routePrefix">The route prefix string.</param>
        /// <returns>The route prefix string without a trailing slash.</returns>
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
        /// <param name="routeBuilder">The <see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>A configuring action to add the services to the root container.</returns>
        internal static Action<IContainerBuilder> ConfigureDefaultServices(IEndpointRouteBuilder routeBuilder, Action<IContainerBuilder> configureAction)
        {
            return (builder =>
            {
                // Add platform-specific services here. Add Configuration first as other services may rely on it.
                // For assembly resolution, add the and internal (IWebApiAssembliesResolver) where IWebApiAssembliesResolver
                // is transient and instantiated from ApplicationPartManager by DI.
                builder.AddService<IWebApiAssembliesResolver, WebApiAssembliesResolver>(ServiceLifetime.Transient);
                builder.AddService<IODataPathTemplateHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);
                builder.AddService<IETagHandler, DefaultODataETagHandler>(ServiceLifetime.Singleton);

                // Access the default query settings and options from the global container.
                builder.AddService(ServiceLifetime.Singleton, sp => routeBuilder.GetDefaultQuerySettings());
                builder.AddService(ServiceLifetime.Singleton, sp => routeBuilder.GetDefaultODataOptions());

                // Add the default webApi services.
                builder.AddDefaultWebApiServices();

                // Add custom actions.
                configureAction?.Invoke(builder);
            });
        }
    }
}
#endif
