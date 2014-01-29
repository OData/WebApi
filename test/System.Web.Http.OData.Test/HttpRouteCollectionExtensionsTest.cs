// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
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
            Assert.IsType<List<IODataRoutingConvention>>(odataConstraint.RoutingConventions);
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
            Assert.Same(conventions, odataConstraint.RoutingConventions);
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
    }
}