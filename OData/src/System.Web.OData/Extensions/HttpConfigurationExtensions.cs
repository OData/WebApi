﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.OData.Batch;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class. 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        // Maintain the System.Web.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.
        private const string ETagHandlerKey = "System.Web.OData.ETagHandler";

        private const string TimeZoneInfoKey = "System.Web.OData.TimeZoneInfo";

        private const string ResolverSettingsKey = "System.Web.OData.ResolverSettingsKey";

        private const string ContinueOnErrorKey = "System.Web.OData.ContinueOnErrorKey";

        private const string NullDynamicPropertyKey = "System.Web.OData.NullDynamicPropertyKey";

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
        /// Sets the case insensitive flag for the Uri parser on the configuration. Both metadata and key words
        /// are impacted by this flag.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="caseInsensitive"><c>true</c> to enable case insensitive, <c>false</c> otherwise.</param>
        public static void EnableCaseInsensitive(this HttpConfiguration configuration, bool caseInsensitive)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ODataUriResolverSetttings settings = configuration.GetResolverSettings();
            settings.CaseInsensitive = caseInsensitive;
        }

        /// <summary>
        /// Sets the un-qualified function and action name call flag for the Uri parser on the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="unqualifiedNameCall"><c>true</c> to enable un-qualified name call, <c>false</c> otherwise.</param>
        public static void EnableUnqualifiedNameCall(this HttpConfiguration configuration, bool unqualifiedNameCall)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ODataUriResolverSetttings settings = configuration.GetResolverSettings();
            settings.UnqualifiedNameCall = unqualifiedNameCall;
        }

        /// <summary>
        /// Sets the Enum prefix free flag for the Uri parser on the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="enumPrefixFree"><c>true</c> to enable Enum prefix free, <c>false</c> otherwise.</param>
        public static void EnableEnumPrefixFree(this HttpConfiguration configuration, bool enumPrefixFree)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ODataUriResolverSetttings settings = configuration.GetResolverSettings();
            settings.EnumPrefixFree = enumPrefixFree;
        }

        /// <summary>
        /// Sets the Alternate Key support for the Uri parser on the configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="alternateKeys"><c>true</c> to enable Alternate Keys, <c>false</c> otherwise.</param>
        public static void EnableAlternateKeys(this HttpConfiguration configuration, bool alternateKeys)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ODataUriResolverSetttings settings = configuration.GetResolverSettings();
            settings.AlternateKeys = alternateKeys;
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
        /// <returns></returns>
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
        /// Set the UrlConventions in DefaultODataPathHandler.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="conventions">The <see cref="ODataUrlConventions"/></param>
        public static void SetUrlConventions(this HttpConfiguration configuration, ODataUrlConventions conventions)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ODataUriResolverSetttings settings = configuration.GetResolverSettings();
            settings.UrlConventions = conventions;
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

        internal static ODataUriResolverSetttings GetResolverSettings(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object value;
            if (configuration.Properties.TryGetValue(ResolverSettingsKey, out value))
            {
                return value as ODataUriResolverSetttings;
            }

            ODataUriResolverSetttings defaultSettings = new ODataUriResolverSetttings();
            configuration.Properties[ResolverSettingsKey] = defaultSettings;
            return defaultSettings;
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
            return MapODataServiceRoute(configuration, routeName, routePrefix, model, batchHandler: null);
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
            return MapODataServiceRoute(configuration, routeName, routePrefix, model, new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefaultWithAttributeRouting(configuration, model), batchHandler);
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
            return MapODataServiceRoute(configuration, routeName, routePrefix, model, new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefaultWithAttributeRouting(configuration, model), defaultHandler);
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
            return MapODataServiceRoute(configuration, routeName, routePrefix, model, pathHandler, routingConventions,
                batchHandler: null);
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
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            HttpRouteCollection routes = configuration.Routes;
            routePrefix = RemoveTrailingSlash(routePrefix);

            if (batchHandler != null)
            {
                batchHandler.ODataRouteName = routeName;
                string batchTemplate = String.IsNullOrEmpty(routePrefix) ? ODataRouteConstants.Batch
                    : routePrefix + '/' + ODataRouteConstants.Batch;
                routes.MapHttpBatchRoute(routeName + "Batch", batchTemplate, batchHandler);
            }

            DefaultODataPathHandler odataPathHandler = pathHandler as DefaultODataPathHandler;
            if (odataPathHandler != null)
            {
                odataPathHandler.ResolverSetttings = configuration.GetResolverSettings();
            }

            ODataPathRouteConstraint routeConstraint =
                new ODataPathRouteConstraint(pathHandler, model, routeName, routingConventions);
            ODataRoute route = new ODataRoute(routePrefix, routeConstraint);
            routes.Add(routeName, route);
            return route;
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
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // We have a more specific overload to map batch handlers that creates a different route for the batch
            // endpoint instead of mapping that handler as the per route handler. Given that HttpMessageHandler is a
            // base type of ODataBatchHandler, it's possible the compiler will call this overload instead of the one
            // for the batch handler, so we detect that case and call the appropiate overload for the user.
            // The case in which the compiler picks the wrong overload is:
            // HttpRequestMessageHandler batchHandler = new DefaultODataBatchHandler(httpServer);
            // config.Routes.MapODataServiceRoute("routeName", "routePrefix", model, batchHandler);
            if (defaultHandler != null)
            {
                ODataBatchHandler batchHandler = defaultHandler as ODataBatchHandler;
                if (batchHandler != null)
                {
                    return MapODataServiceRoute(configuration, routeName, routePrefix, model, batchHandler);
                }
            }

            HttpRouteCollection routes = configuration.Routes;
            routePrefix = RemoveTrailingSlash(routePrefix);

            DefaultODataPathHandler odataPathHandler = pathHandler as DefaultODataPathHandler;
            if (odataPathHandler != null)
            {
                odataPathHandler.ResolverSetttings = configuration.GetResolverSettings();
            }

            ODataPathRouteConstraint routeConstraint =
                new ODataPathRouteConstraint(pathHandler, model, routeName, routingConventions);
            ODataRoute route = new ODataRoute(
                routePrefix,
                routeConstraint,
                defaults: null,
                constraints: null,
                dataTokens: null,
                handler: defaultHandler);
            routes.Add(routeName, route);
            return route;
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
    }
}
