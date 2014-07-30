// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
