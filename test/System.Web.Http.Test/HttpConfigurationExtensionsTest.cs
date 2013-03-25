// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class HttpConfigurationExtensionsTest
    {
        [Fact]
        public void BindParameter_GuardClauses()
        {
            HttpConfiguration config = new HttpConfiguration();
            Type type = typeof(TestParameter);
            IModelBinder binder = new Mock<IModelBinder>().Object;

            Assert.ThrowsArgumentNull(() => HttpConfigurationExtensions.BindParameter(null, type, binder), "configuration");
            Assert.ThrowsArgumentNull(() => HttpConfigurationExtensions.BindParameter(config, null, binder), "type");
            Assert.ThrowsArgumentNull(() => HttpConfigurationExtensions.BindParameter(config, type, null), "binder");
        }

        [Fact]
        public void BindParameter_InsertsModelBinderProviderInPositionZero()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Type type = typeof(TestParameter);
            IModelBinder binder = new Mock<IModelBinder>().Object;

            // Act
            config.BindParameter(type, binder);

            // Assert
            SimpleModelBinderProvider provider = config.Services.GetServices(typeof(ModelBinderProvider)).OfType<SimpleModelBinderProvider>().First();
            Assert.Equal(type, provider.ModelType);
        }

        [Fact]
        public void MapHttpAttributeRoutes_DoesNotAddRoutesWithoutTemplate()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>();
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute() };
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            Assert.Empty(config.Routes);
        }

        [Fact]
        public void MapHttpAttributeRoutes_AddsRoutesFromAttributes()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>();
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("controller/{id}") };
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal("controller/{id}", route.RouteTemplate);
            Assert.Equal(route, routes["Controller.Action"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_RespectsPrefix()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { new RoutePrefixAttribute("prefix/{prefixId}") };
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("controller/{id}") };
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal("prefix/{prefixId}/controller/{id}", route.RouteTemplate);
            Assert.Equal(route, routes["Controller.Action"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_RespectsPrefixThatEndsWithAForwardSlash()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { new RoutePrefixAttribute("prefix/{prefixId}/") };
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("controller/{id}") };
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal("prefix/{prefixId}/controller/{id}", route.RouteTemplate);
            Assert.Equal(route, routes["Controller.Action"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_RegistersRouteForActionsWithPrefixButNoRouteInfo()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { new RoutePrefixAttribute("prefix/{prefixId}") };
            var routeProviders = new Collection<IHttpRouteInfoProvider>();
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal("prefix/{prefixId}", route.RouteTemplate);
            Assert.Equal(route, routes["Controller.Action"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_AddsMultipleRoutesFromAttributes()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>();
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("controller/get1"), new HttpGetAttribute("controller/get2") };
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            Assert.Equal(2, routes.Count);
            Assert.Single(routes.Where(route => route.RouteTemplate == "controller/get1"));
            Assert.Single(routes.Where(route => route.RouteTemplate == "controller/get2"));
        }

        [Fact]
        public void MapHttpAttributeRoutes_AddsMultipleRoutesFromAttributesAndPrefixes()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { new RoutePrefixAttribute("prefix1"), new RoutePrefixAttribute("prefix2") };
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("controller/get1"), new HttpGetAttribute("controller/get2") };
            SetupConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            Assert.Equal(4, routes.Count);
            Assert.Single(routes.Where(route => route.RouteTemplate == "prefix1/controller/get1"));
            Assert.Single(routes.Where(route => route.RouteTemplate == "prefix1/controller/get2"));
            Assert.Single(routes.Where(route => route.RouteTemplate == "prefix2/controller/get1"));
            Assert.Single(routes.Where(route => route.RouteTemplate == "prefix2/controller/get2"));
        }

        private static void SetupConfiguration(HttpConfiguration config, Collection<RoutePrefixAttribute> routePrefixes, Collection<IHttpRouteInfoProvider> routeProviders)
        {
            Mock<HttpControllerDescriptor> controllerDescriptor = new Mock<HttpControllerDescriptor>();
            controllerDescriptor.Object.ControllerName = "Controller";
            controllerDescriptor.Setup(cd => cd.GetCustomAttributes<RoutePrefixAttribute>()).Returns(routePrefixes);

            Mock<HttpActionDescriptor> actionDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor.Object);
            actionDescriptor.Setup(ad => ad.ActionName).Returns("Action");
            actionDescriptor.Setup(ad => ad.GetCustomAttributes<IHttpRouteInfoProvider>(false)).Returns(routeProviders);

            Mock<IHttpControllerSelector> controllerSelector = new Mock<IHttpControllerSelector>();
            controllerSelector.Setup(c => c.GetControllerMapping()).Returns(new Dictionary<string, HttpControllerDescriptor>() { { "Controller", controllerDescriptor.Object } });
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector.Object);

            Mock<IHttpActionSelector> actionSelector = new Mock<IHttpActionSelector>();
            actionSelector.Setup(a => a.GetActionMapping(controllerDescriptor.Object)).Returns(new[] { actionDescriptor.Object }.ToLookup(ad => ad.ActionName));
            config.Services.Replace(typeof(IHttpActionSelector), actionSelector.Object);
        }

        public class TestParameter
        {
        }
    }
}
