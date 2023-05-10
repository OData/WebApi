//-----------------------------------------------------------------------------
// <copyright file="HttpConfigurationExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        // Maintain the Microsoft.AspNet.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.
        private const string ETagHandlerKey = "Microsoft.AspNet.OData.ETagHandler";

        private const string TimeZoneInfoKey = "Microsoft.AspNet.OData.TimeZoneInfo";

        private const string UrlKeyDelimiterKey = "Microsoft.AspNet.OData.UrlKeyDelimiterKey";

        private const string ContinueOnErrorKey = "Microsoft.AspNet.OData.ContinueOnErrorKey";

        private const string NullDynamicPropertyKey = "Microsoft.AspNet.OData.NullDynamicPropertyKey";

        private const string ContainerBuilderFactoryKey = "Microsoft.AspNet.OData.ContainerBuilderFactoryKey";

        private const string PerRouteContainerKey = "Microsoft.AspNet.OData.PerRouteContainerKey";

        private const string DefaultQuerySettingsKey = "Microsoft.AspNet.OData.DefaultQuerySettings";

        private const string NonODataRootContainerKey = "Microsoft.AspNet.OData.NonODataRootContainerKey";

        private const string CompatibilityOptionsKey = "Microsoft.AspNet.OData.CompatibilityOptionsKey";

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void AddODataQueryFilter(this HttpConfiguration configuration)
        {
            AddODataQueryFilter(configuration, new EnableQueryAttribute());
        }

        /// <summary>
        /// Sets the <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static void SetDefaultQuerySettings(this HttpConfiguration configuration, DefaultQuerySettings defaultQuerySettings)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (defaultQuerySettings == null)
            {
                throw Error.ArgumentNull("defaultQuerySettings");
            }

            if (!defaultQuerySettings.MaxTop.HasValue || defaultQuerySettings.MaxTop > 0)
            {
                ModelBoundQuerySettings.DefaultModelBoundQuerySettings.MaxTop = defaultQuerySettings.MaxTop;
            }

            configuration.Properties[DefaultQuerySettingsKey] = defaultQuerySettings;
        }

        /// <summary>
        /// Gets the <see cref="DefaultQuerySettings"/> from the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static DefaultQuerySettings GetDefaultQuerySettings(this HttpConfiguration configuration)
        {
            object instance;
            if (!configuration.Properties.TryGetValue(DefaultQuerySettingsKey, out instance))
            {
                DefaultQuerySettings defaultQuerySettings = new DefaultQuerySettings();
                configuration.SetDefaultQuerySettings(defaultQuerySettings);
                return defaultQuerySettings;
            }

            return instance as DefaultQuerySettings;
        }

        /// <summary>
        /// Sets the MaxTop of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration MaxTop(this HttpConfiguration configuration, int? maxTopValue)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.MaxTop = maxTopValue;
            if (!maxTopValue.HasValue || maxTopValue > 0)
            {
                ModelBoundQuerySettings.DefaultModelBoundQuerySettings.MaxTop = maxTopValue;
            }

            return configuration;
        }

        /// <summary>
        /// Sets the EnableExpand of <see cref="DefaultQuerySettings"/> in the configuration,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// Todo: change QueryOptionSetting to SelectExpandType.
        /// </summary>
        public static HttpConfiguration Expand(this HttpConfiguration configuration, QueryOptionSetting setting)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableExpand = setting == QueryOptionSetting.Allowed;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableExpand to true of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration Expand(this HttpConfiguration configuration)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableExpand = true;
            return configuration;
        }

        /// <summary>
        /// Sets the SelectType of <see cref="DefaultQuerySettings"/> in the configuration,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// Todo: change QueryOptionSetting to SelectExpandType.
        /// </summary>
        public static HttpConfiguration Select(this HttpConfiguration configuration, QueryOptionSetting setting)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSelect = setting == QueryOptionSetting.Allowed;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableSelect to true of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration Select(this HttpConfiguration configuration)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSelect = true;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableFilter of <see cref="DefaultQuerySettings"/> in the configuration,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static HttpConfiguration Filter(this HttpConfiguration configuration, QueryOptionSetting setting)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableFilter = setting == QueryOptionSetting.Allowed;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableFilter to true of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration Filter(this HttpConfiguration configuration)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableFilter = true;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableOrderBy of <see cref="DefaultQuerySettings"/> in the configuration,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static HttpConfiguration OrderBy(this HttpConfiguration configuration, QueryOptionSetting setting)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableOrderBy = setting == QueryOptionSetting.Allowed;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableOrderBy to true of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration OrderBy(this HttpConfiguration configuration)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableOrderBy = true;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableSkipToken of <see cref="DefaultQuerySettings"/> in the configuration,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static HttpConfiguration SkipToken(this HttpConfiguration configuration, QueryOptionSetting setting)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSkipToken = setting == QueryOptionSetting.Allowed;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableSkipToken to true of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration SkipToken(this HttpConfiguration configuration)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSkipToken = true;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableCount of <see cref="DefaultQuerySettings"/> in the configuration,
        /// depends on <see cref="QueryOptionSetting"/>.
        /// </summary>
        public static HttpConfiguration Count(this HttpConfiguration configuration, QueryOptionSetting setting)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableCount = setting == QueryOptionSetting.Allowed;
            return configuration;
        }

        /// <summary>
        /// Sets the EnableCount to true of <see cref="DefaultQuerySettings"/> in the configuration.
        /// </summary>
        public static HttpConfiguration Count(this HttpConfiguration configuration)
        {
            DefaultQuerySettings defaultQuerySettings = configuration.GetDefaultQuerySettings();
            defaultQuerySettings.EnableCount = true;
            return configuration;
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public static void AddODataQueryFilter(this HttpConfiguration configuration, IActionFilter queryFilter)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Add(typeof(IFilterProvider), new QueryFilterProvider(queryFilter));
        }

        /// <summary>
        /// Gets the <see cref="IETagHandler"/> from the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>The <see cref="IETagHandler"/> for the configuration.</returns>
        public static IETagHandler GetETagHandler(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object handler;
            if (!configuration.Properties.TryGetValue(ETagHandlerKey, out handler))
            {
                IETagHandler defaultETagHandler = new DefaultODataETagHandler();
                configuration.SetETagHandler(defaultETagHandler);
                return defaultETagHandler;
            }

            if (handler == null)
            {
                throw Error.InvalidOperation(SRResources.NullETagHandler);
            }

            IETagHandler etagHandler = handler as IETagHandler;
            if (etagHandler == null)
            {
                throw Error.InvalidOperation(SRResources.InvalidETagHandler, handler.GetType());
            }

            return etagHandler;
        }

        /// <summary>
        /// Sets the <see cref="IETagHandler"/> on the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="handler">The <see cref="IETagHandler"/> for the configuration.</param>
        public static void SetETagHandler(this HttpConfiguration configuration, IETagHandler handler)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            configuration.Properties[ETagHandlerKey] = handler;
        }

        /// <summary>
        /// Gets the <see cref="TimeZoneInfo"/> from the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>The <see cref="TimeZoneInfo"/> for the configuration.</returns>
        public static TimeZoneInfo GetTimeZoneInfo(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object value;
            TimeZoneInfo timeZoneInfo;
            if (!configuration.Properties.TryGetValue(TimeZoneInfoKey, out value))
            {
                timeZoneInfo = TimeZoneInfo.Local;
                configuration.SetTimeZoneInfo(timeZoneInfo);
                return timeZoneInfo;
            }

            timeZoneInfo = value as TimeZoneInfo;
            if (timeZoneInfo == null)
            {
                throw Error.InvalidOperation(SRResources.InvalidTimeZoneInfo, value.GetType(), typeof(TimeZoneInfo));
            }

            return timeZoneInfo;
        }

        /// <summary>
        /// Sets the <see cref="TimeZoneInfo"/> on the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="timeZoneInfo">The <see cref="TimeZoneInfo"/> for the configuration.</param>
        /// <returns></returns>
        public static void SetTimeZoneInfo(this HttpConfiguration configuration, TimeZoneInfo timeZoneInfo)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (timeZoneInfo == null)
            {
                throw Error.ArgumentNull("timeZoneInfo");
            }

            configuration.Properties[TimeZoneInfoKey] = timeZoneInfo;
            TimeZoneInfoHelper.TimeZone = timeZoneInfo;
        }

        /// <summary>
        /// Enable the continue-on-error header.
        /// </summary>
        public static void EnableContinueOnErrorHeader(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Properties[ContinueOnErrorKey] = true;
        }

        /// <summary>
        /// Check the continue-on-error header is enable or not.
        /// </summary>
        /// <returns>True if continue-on-error header is enable; false otherwise</returns>
        internal static bool HasEnabledContinueOnErrorHeader(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object value;
            if (configuration.Properties.TryGetValue(ContinueOnErrorKey, out value))
            {
                return (bool)value;
            }
            return false;
        }

        /// <summary>
        /// Sets whether or not the null dynamic property to be serialized.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="serialize"><c>true</c> to serialize null dynamic property, <c>false</c> otherwise.</param>
        public static void SetSerializeNullDynamicProperty(this HttpConfiguration configuration, bool serialize)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Properties[NullDynamicPropertyKey] = serialize;
        }

        /// <summary>
        /// Set the UrlKeyDelimiter in DefaultODataPathHandler.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="urlKeyDelimiter">The <see cref="ODataUrlKeyDelimiter"/></param>
        public static void SetUrlKeyDelimiter(this HttpConfiguration configuration, ODataUrlKeyDelimiter urlKeyDelimiter)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Properties[UrlKeyDelimiterKey] = urlKeyDelimiter;
        }

        /// <summary>
        /// Set the ODataCompatibilityOption.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="options">The <see cref="CompatibilityOptions"/></param>
        public static void SetCompatibilityOptions(this HttpConfiguration configuration, CompatibilityOptions options)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Properties[CompatibilityOptionsKey] = options;
        }

        /// <summary>
        /// Check the null dynamic property is enable or not.
        /// </summary>
        /// <returns></returns>
        internal static bool HasEnabledNullDynamicProperty(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object value;
            if (configuration.Properties.TryGetValue(NullDynamicPropertyKey, out value))
            {
                return (bool)value;
            }

            return false;
        }

        internal static ODataUrlKeyDelimiter GetUrlKeyDelimiter(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object value;
            if (configuration.Properties.TryGetValue(UrlKeyDelimiterKey, out value))
            {
                return value as ODataUrlKeyDelimiter;
            }

            configuration.Properties[UrlKeyDelimiterKey] = null;
            return null;
        }

        internal static CompatibilityOptions GetCompatibilityOptions(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object value;
            if (configuration.Properties.TryGetValue(CompatibilityOptionsKey, out value))
            {
                return (CompatibilityOptions)value;
            }

            configuration.Properties[CompatibilityOptionsKey] = CompatibilityOptions.None;
            return CompatibilityOptions.None;
        }

        /// <summary>
        /// Specifies a custom container builder.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="builderFactory">The factory to create a container builder.</param>
        /// <returns>The server configuration.</returns>
        public static HttpConfiguration UseCustomContainerBuilder(this HttpConfiguration configuration, Func<IContainerBuilder> builderFactory)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (builderFactory == null)
            {
                throw Error.ArgumentNull("builderFactory");
            }

            configuration.Properties[ContainerBuilderFactoryKey] = builderFactory;

            return configuration;
        }

        /// <summary>
        /// Enables dependency injection support for HTTP routes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void EnableDependencyInjection(this HttpConfiguration configuration)
        {
            configuration.EnableDependencyInjection(null);
        }

        /// <summary>
        /// Enables dependency injection support for HTTP routes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        public static void EnableDependencyInjection(this HttpConfiguration configuration,
            Action<IContainerBuilder> configureAction)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (configuration.Properties.ContainsKey(NonODataRootContainerKey))
            {
                throw Error.InvalidOperation(SRResources.CannotReEnableDependencyInjection);
            }

            // Get the per-route container and create a new non-route container.
            IPerRouteContainer perRouteContainer = GetPerRouteContainer(configuration);
            perRouteContainer.CreateODataRootContainer(null, ConfigureDefaultServices(configuration, configureAction));
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, Action<IContainerBuilder> configureAction)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            // 1) Build and configure the root container.
            IServiceProvider rootContainer = configuration.CreateODataRootContainer(routeName, configureAction);

            // 2) Resolve the path handler and set URI resolver to it.
            IODataPathHandler pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();
            
            // if settings is not on local, use the global configuration settings.
            if (pathHandler != null && pathHandler.UrlKeyDelimiter == null)
            {
                ODataUrlKeyDelimiter urlKeyDelimiter = configuration.GetUrlKeyDelimiter();
                pathHandler.UrlKeyDelimiter = urlKeyDelimiter;
            }

            // 3) Resolve some required services and create the route constraint.
            ODataPathRouteConstraint routeConstraint = new ODataPathRouteConstraint(routeName);

            // Attribute routing must initialized before configuration.EnsureInitialized is called.
            rootContainer.GetServices<IODataRoutingConvention>();

            // 4) Resolve HTTP handler, create the OData route and register it.
            ODataRoute route;
            HttpRouteCollection routes = configuration.Routes;
            routePrefix = RemoveTrailingSlash(routePrefix);
            HttpMessageHandler messageHandler = rootContainer.GetService<HttpMessageHandler>();
            if (messageHandler != null)
            {
                route = new ODataRoute(
                    routePrefix,
                    routeConstraint,
                    defaults: null,
                    constraints: null,
                    dataTokens: null,
                    handler: messageHandler);
            }
            else
            {
                ODataBatchHandler batchHandler = rootContainer.GetService<ODataBatchHandler>();
                if (batchHandler != null)
                {
                    batchHandler.ODataRouteName = routeName;
                    string batchTemplate = String.IsNullOrEmpty(routePrefix) ? ODataRouteConstants.Batch
                        : routePrefix + '/' + ODataRouteConstants.Batch;
                    routes.MapHttpBatchRoute(routeName + "Batch", batchTemplate, batchHandler);
                }

                route = new ODataRoute(routePrefix, routeConstraint);
            }

            routes.Add(routeName, route);
            return route;
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, IEdmModel model)
        {
            return configuration.MapODataServiceRoute(routeName, routePrefix, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, configuration)));
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes. When the <paramref name="batchHandler"/> is
        /// non-<c>null</c>, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, IEdmModel model, ODataBatchHandler batchHandler)
        {
            return configuration.MapODataServiceRoute(routeName, routePrefix, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => batchHandler)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, configuration)));
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes. When the <paramref name="defaultHandler"/>
        /// is non-<c>null</c>, it will map it as the default handler for the route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, IEdmModel model, HttpMessageHandler defaultHandler)
        {
            return configuration.MapODataServiceRoute(routeName, routePrefix, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => defaultHandler)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, configuration)));
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions)
        {
            return configuration.MapODataServiceRoute(routeName, routePrefix, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable()));
        }

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="batchHandler"/> is non-<c>null</c>, it will
        /// create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "We want the handler to be a batch handler.")]
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions, ODataBatchHandler batchHandler)
        {
            return configuration.MapODataServiceRoute(routeName, routePrefix, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable())
                       .AddService(ServiceLifetime.Singleton, sp => batchHandler));
        }

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="defaultHandler"/> is non-<c>null</c>, it will map
        /// it as the handler for the route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions, HttpMessageHandler defaultHandler)
        {
            return configuration.MapODataServiceRoute(routeName, routePrefix, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable())
                       .AddService(ServiceLifetime.Singleton, sp => defaultHandler));
        }

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
        /// Create the per-route container from the configuration for a given route.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The per-route container from the configuration</returns>
        internal static IServiceProvider CreateODataRootContainer(this HttpConfiguration configuration,
            string routeName, Action<IContainerBuilder> configureAction)
        {
            IPerRouteContainer perRouteContainer = configuration.GetPerRouteContainer();
            return perRouteContainer.CreateODataRootContainer(routeName, ConfigureDefaultServices(configuration, configureAction));
        }

        /// <summary>
        /// Get the per-route container from the configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The per-route container from the configuration</returns>
        internal static IPerRouteContainer GetPerRouteContainer(this HttpConfiguration configuration)
        {
            return (IPerRouteContainer)configuration.Properties.GetOrAdd(
                PerRouteContainerKey,
                key =>
                {
                    IPerRouteContainer perRouteContainer = new PerRouteContainer(configuration);

                    // Attach the build factory if there is one.
                    object value;
                    if (configuration.Properties.TryGetValue(ContainerBuilderFactoryKey, out value))
                    {
                        Func<IContainerBuilder> builderFactory = (Func<IContainerBuilder>)value;
                        perRouteContainer.BuilderFactory = builderFactory;
                    }

                    return perRouteContainer;
                });
        }

        /// <summary>
        /// Get the OData root container for a given route.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="routeName">The route name.</param>
        /// <returns>The OData root container for a given route.</returns>
        internal static IServiceProvider GetODataRootContainer(this HttpConfiguration configuration, string routeName)
        {
            IPerRouteContainer perRouteContainer = GetPerRouteContainer(configuration);
            return perRouteContainer.GetODataRootContainer(routeName);
        }

        /// <summary>
        /// Get the OData root container for HTTP routes.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The OData root container for HTTP routes.</returns>
        internal static IServiceProvider GetNonODataRootContainer(this HttpConfiguration configuration)
        {
            object value;
            if (configuration.Properties.TryGetValue(NonODataRootContainerKey, out value))
            {
                return (IServiceProvider)value;
            }

            throw Error.InvalidOperation(SRResources.NoNonODataHttpRouteRegistered);
        }

        /// <summary>
        /// Enables dependency injection support for HTTP routes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="rootContainer">The root container.</param>
        internal static void SetNonODataRootContainer(this HttpConfiguration configuration,
            IServiceProvider rootContainer)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (rootContainer == null)
            {
                throw Error.ArgumentNull("rootContainer");
            }

            if (configuration.Properties.ContainsKey(NonODataRootContainerKey))
            {
                throw Error.InvalidOperation(SRResources.CannotReEnableDependencyInjection);
            }

            configuration.Properties[NonODataRootContainerKey] = rootContainer;
        }

        /// <summary>
        /// Configure the default services.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>A configuring action to add the services to the root container.</returns>
        private static Action<IContainerBuilder> ConfigureDefaultServices(HttpConfiguration configuration, Action<IContainerBuilder> configureAction)
        {
            return (builder =>
            {
                // Add platform-specific services here. Add Configuration first as other services may rely on it.
                // For assembly resolution, add both the public (IAssembliesResolver) and internal (IWebApiAssembliesResolver)
                // where IWebApiAssembliesResolver is transient and instantiated from IAssembliesResolver by DI.
                IAssembliesResolver resolver = configuration.Services.GetAssembliesResolver() ?? new DefaultAssembliesResolver();
                builder.AddService(ServiceLifetime.Singleton, sp => resolver);
                builder.AddService<IWebApiAssembliesResolver, WebApiAssembliesResolver>(ServiceLifetime.Transient);

                builder.AddService(ServiceLifetime.Singleton, sp => configuration);
                builder.AddService(ServiceLifetime.Singleton, sp => configuration.GetDefaultQuerySettings());

                // Currently, the ETagHandler is attached to the configuration.
                //builder.AddService<IETagHandler, DefaultODataETagHandler>(ServiceLifetime.Singleton);

                // Add the default webApi services.
                builder.AddDefaultWebApiServices();

                // Add custom actions.
                if (configureAction != null)
                {
                    configureAction.Invoke(builder);
                }
            });
        }
    }
}
