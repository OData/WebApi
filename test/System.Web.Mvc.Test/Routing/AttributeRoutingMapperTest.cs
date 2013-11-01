// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Mvc.Async;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Routing
{
    public class AttributeRoutingMapperTest
    {
        [Fact]
        public void MapMvcAttributeRoutes_DoesNotTryToInferRouteNames()
        {
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MyController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder2());

            var routeEntries = mapper.MapAttributeRoutes(controllerDescriptor);

            var routeEntry = Assert.Single(routeEntries);
            Assert.Null(routeEntry.Name);
        }

        [Fact]
        public void MapMvcAttributeRoutes_RespectsActionNameAttribute()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MyController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder2());

            // Act
            var routeEntries = mapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            var routeEntry = Assert.Single(routeEntries);
            Assert.Equal("ActionName", routeEntry.Route.Defaults["action"]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(AnotherController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder2());

            // Act
            var entries = mapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            var controllerEntry = Assert.Single(entries.Where(r => !r.Route.Defaults.ContainsKey("action")));
            Assert.Same(controllerDescriptor, controllerEntry.Route.GetTargetControllerDescriptor());

            var actionMethods = controllerEntry.Route.GetTargetActionDescriptors().ToArray();
            Assert.Equal(2, actionMethods.Length);
            Assert.Single(actionMethods, a => a.ActionName == "RegularAction");
            Assert.Single(actionMethods, a => a.ActionName == "AnotherAction");
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute_AndNoReachableActions()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(NoActionsController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder2());

            // Act
            var entries = mapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            Assert.Empty(entries);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute_ExcludesAttributeRoute()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MixedRoutingController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder2());

            // Act
            var entries = mapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            var controllerEntry = Assert.Single(entries.Where(r => !r.Route.Defaults.ContainsKey("action")));
            Assert.Same(controllerDescriptor, controllerEntry.Route.GetTargetControllerDescriptor());

            var actionMethods = controllerEntry.Route.GetTargetActionDescriptors().ToArray();
            Assert.Equal(1, actionMethods.Length);
            Assert.Single(actionMethods, a => a.ActionName == "GoodAction");

            var actionEntry = Assert.Single(entries.Where(r => r.Route.Defaults.ContainsKey("action")));
            Assert.Equal("DirectRouteAction", Assert.Single(actionEntry.Route.GetTargetActionDescriptors()).ActionName);
        }

        public class MyController : Controller
        {
            [HttpGet]
            [Route("")]
            [ActionName("ActionName")]
            public void MethodName()
            {
            }
        }

        [Route("controller/{action}")]
        public class AnotherController : Controller
        {
            public void RegularAction()
            {
            }

            public void AnotherAction()
            {
            }
        }

        [Route("controller/{action}")]
        public class NoActionsController : Controller
        {
        }

        [Route("controller/{action}")]
        public class MixedRoutingController : Controller
        {
            [Route("Yep")]
            public void DirectRouteAction()
            {
            }

            public void GoodAction()
            {
            }
        }
    }
}
