// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class HttpRouteCollectionExtensionsTest
    {
        [Fact]
        public void MapODataServiceRoute_ConfiguresARoute_WithAnODataRouteAndVersionConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            // Act
            config.MapODataServiceRoute(routeName, routePrefix, model);

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
            Assert.NotNull(odataVersionConstraint.Version);
            Assert.Equal(ODataVersion.V4, odataVersionConstraint.Version);
        }

        [Fact]
        public void AdvancedMapODataServiceRoute_ConfiguresARoute_WithAnODataRouteAndVersionConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";
            var pathHandler = new DefaultODataPathHandler();
            var conventions = new List<IODataRoutingConvention>();

            // Act
            config.MapODataServiceRoute(routeName, routePrefix, model, pathHandler, conventions);

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
            Assert.NotNull(odataVersionConstraint.Version);
            Assert.Equal(ODataVersion.V4, odataVersionConstraint.Version);
        }

        [Fact]
        public void MapODataServiceRoute_AddsBatchRoute_WhenBatchHandlerIsProvided()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            var batchHandler = new DefaultODataBatchHandler(new HttpServer());
            config.MapODataServiceRoute(routeName, routePrefix, model, batchHandler);

            IHttpRoute batchRoute = routes["nameBatch"];
            Assert.NotNull(batchRoute);
            Assert.Same(batchHandler, batchRoute.Handler);
            Assert.Equal("prefix/$batch", batchRoute.RouteTemplate);
        }

        [Fact]
        public void MapODataServiceRoute_MapsBatchRouteWhenBatchHandlerIsProvidedToGeneralOverload()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();
            string routeName = "name";
            string routePrefix = "prefix";

            HttpMessageHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            config.MapODataServiceRoute(routeName, routePrefix, model, batchHandler);

            IHttpRoute batchRoute = routes["nameBatch"];
            Assert.NotNull(batchRoute);
            Assert.Same(batchHandler, batchRoute.Handler);
            Assert.Equal("prefix/$batch", batchRoute.RouteTemplate);
        }

        [Fact]
        public void MapODataServiceRoute_MapsHandlerWhenAHandlerIsProvided()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();
            HttpMessageHandler handler = new HttpControllerDispatcher(new HttpConfiguration());

            // Act
            ODataRoute route = config.MapODataServiceRoute("odata", "odata", model, handler);

            // Assert
            Assert.NotNull(route);
            Assert.Same(handler, route.Handler);
        }

        [Fact]
        public void MapODataRoute_Returns_ODataRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();

            // Act
            ODataRoute route = config.MapODataServiceRoute("odata", "odata", model);

            // Assert
            Assert.NotNull(route);
            Assert.Same(model, route.PathRouteConstraint.EdmModel);
            Assert.Equal("odata", route.PathRouteConstraint.RouteName);
        }

        [Fact]
        public void MapODataServiceRoute_ConfigEnsureInitialized_DoesNotThrowForValidPathTemplateWithAttributeRouting()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            HttpConfiguration configuration = new[] { typeof(CustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute(model);

            // Act & Assert
            Assert.DoesNotThrow(() => configuration.EnsureInitialized());
        }

        [Fact]
        public void MapODataServiceRoute_ConfigEnsureInitialized_DoesNotThrowForInvalidPathTemplateWithoutAttributeRouting()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers").EntityType.Ignore(c => c.Name);
            IEdmModel model = builder.GetEdmModel();
            HttpConfiguration configuration = new[] { typeof(CustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute(
                "RouteName",
                "RoutePrefix",
                model,
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault());

            // Act & Assert
            Assert.DoesNotThrow(() => configuration.EnsureInitialized());
        }

        public class Customer
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class CustomersController : ODataController
        {
            [ODataRoute("Customers({ID})/Name")]
            public IHttpActionResult Get(int ID)
            {
                return Ok();
            }
        }
    }
}