// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
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
            SetUpConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            Assert.Empty(config.Routes);
        }

        [Theory]
        [InlineData(null, "", "")]
        [InlineData(null, "   ", "   ")]
        [InlineData(null, "controller/{id}", "controller/{id}")]
        [InlineData("", null, "")]
        [InlineData("", "", "")]
        [InlineData("", "   ", "   ")]
        [InlineData("", "controller/{id}", "controller/{id}")]
        [InlineData("   ", null, "   ")]
        [InlineData("   ", "", "   ")]
        [InlineData("   ", "   ", "   /   ")]
        [InlineData("   ", "controller/{id}", "   /controller/{id}")]
        [InlineData("prefix/{prefixId}", null, "prefix/{prefixId}")]
        [InlineData("prefix/{prefixId}", "", "prefix/{prefixId}")]
        [InlineData("prefix/{prefixId}", "   ", "prefix/{prefixId}/   ")]
        [InlineData("prefix/{prefixId}", "controller/{id}", "prefix/{prefixId}/controller/{id}")]
        public void MapHttpAttributeRoutes_AddsRouteFromAttribute(string prefix, string template, string expectedTemplate)
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>();
            if (prefix != null)
            {
                routePrefixes.Add(new RoutePrefixAttribute(prefix));
            }

            var routeProviders = new Collection<IHttpRouteInfoProvider>();
            if (template != null)
            {
                routeProviders.Add(new HttpGetAttribute(template));
            }

            SetUpConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal(expectedTemplate, route.RouteTemplate);
            Assert.Equal(route, routes["Controller.Action"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_ThrowsForRoutePrefixThatEndsWithSeparator()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { new RoutePrefixAttribute("prefix/") };
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { };
            SetUpConfiguration(config, routePrefixes, routeProviders);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => config.MapHttpAttributeRoutes(),
                "The route prefix 'prefix/' on the controller named 'Controller' cannot end with a '/' character.");
        }

        [Fact]
        public void MapHttpAttributeRoutes_ThrowsForRouteTemplateThatStartsWithSeparator()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { };
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("/get") };
            SetUpConfiguration(config, routePrefixes, routeProviders);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => config.MapHttpAttributeRoutes(),
                "The route template '/get' on the action named 'Action' cannot start with a '/' character.");
        }

        [Fact]
        public void MapHttpAttributeRoutes_RegistersRouteForActionsWithPrefixButNoRouteTemplate()
        {
            // Arrange
            var config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { new RoutePrefixAttribute("prefix/{prefixId}") };
            var routeProviders = new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute() };
            SetUpConfiguration(config, routePrefixes, routeProviders);

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
            SetUpConfiguration(config, routePrefixes, routeProviders);

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
            SetUpConfiguration(config, routePrefixes, routeProviders);

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

        [Fact]
        public void MapHttpAttributeRoutes_RespectsRouteOrder()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>() { };
            var routeProviders = new Collection<IHttpRouteInfoProvider>()
                {
                    new HttpGetAttribute("get1") { RouteOrder = 1 },
                    new HttpGetAttribute("get2"),
                    new HttpGetAttribute("get3") { RouteOrder = -1 }
                };
            SetUpConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            Assert.Equal(3, routes.Count);
            Assert.Equal("get3", routes.ElementAt(0).RouteTemplate);
            Assert.Equal("get2", routes.ElementAt(1).RouteTemplate);
            Assert.Equal("get1", routes.ElementAt(2).RouteTemplate);
        }

        [Fact]
        public void MapHttpAttributeRoutes_RespectsRouteOrderAcrossControllers()
        {
            // Arrange
            var config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor1 = CreateControllerDescriptor(config, "Controller1", new Collection<RoutePrefixAttribute>());
            HttpActionDescriptor actionDescriptor1 = CreateActionDescriptor(
                "Action1",
                new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("action1/route1") { RouteOrder = 3 }, new HttpGetAttribute("action1/route2") { RouteOrder = 1 } },
                controllerDescriptor1);
            HttpControllerDescriptor controllerDescriptor2 = CreateControllerDescriptor(config, "Controller2", new Collection<RoutePrefixAttribute>());
            HttpActionDescriptor actionDescriptor2 = CreateActionDescriptor(
                "Action2",
                new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("action2/route1") { RouteOrder = 2 } },
                controllerDescriptor2);
            
            var controllerSelector = CreateControllerSelector(new[] { controllerDescriptor1, controllerDescriptor2 });
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            var actionSelector = CreateActionSelector(
                new Dictionary<HttpControllerDescriptor, IEnumerable<HttpActionDescriptor>>()
                {
                    { controllerDescriptor1, new HttpActionDescriptor[] { actionDescriptor1 } },
                    { controllerDescriptor2, new HttpActionDescriptor[] { actionDescriptor2 } }
                });
            config.Services.Replace(typeof(IHttpActionSelector), actionSelector);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            Assert.Equal(3, routes.Count);
            Assert.Equal("action1/route2", routes.ElementAt(0).RouteTemplate);
            Assert.Equal("action2/route1", routes.ElementAt(1).RouteTemplate);
            Assert.Equal("action1/route1", routes.ElementAt(2).RouteTemplate);
        }

        [Fact]
        public void MapHttpAttributeRoutes_RespectsRoutePrefixOrder()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var routePrefixes = new Collection<RoutePrefixAttribute>()
                {
                    new RoutePrefixAttribute("prefix1") { Order = 1 },
                    new RoutePrefixAttribute("prefix2"),
                    new RoutePrefixAttribute("prefix3") { Order = -1 },
                };
            var routeProviders = new Collection<IHttpRouteInfoProvider>()
                {
                    new HttpGetAttribute("get1") { RouteOrder = 1 },
                    new HttpGetAttribute("get2"),
                    new HttpGetAttribute("get3") { RouteOrder = -1 }
                };
            SetUpConfiguration(config, routePrefixes, routeProviders);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            Assert.Equal(9, routes.Count);
            Assert.Equal("prefix3/get3", routes.ElementAt(0).RouteTemplate);
            Assert.Equal("prefix3/get2", routes.ElementAt(1).RouteTemplate);
            Assert.Equal("prefix3/get1", routes.ElementAt(2).RouteTemplate);
            Assert.Equal("prefix2/get3", routes.ElementAt(3).RouteTemplate);
            Assert.Equal("prefix2/get2", routes.ElementAt(4).RouteTemplate);
            Assert.Equal("prefix2/get1", routes.ElementAt(5).RouteTemplate);
            Assert.Equal("prefix1/get3", routes.ElementAt(6).RouteTemplate);
            Assert.Equal("prefix1/get2", routes.ElementAt(7).RouteTemplate);
            Assert.Equal("prefix1/get1", routes.ElementAt(8).RouteTemplate);
        }

        [Fact]
        public void MapHttpAttributeRoutes_RespectsPerControllerActionSelectors()
        {
            // Arrange
            var globalConfiguration = new HttpConfiguration();
            var _controllerDescriptor = new HttpControllerDescriptor(globalConfiguration, "PerControllerActionSelector", typeof(PerControllerActionSelectorController));

            // Set up the global action selector and controller selector
            var controllerSelector = CreateControllerSelector(new HttpControllerDescriptor[] { _controllerDescriptor });
            globalConfiguration.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            var globalAction = CreateActionDescriptor("Global", new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("Global") }, _controllerDescriptor);
            var globalActionSelector = CreateActionSelector(
                new Dictionary<HttpControllerDescriptor, IEnumerable<HttpActionDescriptor>>()
                    {
                        { _controllerDescriptor, new HttpActionDescriptor[] { globalAction } }
                    });
            globalConfiguration.Services.Replace(typeof(IHttpActionSelector), globalActionSelector);

            // Configure the per controller action selector to return the action with route "PerController"
            var perControllerAction = CreateActionDescriptor(
                "PerController",
                new Collection<IHttpRouteInfoProvider>() { new HttpGetAttribute("PerController") },
                _controllerDescriptor);
            ActionSelectorConfigurationAttribute.PerControllerActionSelectorMock
                .Setup(a => a.GetActionMapping(_controllerDescriptor))
                .Returns(new HttpActionDescriptor[] { perControllerAction }.ToLookup(ad => ad.ActionName));

            // Act
            globalConfiguration.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = globalConfiguration.Routes;
            Assert.Equal("PerController", Assert.Single(routes).RouteTemplate);
        }

        [Fact]
        public void SuppressHostPrincipal_InsertsSuppressHostPrincipalMessageHandler()
        {
            // Arrange
            IHostPrincipalService expectedPrincipalService = new Mock<IHostPrincipalService>(
                MockBehavior.Strict).Object;
            DelegatingHandler existingHandler = new Mock<DelegatingHandler>(MockBehavior.Strict).Object;

            using (HttpConfiguration configuration = new HttpConfiguration())
            {
                configuration.Services.Replace(typeof(IHostPrincipalService), expectedPrincipalService);
                configuration.MessageHandlers.Add(existingHandler);

                // Act
                configuration.SuppressHostPrincipal();

                // Assert
                Assert.Equal(2, configuration.MessageHandlers.Count);
                DelegatingHandler firstHandler = configuration.MessageHandlers[0];
                Assert.IsType<SuppressHostPrincipalMessageHandler>(firstHandler);
                SuppressHostPrincipalMessageHandler suppressPrincipalHandler =
                    (SuppressHostPrincipalMessageHandler)firstHandler;
                IHostPrincipalService principalService = suppressPrincipalHandler.HostPrincipalService;
                Assert.Same(expectedPrincipalService, principalService);
            }
        }

        [Fact]
        public void SuppressHostPrincipal_Throws_WhenConfigurationIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => { HttpConfigurationExtensions.SuppressHostPrincipal(null); },
                "configuration");
        }

        private static void SetUpConfiguration(HttpConfiguration config, Collection<RoutePrefixAttribute> routePrefixes, Collection<IHttpRouteInfoProvider> routeProviders)
        {
            HttpControllerDescriptor controllerDescriptor = CreateControllerDescriptor(config, "Controller", routePrefixes);
            HttpActionDescriptor actionDescriptor = CreateActionDescriptor("Action", routeProviders, controllerDescriptor);

            var controllerSelector = CreateControllerSelector(new[] { controllerDescriptor });
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            var actionSelector = CreateActionSelector(
                new Dictionary<HttpControllerDescriptor, IEnumerable<HttpActionDescriptor>>()
                {
                    { controllerDescriptor, new HttpActionDescriptor[] { actionDescriptor } }
                });
            config.Services.Replace(typeof(IHttpActionSelector), actionSelector);
        }

        private static HttpControllerDescriptor CreateControllerDescriptor(HttpConfiguration configuration, string controllerName,
            Collection<RoutePrefixAttribute> routePrefixes)
        {
            Mock<HttpControllerDescriptor> controllerDescriptor = new Mock<HttpControllerDescriptor>();
            controllerDescriptor.Object.Configuration = configuration;
            controllerDescriptor.Object.ControllerName = controllerName;
            controllerDescriptor.Setup(cd => cd.GetCustomAttributes<RoutePrefixAttribute>(false)).Returns(routePrefixes);
            return controllerDescriptor.Object;
        }

        private static HttpActionDescriptor CreateActionDescriptor(string actionName, Collection<IHttpRouteInfoProvider> routeProviders,
            HttpControllerDescriptor controllerDescriptor)
        {
            Mock<HttpActionDescriptor> actionDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            actionDescriptor.Setup(ad => ad.ActionName).Returns(actionName);
            actionDescriptor.Setup(ad => ad.GetCustomAttributes<IHttpRouteInfoProvider>(false)).Returns(routeProviders);
            return actionDescriptor.Object;
        }

        private static IHttpControllerSelector CreateControllerSelector(IEnumerable<HttpControllerDescriptor> controllerDescriptors)
        {
            Mock<IHttpControllerSelector> controllerSelector = new Mock<IHttpControllerSelector>();
            controllerSelector.Setup(c => c.GetControllerMapping()).Returns(controllerDescriptors.ToDictionary(cd => cd.ControllerName));
            return controllerSelector.Object;
        }

        private static IHttpActionSelector CreateActionSelector(Dictionary<HttpControllerDescriptor, IEnumerable<HttpActionDescriptor>> actionMap)
        {
            Mock<IHttpActionSelector> actionSelector = new Mock<IHttpActionSelector>();
            foreach (var mapEntry in actionMap)
            {
                actionSelector.Setup(a => a.GetActionMapping(mapEntry.Key)).Returns(mapEntry.Value.ToLookup(ad => ad.ActionName));
            }
            return actionSelector.Object;
        }

        public class TestParameter
        {
        }
        
        [ActionSelectorConfiguration]
        public class PerControllerActionSelectorController : ApiController { }

        public class ActionSelectorConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public static Mock<IHttpActionSelector> PerControllerActionSelectorMock = new Mock<IHttpActionSelector>();

            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), PerControllerActionSelectorMock.Object);
            }
        }
    }
}
