// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class HttpRouteCollectionExtensionsTest
    {
        [Fact]
        public void MapODataServiceRoute_ConfiguresARoute_WithAnODataRouteAndVersionConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            // Act
            routes.MapODataServiceRoute(routeName, routePrefix, model);

            // Assert
            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            Assert.Equal(2, odataRoute.Constraints.Count);

            var odataPathConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataPathRouteConstraint>());
            Assert.Same(model, odataPathConstraint.EdmModel);
            Assert.IsType<DefaultODataPathHandler>(odataPathConstraint.PathHandler);
            Assert.NotNull(odataPathConstraint.RoutingConventions);

            var odataVersionConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataVersionConstraint>());
            Assert.Equal(ODataVersion.V1, odataVersionConstraint.MinVersion);            
            Assert.Equal(ODataVersion.V3, odataVersionConstraint.MaxVersion);
        }

        [Fact]
        public void AdvancedMapODataServiceRoute_ConfiguresARoute_WithAnODataRouteConstraintAndVersionConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";
            var pathHandler = new DefaultODataPathHandler();
            var conventions = new List<IODataRoutingConvention>();

            // Act
            routes.MapODataServiceRoute(routeName, routePrefix, model, pathHandler, conventions);

            // Assert
            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            Assert.Equal(2, odataRoute.Constraints.Count);

            var odataPathConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataPathRouteConstraint>());
            Assert.Same(model, odataPathConstraint.EdmModel);
            Assert.IsType<DefaultODataPathHandler>(odataPathConstraint.PathHandler);
            Assert.NotNull(odataPathConstraint.RoutingConventions);

            var odataVersionConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataVersionConstraint>());
            Assert.Equal(ODataVersion.V1, odataVersionConstraint.MinVersion);
            Assert.Equal(ODataVersion.V3, odataVersionConstraint.MaxVersion);
        }

        [Fact]
        public void MapODataServiceRoute_AddsBatchRoute_WhenBatchHandlerIsProvided()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";
            var batchHandler = new DefaultODataBatchHandler(new HttpServer());

            // Act
            routes.MapODataServiceRoute(routeName, routePrefix, model, batchHandler);

            // Assert
            IHttpRoute batchRoute = routes["nameBatch"];
            Assert.NotNull(batchRoute);
            Assert.Same(batchHandler, batchRoute.Handler);
            Assert.Equal("prefix/$batch", batchRoute.RouteTemplate);
        }
    }
}