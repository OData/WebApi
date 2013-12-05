// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Async;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Routing
{
    public class AttributeRoutingMapperTest
    {
        [Fact]
        public void MapMvcAttributeRoutes_DoesNotTryToInferRouteNames()
        {
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MyController));

            var routeEntries = AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor);

            var routeEntry = Assert.Single(routeEntries);
            Assert.Null(routeEntry.Name);
        }

        [Fact]
        public void MapMvcAttributeRoutes_RespectsActionNameAttribute()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MyController));

            // Act
            var routeEntries = AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            var routeEntry = Assert.Single(routeEntries);
            Assert.Equal("ActionName", routeEntry.Route.Defaults["action"]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(AnotherController));

            // Act
            var entries = AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor);

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

            // Act
            var entries = AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            Assert.Empty(entries);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute_ExcludesAttributeRoute()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MixedRoutingController));

            // Act
            var entries = AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            var controllerEntry = Assert.Single(entries.Where(r => !r.Route.Defaults.ContainsKey("action")));
            Assert.Same(controllerDescriptor, controllerEntry.Route.GetTargetControllerDescriptor());

            var actionMethods = controllerEntry.Route.GetTargetActionDescriptors().ToArray();
            Assert.Equal(1, actionMethods.Length);
            Assert.Single(actionMethods, a => a.ActionName == "GoodAction");

            var actionEntry = Assert.Single(entries.Where(r => r.Route.Defaults.ContainsKey("action")));
            Assert.Equal("DirectRouteAction", Assert.Single(actionEntry.Route.GetTargetActionDescriptors()).ActionName);
        }

        [Fact]
        public void MapMvcAttributeRoutes_SetsTargetIsAction()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MixedRoutingController));

            // Act
            var entries = AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor);

            // Assert
            var controllerEntry = Assert.Single(entries.Where(r => !r.Route.Defaults.ContainsKey("action")));
            Assert.False(controllerEntry.Route.GetTargetIsAction());

            var actionEntry = Assert.Single(entries.Where(r => r.Route.Defaults.ContainsKey("action")));
            Assert.True(actionEntry.Route.GetTargetIsAction());
        }

        [Fact]
        public void MapMvcAttributeRoutes_ValidatesConstraints()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(InvalidConstraintController));

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template 'invalidconstraint/{action}' " +
                "must have a string value or be of a type which implements 'System.Web.Routing.IRouteConstraint'.";


            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.MapAttributeRoutes(controllerDescriptor), expectedMessage);
        }


        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsNull_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            IDirectRouteFactory factory = CreateStubRouteFactory(null);
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false),
                "IDirectRouteFactory.CreateRoute must not return null.");
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithoutActionDescriptors_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            Route route = new Route(url: null, routeHandler: null);
            Assert.Null(route.GetTargetActionDescriptors()); // Guard
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false), expectedMessage);
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithEmptyActionDescriptors_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            Route route = new Route(url: null, routeHandler: null);
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add(RouteDataTokenKeys.Actions, new ActionDescriptor[0]);
            ActionDescriptor[] originalActions = route.GetTargetActionDescriptors();
            Assert.NotNull(originalActions); // Guard
            Assert.Equal(0, originalActions.Length); // Guard
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false), expectedMessage);
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithHandler_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            Route route = new Route(url: null, routeHandler: null);
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add(RouteDataTokenKeys.Actions, new ActionDescriptor[] { actionDescriptor });
            route.RouteHandler = new Mock<IRouteHandler>(MockBehavior.Strict).Object;
            ActionDescriptor[] originalActions = route.GetTargetActionDescriptors();
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "Direct routing does not support per-route route handlers.";
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false), expectedMessage);
        }

        private static ActionDescriptor CreateStubActionDescriptor(ControllerDescriptor controllerDescriptor, string actionName)
        {
            Mock<ActionDescriptor> mock = new Mock<ActionDescriptor>(MockBehavior.Strict);
            mock.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor);
            mock.SetupGet(d => d.ActionName).Returns(actionName);
            return mock.Object;
        }

        private static ControllerDescriptor CreateStubControllerDescriptor(string controllerName)
        {
            Mock<ControllerDescriptor> mock = new Mock<ControllerDescriptor>(MockBehavior.Strict);
            mock.SetupGet(d => d.ControllerName).Returns(controllerName);
            return mock.Object;
        }

        private static IDirectRouteFactory CreateStubRouteFactory(RouteEntry entry)
        {
            Mock<IDirectRouteFactory> mock = new Mock<IDirectRouteFactory>(MockBehavior.Strict);
            mock.Setup(p => p.CreateRoute(It.IsAny<DirectRouteFactoryContext>())).Returns(entry);
            return mock.Object;
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

        [InvalidConstraintRoute("invalidconstraint/{action}")]
        public class InvalidConstraintController : Controller
        {
            public void A1()
            {
            }
        }

        public class InvalidConstraintRouteAttribute : RouteFactoryAttribute
        {
            public InvalidConstraintRouteAttribute(string template)
                : base(template)
            {
            }

            public override RouteValueDictionary Constraints
            {
                get
                {
                    var result = new RouteValueDictionary();
                    result.Add("custom", new Uri("http://localhost"));
                    return result;
                }
            }
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
