// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
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
        public void MapHttpAttributeRoutes_AddsRoutesFromAttributes()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor() { ControllerName = "Controller" };
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(AttributedController).GetMethod("Get"));

            Mock<IHttpControllerSelector> controllerSelector = new Mock<IHttpControllerSelector>();
            controllerSelector.Setup(c => c.GetControllerMapping()).Returns(new Dictionary<string, HttpControllerDescriptor>() { { "Controller", controllerDescriptor } });
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector.Object);

            Mock<IHttpActionSelector> actionSelector = new Mock<IHttpActionSelector>();
            actionSelector.Setup(a => a.GetActionMapping(controllerDescriptor)).Returns(new[] { actionDescriptor }.ToLookup(ad => ad.ActionName));
            config.Services.Replace(typeof(IHttpActionSelector), actionSelector.Object);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route = Assert.Single(routes);
            Assert.Equal("Controller/{id}", route.RouteTemplate);
            Assert.Equal(route, routes["Controller.Get"]);
        }

        [Fact]
        public void MapHttpAttributeRoutes_AddsRoutesFromAttributes_WithDisambiguatingRouteName()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor() { ControllerName = "Controller" };
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(AttributedController).GetMethod("MultipleGet"));

            Mock<IHttpControllerSelector> controllerSelector = new Mock<IHttpControllerSelector>();
            controllerSelector.Setup(c => c.GetControllerMapping()).Returns(new Dictionary<string, HttpControllerDescriptor>() { { "Controller", controllerDescriptor } });
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector.Object);

            Mock<IHttpActionSelector> actionSelector = new Mock<IHttpActionSelector>();
            actionSelector.Setup(a => a.GetActionMapping(controllerDescriptor)).Returns(new[] { actionDescriptor }.ToLookup(ad => ad.ActionName));
            config.Services.Replace(typeof(IHttpActionSelector), actionSelector.Object);

            // Act
            config.MapHttpAttributeRoutes();

            // Assert
            HttpRouteCollection routes = config.Routes;
            IHttpRoute route1 = routes.First(route => route.RouteTemplate == "Controller/Get1");
            Assert.Equal(route1, routes["Controller.MultipleGet1"]);
            IHttpRoute route2 = routes.First(route => route.RouteTemplate == "Controller/Get2");
            Assert.Equal(route2, routes["Controller.MultipleGet2"]);
        }

        public class TestParameter
        {
        }
    }
}
