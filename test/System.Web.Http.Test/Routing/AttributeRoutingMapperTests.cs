// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
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
            IDirectRouteProvider provider = CreateStubRouteProvider(null);
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(prefix, provider,
                actions, constraintResolver), "IDirectRouteProvider.CreateRoute must not return null.");
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithoutActionDescriptors_Throws()
        {
            // Arrange
            string prefix = null;
            IHttpRoute route = new Mock<IHttpRoute>().Object;
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteProvider provider = CreateStubRouteProvider(entry);
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(prefix, provider,
                actions, constraintResolver), expectedMessage);
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
            IDirectRouteProvider provider = CreateStubRouteProvider(entry);
            HttpActionDescriptor actionDescriptor = CreateStubActionDescriptor("IgnoreAction");
            IReadOnlyCollection<HttpActionDescriptor> actions = new HttpActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.CreateRouteEntry(prefix, provider,
                actions, constraintResolver), expectedMessage);
        }

        private static HttpActionDescriptor CreateStubActionDescriptor(string actionName)
        {
            Mock<HttpActionDescriptor> mock = new Mock<HttpActionDescriptor>(MockBehavior.Strict);
            mock.SetupGet(d => d.ActionName).Returns(actionName);
            return mock.Object;
        }

        private static IDirectRouteProvider CreateStubRouteProvider(RouteEntry entry)
        {
            Mock<IDirectRouteProvider> mock = new Mock<IDirectRouteProvider>(MockBehavior.Strict);
            mock.Setup(p => p.CreateRoute(It.IsAny<DirectRouteProviderContext>())).Returns(entry);
            return mock.Object;
        }
    }
}
