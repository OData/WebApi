// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpConfigurationExtensions
    {
        private const string ODataRoutingConventionsKey = "MS_ODataRoutingConventions";

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void EnableQuerySupport(this HttpConfiguration configuration)
        {
            configuration.EnableQuerySupport(new QueryableAttribute());
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public static void EnableQuerySupport(this HttpConfiguration configuration, IActionFilter queryFilter)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Add(typeof(IFilterProvider), new QueryFilterProvider(queryFilter));
        }

        /// <summary>
        /// Maps the specified OData route without a route prefix and with the default OData route name.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        public static void MapODataRoute(this HttpConfiguration configuration, IEdmModel model)
        {
            configuration.MapODataRoute(ODataRouteConstants.DefaultRouteName, routePrefix: null, model: model, pathHandler: new DefaultODataPathHandler());
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        public static void MapODataRoute(this HttpConfiguration configuration, string routeName, string routePrefix, IEdmModel model)
        {
            configuration.MapODataRoute(routeName, routePrefix, model, pathHandler: new DefaultODataPathHandler());
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        public static void MapODataRoute(this HttpConfiguration configuration, string routeName, string routePrefix, IEdmModel model, IODataPathHandler pathHandler)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // Routing
            string routeTemplate = String.IsNullOrEmpty(routePrefix) ?
                ODataRouteConstants.ODataPathTemplate :
                routePrefix + "/" + ODataRouteConstants.ODataPathTemplate;
            IHttpRouteConstraint routeConstraint = new ODataPathRouteConstraint(pathHandler, model, routeName, configuration.GetODataRoutingConventions());
            HttpRouteValueDictionary constraintDictionary = new HttpRouteValueDictionary() { { ODataRouteConstants.ConstraintName, routeConstraint } };
            configuration.Routes.MapHttpRoute(routeName, routeTemplate, defaults: null, constraints: constraintDictionary);
        }

        /// <summary>
        /// Gets the OData routing conventions to use for controller and action selection.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>A list of the OData routing conventions for the server.</returns>
        public static IList<IODataRoutingConvention> GetODataRoutingConventions(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            return configuration.Properties.GetOrAdd(ODataRoutingConventionsKey, GetDefaultRoutingConventions) as IList<IODataRoutingConvention>;
        }

        private static IList<IODataRoutingConvention> GetDefaultRoutingConventions(object key)
        {
            return new List<IODataRoutingConvention>()
            {
                new MetadataRoutingConvention(),
                new EntitySetRoutingConvention(),
                new EntityRoutingConvention(),
                new NavigationRoutingConvention(),
                new LinksRoutingConvention(),
                new ActionRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}
