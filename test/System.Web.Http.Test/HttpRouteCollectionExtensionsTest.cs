// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpRouteCollectionExtensionsTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(HttpRouteCollectionExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void MapHttpRoute1ThrowsOnNullRouteCollection()
        {
            Assert.ThrowsArgumentNull(() => HttpRouteCollectionExtensions.MapHttpRoute(null, "", "", null), "routes");
        }

        [Fact]
        public void MapHttpRoute1CreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new { d1 = "D1" };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute1WithDefaultsAsDictionaryCreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new Dictionary<string, object> { { "d1", "D1" } };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute2ThrowsOnNullRouteCollection()
        {
            Assert.ThrowsArgumentNull(() => HttpRouteCollectionExtensions.MapHttpRoute(null, "", "", null, null), "routes");
        }

        [Fact]
        public void MapHttpRoute2CreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new { d1 = "D1" };
            object constraints = new { c1 = "C1" };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults, constraints);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("C1", route.Constraints["c1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute2WithDefaultsAndConstraintsAsDictionaryCreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new Dictionary<string, object> { { "d1", "D1" } };
            object constraints = new Dictionary<string, object> { { "c1", "C1" } };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults, constraints);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("C1", route.Constraints["c1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_AddsRoutesFromAttributes()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor() { ControllerName = "Controller" };
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(AttributedController).GetMethod("Get"));

            Mock<IHttpControllerSelector> controllerSelector = new Mock<IHttpControllerSelector>();
            controllerSelector.Setup(c => c.GetControllerMapping()).Returns(new Dictionary<string, HttpControllerDescriptor>() { { "Controller", controllerDescriptor } });

            Mock<IHttpActionSelector> actionSelector = new Mock<IHttpActionSelector>();
            actionSelector.Setup(a => a.GetActionMapping(controllerDescriptor)).Returns(new[] { actionDescriptor }.ToLookup(ad => ad.ActionName));

            // Act
            routes.MapHttpAttributeRoutes(controllerSelector.Object, actionSelector.Object);

            // Assert
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal("Controller/{id}", route.RouteTemplate);
        }
    }
}
