// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

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
        public static void MapODataRoute(this HttpRouteCollection routes, string routeName, string routePrefix, IEdmModel model)
        {
            routes.MapODataRoute(routeName, routePrefix, model, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
        public static void MapODataRoute(this HttpRouteCollection routes, string routeName, string routePrefix, IEdmModel model,
            IODataPathHandler pathHandler, IEnumerable<IODataRoutingConvention> routingConventions)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

            string routeTemplate = String.IsNullOrEmpty(routePrefix) ?
                ODataRouteConstants.ODataPathTemplate :
                routePrefix + "/" + ODataRouteConstants.ODataPathTemplate;
            IHttpRouteConstraint routeConstraint = new ODataPathRouteConstraint(pathHandler, model, routeName, routingConventions);
            HttpRouteValueDictionary constraintDictionary = new HttpRouteValueDictionary() { { ODataRouteConstants.ConstraintName, routeConstraint } };
            routes.MapHttpRoute(routeName, routeTemplate, defaults: null, constraints: constraintDictionary);
        }
    }
}
