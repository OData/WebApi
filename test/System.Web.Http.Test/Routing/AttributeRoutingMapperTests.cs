// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class AttributeRoutingMapperTests
    {
        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsNull_Throws()
        {
            // Arrange
            string prefix = null;
            IDirectRouteFactory factory = CreateStubRouteFactory(null);
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.CreateRouteEntry(prefix, factory, actions, constraintResolver, targetIsAction: true), 
                "IDirectRouteFactory.CreateRoute must not return null.");
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithoutActionDescriptors_Throws()
        {
            // Arrange
            string prefix = null;
            IHttpRoute route = new Mock<IHttpRoute>().Object;
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.CreateRouteEntry(prefix, factory, actions, constraintResolver, targetIsAction: true), 
                expectedMessage);
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithEmptyActionDescriptors_Throws()
        {
            // Arrange
            string prefix = null;
            HttpRouteValueDictionary dataTokens = new HttpRouteValueDictionary
            {
                { RouteDataTokenKeys.Actions, new HttpActionDescriptor[0] }
            };
            HttpRoute route = new HttpRoute(null, null, null, dataTokens);
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.CreateRouteEntry(prefix, factory, actions, constraintResolver, targetIsAction: true), 
                expectedMessage);
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithHandler_Throws()
        {
            // Arrange
            string prefix = null;
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            HttpRouteValueDictionary dataTokens = new HttpRouteValueDictionary
            {
                { RouteDataTokenKeys.Actions, new HttpActionDescriptor[] { actionDescriptor } }
            };
            HttpMessageHandler handler = new Mock<HttpMessageHandler>(MockBehavior.Strict).Object;
            HttpRoute route = new HttpRoute(null, null, null, dataTokens, handler);
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "Direct routing does not support per-route message handlers.";
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(prefix, factory,
                actions, constraintResolver, targetIsAction: true), expectedMessage);
        }

        [Fact]
        public void GetRoutePrefix_WithMultiRoutePrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpControllerDescriptor = new MultiRoutePrefixControllerDescripter();
            var typeMock = new Mock<Type>();
            typeMock.SetupGet(t => t.FullName).Returns("Namespace.TypeFullName");
            httpControllerDescriptor.ControllerType = typeMock.Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.GetRoutePrefix(httpControllerDescriptor),
                "Only one route prefix attribute is supported. Remove extra attributes from the controller of type 'Namespace.TypeFullName'.");
        }

        [Fact]
        public void GetRoutePrefix_WithNullPrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpControllerDescriptor = new NullRoutePrefixControllerDescripter();
            var typeMock = new Mock<Type>();
            typeMock.SetupGet(t => t.FullName).Returns("Namespace.TypeFullName");
            httpControllerDescriptor.ControllerType = typeMock.Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.GetRoutePrefix(httpControllerDescriptor),
                "The property 'prefix' from route prefix attribute on controller of type 'Namespace.TypeFullName' cannot be null.");
        }

        private static HttpActionDescriptor CreateStubActionDescriptor(string actionName)
        {
            Mock<HttpActionDescriptor> mock = new Mock<HttpActionDescriptor>(MockBehavior.Strict);
            mock.SetupGet(d => d.ActionName).Returns(actionName);
            return mock.Object;
        }

        private static IDirectRouteFactory CreateStubRouteFactory(RouteEntry entry)
        {
            Mock<IDirectRouteFactory> mock = new Mock<IDirectRouteFactory>(MockBehavior.Strict);
            mock.Setup(p => p.CreateRoute(It.IsAny<DirectRouteFactoryContext>())).Returns(entry);
            return mock.Object;
        }

        private class MultiRoutePrefixControllerDescripter : HttpControllerDescriptor
        {
            public override Collection<T> GetCustomAttributes<T>(bool inherit)
            {
                object[] attributes = new object[] { new ExtendedRoutePrefixAttribute(), new RoutePrefixAttribute("Prefix") };
                return new Collection<T>(TypeHelper.OfType<T>(attributes));
            }
        }

        private class NullRoutePrefixControllerDescripter : HttpControllerDescriptor
        {
            public override Collection<T> GetCustomAttributes<T>(bool inherit)
            {
                object[] attributes = new object[] { new ExtendedRoutePrefixAttribute() };
                return new Collection<T>(TypeHelper.OfType<T>(attributes));
            }
        }

        private class ExtendedRoutePrefixAttribute : RoutePrefixAttribute
        {
        }
    }
}
