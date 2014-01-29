// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Extensions = System.Web.Http.OData.Extensions;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRouteCollection"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpRouteCollectionExtensions
    {
        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        [Obsolete("This method is obsolete; use the MapODataServiceRoute method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void MapODataRoute(this HttpRouteCollection routes, string routeName, string routePrefix,
            IEdmModel model)
        {
            Extensions.HttpRouteCollectionExtensions.MapODataServiceRoute(routes, routeName, routePrefix, model);
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
        [Obsolete("This method is obsolete; use the MapODataServiceRoute method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void MapODataRoute(this HttpRouteCollection routes, string routeName, string routePrefix,
            IEdmModel model, ODataBatchHandler batchHandler)
        {
            Extensions.HttpRouteCollectionExtensions.MapODataServiceRoute(routes, routeName, routePrefix, model, batchHandler);
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
        [Obsolete("This method is obsolete; use the MapODataServiceRoute method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void MapODataRoute(this HttpRouteCollection routes, string routeName, string routePrefix,
            IEdmModel model, IODataPathHandler pathHandler, IEnumerable<IODataRoutingConvention> routingConventions)
        {
            Extensions.HttpRouteCollectionExtensions.MapODataServiceRoute(routes, routeName, routePrefix, model,
                pathHandler, routingConventions);
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
        [Obsolete("This method is obsolete; use the MapODataServiceRoute method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "We want the handler to be a batch handler.")]
        public static void MapODataRoute(this HttpRouteCollection routes, string routeName, string routePrefix,
            IEdmModel model, IODataPathHandler pathHandler, IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler)
        {
            Extensions.HttpRouteCollectionExtensions.MapODataServiceRoute(routes, routeName, routePrefix, model,
                pathHandler, routingConventions, batchHandler);
        }
    }
}