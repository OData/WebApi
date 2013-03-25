// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing.Constraints;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class HttpRouteBuilderTest
    {
        [Fact]
        public void BuildHttpRoute_ChainedConstraintAndDefault()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:int=8675309}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal("8675309", route.Defaults["param"]);
            Assert.IsType<IntHttpRouteConstraint>(route.Constraints["param"]);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraintWithArgumentsAndDefault()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\d+)=8675309}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal("8675309", route.Defaults["param"]);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraintAndOptional()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:int?}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.Equal(RouteParameter.Optional, route.Defaults["param"]);

            Assert.IsType<OptionalHttpRouteConstraint>(route.Constraints["param"]);
            var constraint = (OptionalHttpRouteConstraint)route.Constraints["param"];
            Assert.IsType<IntHttpRouteConstraint>(constraint.InnerConstraint);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraintWithArgumentsAndOptional()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\d+)?}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.Equal(RouteParameter.Optional, route.Defaults["param"]);

            Assert.IsType<OptionalHttpRouteConstraint>(route.Constraints["param"]);
            var constraint = (OptionalHttpRouteConstraint)route.Constraints["param"];
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)constraint.InnerConstraint).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraints()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\d+):regex(\w+)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            
            Assert.IsType<CompoundHttpRouteConstraint>(route.Constraints["param"]);
            CompoundHttpRouteConstraint constraint = (CompoundHttpRouteConstraint)route.Constraints["param"];
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)constraint.Constraints.ElementAt(0)).Pattern);
            Assert.Equal(@"\w+", ((RegexHttpRouteConstraint)constraint.Constraints.ElementAt(1)).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_Constraint()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\d+)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ConstraintsDefaultsAndOptionalsInMultipleSections()
        {
            IHttpRoute route = BuildRoute(@"some/url-{p1:alpha:length(3)=hello}/{p2=abc}/{p3?}");

            Assert.Equal("some/url-{p1}/{p2}/{p3}", route.RouteTemplate);
            
            Assert.Equal("hello", route.Defaults["p1"]);
            Assert.Equal("abc", route.Defaults["p2"]);
            Assert.Equal(RouteParameter.Optional, route.Defaults["p3"]);

            Assert.IsType<CompoundHttpRouteConstraint>(route.Constraints["p1"]);
            CompoundHttpRouteConstraint constraint = (CompoundHttpRouteConstraint)route.Constraints["p1"];
            Assert.IsType<AlphaHttpRouteConstraint>(constraint.Constraints.ElementAt(0));
            Assert.IsType<LengthHttpRouteConstraint>(constraint.Constraints.ElementAt(1));
        }

        [Fact]
        public void BuildHttpRoute_NoTokens()
        {
            IHttpRoute route = BuildRoute("hello/world");

            Assert.Equal("hello/world", route.RouteTemplate);
        }

        [Fact]
        public void BuildHttpRoute_OptionalParam()
        {
            IHttpRoute route = BuildRoute("hello/{param?}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal(RouteParameter.Optional, route.Defaults["param"]);
        }

        [Fact]
        public void BuildHttpRoute_ParamDefault()
        {
            IHttpRoute route = BuildRoute("hello/{param=world}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal("world", route.Defaults["param"]);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithClosingBraceInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\})}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\}", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithClosingParenInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\))}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\)", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithColonInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(:)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@":", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithCommaInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\w,\w)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\w,\w", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithEqualsSignInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(=)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.DoesNotContain("param", route.Defaults.Keys);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"=", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithOpenBraceInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\{)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\{", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithOpenParenInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\()}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\(", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithQuestionMarkInPattern()
        {
            IHttpRoute route = BuildRoute(@"hello/{param:regex(\?)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.DoesNotContain("param", route.Defaults.Keys);
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\?", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

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
            // Act
            HttpRouteBuilder routeBuilder = new HttpRouteBuilder(constraintResolver ?? new DefaultInlineConstraintResolver());
            IHttpRoute route = routeBuilder.BuildHttpRoute(routeTemplate, new HttpMethod[] { HttpMethod.Get }, "FakeController", "FakeAction");

            // Assertions for default, unspecified behavior:
            Assert.NotNull(route);
            Assert.Equal("FakeController", route.Defaults["controller"]);
            Assert.Equal("FakeAction", route.Defaults["action"]);
            Assert.IsType<HttpMethodConstraint>(route.Constraints["httpMethod"]);

            return route;
        }
    }
}
