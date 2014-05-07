// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Batch;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRouteCollection"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRouteCollectionExtensions
    {
        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpRouteCollection routes, string routeName,
            string routePrefix, IEdmModel model)
        {
            return MapODataServiceRoute(routes, routeName, routePrefix, model, batchHandler: null);
        }

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="batchHandler"/> is provided, it will create a
        /// '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpRouteCollection routes, string routeName,
            string routePrefix, IEdmModel model, ODataBatchHandler batchHandler)
        {
            return MapODataServiceRoute(routes, routeName, routePrefix, model, new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(), batchHandler);
        }

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="defaultHandler"/> is provided, it will map it
        /// as the default handler for the route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpRouteCollection routes, string routeName,
            string routePrefix, IEdmModel model, HttpMessageHandler defaultHandler)
        {
            return MapODataServiceRoute(routes, routeName, routePrefix, model, new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(), defaultHandler);
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpRouteCollection routes, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions)
        {
            return MapODataServiceRoute(routes, routeName, routePrefix, model, pathHandler, routingConventions,
                batchHandler: null);
        }

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="batchHandler"/> is provided, it will create a
        /// '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
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
        public static ODataRoute MapODataServiceRoute(this HttpRouteCollection routes, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions, ODataBatchHandler batchHandler)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

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
        /// Maps the specified OData route. When the <paramref name="defaultHandler"/> is provided, it will map it
        /// as the handler for the route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this HttpRouteCollection routes, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions, HttpMessageHandler defaultHandler)
        {
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
                    return MapODataServiceRoute(routes, routeName, routePrefix, model, batchHandler);
                }
            }

            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

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
