// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class ODataHttpRouteCollectionExtensionTest
    {
        [Fact]
        public void MapODataRoute_ConfiguresARoute_WithAnODataRouteConstraint()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            routes.MapODataRoute(routeName, routePrefix, model);

            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            var constraint = Assert.Single(odataRoute.Constraints);
            var odataConstraint = Assert.IsType<ODataPathRouteConstraint>(constraint.Value);
            Assert.Same(model, odataConstraint.EdmModel);
            Assert.IsType<DefaultODataPathHandler>(odataConstraint.PathHandler);
            Assert.IsType<Collection<IODataRoutingConvention>>(odataConstraint.RoutingConventions);
        }

        [Fact]
        public void AdvancedMapODataRoute_ConfiguresARoute_WithAnODataRouteConstraint()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";
            var pathHandler = new DefaultODataPathHandler();
            var conventions = new List<IODataRoutingConvention>();

            routes.MapODataRoute(routeName, routePrefix, model, pathHandler, conventions);

            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            var constraint = Assert.Single(odataRoute.Constraints);
            var odataConstraint = Assert.IsType<ODataPathRouteConstraint>(constraint.Value);
            Assert.Same(model, odataConstraint.EdmModel);
            Assert.Same(pathHandler, odataConstraint.PathHandler);
            Assert.Same(conventions, odataConstraint.RoutingConventions);
        }
    }
}
