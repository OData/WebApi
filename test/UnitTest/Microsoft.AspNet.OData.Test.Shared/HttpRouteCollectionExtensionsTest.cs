//-----------------------------------------------------------------------------
// <copyright file="HttpRouteCollectionExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
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
            string routeName = "odata";
            string routePrefix = "odata";

            // Act
            config.MapODataServiceRoute(routeName, routePrefix, model);

            // Assert
            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            Assert.Equal(2, odataRoute.Constraints.Count);

            Assert.Single(odataRoute.Constraints.Values.OfType<ODataPathRouteConstraint>());
            Assert.Same(model, GetModel(config));
            Assert.IsType<DefaultODataPathHandler>(GetPathHandler(config));
            Assert.NotEmpty(GetRoutingConvetions(config));

            var odataVersionConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataVersionConstraint>());
            Assert.Equal(ODataVersion.V4, odataVersionConstraint.Version);
        }

        [Fact]
        public void AdvancedMapODataServiceRoute_ConfiguresARoute_WithAnODataRouteAndVersionConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpConfiguration config = new HttpConfiguration(routes);
            IEdmModel model = new EdmModel();
            string routeName = "odata";
            string routePrefix = "odata";
            var pathHandler = new DefaultODataPathHandler();
            var conventions = new List<IODataRoutingConvention>();

            // Act
            config.MapODataServiceRoute(routeName, routePrefix, model, pathHandler, conventions);

            // Assert
            IHttpRoute odataRoute = routes[routeName];
            Assert.Single(routes);
            Assert.Equal(routePrefix + "/{*odataPath}", odataRoute.RouteTemplate);
            Assert.Equal(2, odataRoute.Constraints.Count);

            Assert.Single(odataRoute.Constraints.Values.OfType<ODataPathRouteConstraint>());
            Assert.Same(model, GetModel(config));
            Assert.IsType<DefaultODataPathHandler>(GetPathHandler(config));
            Assert.Empty(GetRoutingConvetions(config));

            var odataVersionConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataVersionConstraint>());
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
            Assert.Same(model, GetModel(config));
            Assert.Equal("odata", route.PathRouteConstraint.RouteName);
        }

        [Fact]
        public void MapODataServiceRoute_ConfigEnsureInitialized_DoesNotThrowForValidPathTemplateWithAttributeRouting()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(CustomersController) });
            configuration.MapODataServiceRoute(model);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => configuration.EnsureInitialized());
        }

        [Fact]
        public void MapODataServiceRoute_ConfigEnsureInitialized_DoesNotThrowForInvalidPathTemplateWithoutAttributeRouting()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers").EntityType.Ignore(c => c.Name);
            IEdmModel model = builder.GetEdmModel();
            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(CustomersController) });
            configuration.MapODataServiceRoute(
                "RouteName",
                "RoutePrefix",
                model,
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault());

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => configuration.EnsureInitialized());
        }

        [Fact]
        public void MapODataServiceRoute_ConfiguresARoute_RelexVersionConstraints()
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
            var odataVersionConstraint = Assert.Single(odataRoute.Constraints.Values.OfType<ODataVersionConstraint>());
            Assert.True(odataVersionConstraint.IsRelaxedMatch);
        }

        private static IEdmModel GetModel(HttpConfiguration config)
        {
            return config.GetODataRootContainer("odata").GetRequiredService<IEdmModel>();
        }

        private static IODataPathHandler GetPathHandler(HttpConfiguration config)
        {
            return config.GetODataRootContainer("odata").GetRequiredService<IODataPathHandler>();
        }

        private static IEnumerable<IODataRoutingConvention> GetRoutingConvetions(HttpConfiguration config)
        {
            return config.GetODataRootContainer("odata").GetServices<IODataRoutingConvention>();
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
#endif
