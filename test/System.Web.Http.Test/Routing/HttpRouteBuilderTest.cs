// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class HttpRouteBuilderTest
    {
        [Fact]
        public void BuildHttpRoute_Throws_WhenConstraintResolverReturnsNull()
        {
            Mock<IInlineConstraintResolver> constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(r => r.ResolveConstraint("constraint")).Returns<IHttpRouteConstraint>(null);

            Assert.Throws<InvalidOperationException>(
                () => BuildRoute(@"hello/{param:constraint}", constraintResolver: constraintResolver.Object),
                "The inline constraint resolver of type 'IInlineConstraintResolverProxy' was unable to resolve the following inline constraint: 'constraint'.");
        }

        [Fact]
        public void BuildHttpRoute_ResolvesConstraintUsingConstraintResolver()
        {
            IHttpRouteConstraint routeConstraint = new Mock<IHttpRouteConstraint>().Object;
            Mock<IInlineConstraintResolver> constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(r => r.ResolveConstraint("constraint")).Returns(routeConstraint);

            var route = BuildRoute(@"hello/{param:constraint}", constraintResolver: constraintResolver.Object);

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal(routeConstraint, route.Constraints["param"]);
        }

        private static IHttpRoute BuildRoute(string routeTemplate, IInlineConstraintResolver constraintResolver = null)
        {
            ReflectedHttpActionDescriptor[] actions = new ReflectedHttpActionDescriptor[0];

            // Act
            HttpRouteBuilder routeBuilder = new HttpRouteBuilder(constraintResolver ?? new DefaultInlineConstraintResolver());
            IHttpRoute route = routeBuilder.BuildHttpRoute(routeTemplate, new HttpMethod[] { HttpMethod.Get }, actions: actions);

            // Assertions for default, unspecified behavior:
            Assert.NotNull(route);
            Assert.Same(actions, route.DataTokens["actions"]);
            Assert.IsType<HttpMethodConstraint>(route.Constraints["httpMethod"]);

            return route;
        }
    }
}
