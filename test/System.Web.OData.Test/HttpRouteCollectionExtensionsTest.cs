// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class HttpRouteCollectionExtensionsTest
    {
        [Fact]
        public void MapODataServiceRoute_ConfiguresARoute_WithAnODataRouteConstraint()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            routes.MapODataServiceRoute(routeName, routePrefix, model);

            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            var constraint = Assert.Single(odataRoute.Constraints);
            var odataConstraint = Assert.IsType<ODataPathRouteConstraint>(constraint.Value);
            Assert.Same(model, odataConstraint.EdmModel);
            Assert.IsType<DefaultODataPathHandler>(odataConstraint.PathHandler);
            Assert.NotNull(odataConstraint.RoutingConventions);
        }

        [Fact]
        public void AdvancedMapODataServiceRoute_ConfiguresARoute_WithAnODataRouteConstraint()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";
            var pathHandler = new DefaultODataPathHandler();
            var conventions = new List<IODataRoutingConvention>();

            routes.MapODataServiceRoute(routeName, routePrefix, model, pathHandler, conventions);

            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            var constraint = Assert.Single(odataRoute.Constraints);
            var odataConstraint = Assert.IsType<ODataPathRouteConstraint>(constraint.Value);
            Assert.Same(model, odataConstraint.EdmModel);
            Assert.Same(pathHandler, odataConstraint.PathHandler);
            Assert.Equal(conventions, odataConstraint.RoutingConventions);
        }

        [Fact]
        public void MapODataServiceRoute_AddsBatchRoute_WhenBatchHandlerIsProvided()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            var batchHandler = new DefaultODataBatchHandler(new HttpServer());
            routes.MapODataServiceRoute(routeName, routePrefix, model, batchHandler);

            IHttpRoute batchRoute = routes["nameBatch"];
            Assert.NotNull(batchRoute);
            Assert.Same(batchHandler, batchRoute.Handler);
            Assert.Equal("prefix/$batch", batchRoute.RouteTemplate);
        }

        [Fact]
        public void MapODataRoute_Returns_ODataRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();

            // Act
            ODataRoute route = routes.MapODataServiceRoute("odata", "odata", model);

            // Assert
            Assert.NotNull(route);
            Assert.Same(model, route.PathRouteConstraint.EdmModel);
            Assert.Equal("odata", route.PathRouteConstraint.RouteName);
        }
    }
}