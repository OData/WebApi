// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class HttpRouteBuilderTests
    {
        [Fact]
        public void BuildHttpRoute_ChainedConstraintAndDefault()
        {
            IHttpRoute route = BuildRoute(@"hello/{param=8675309:regex(\d+)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            
            Assert.Equal("8675309", route.Defaults["param"]);
            
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraintAndOptional()
        {
            IHttpRoute route = BuildRoute(@"hello/{param?:regex(\d+)}");

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
            IHttpRoute route = BuildRoute(@"some/url-{p1=hello:alpha:length(3)}/{p2=abc}/{p3?}");

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

        private static IHttpRoute BuildRoute(string routeTemplate)
        {
            // Arrange
            IHttpRouteProvider provider = new FakeRouteProvider(routeTemplate);

            // Act
            IHttpRoute route = HttpRouteBuilder.BuildHttpRoute(provider, "FakeController", "FakeAction");

            // Assertions for default, unspecified behavior:
            Assert.NotNull(route);
            Assert.Equal("FakeController", route.Defaults["controller"]);
            Assert.Equal("FakeAction", route.Defaults["action"]);
            Assert.IsType<HttpMethodConstraint>(route.Constraints["httpMethod"]);

            return route;
        }

        private class FakeRouteProvider : IHttpRouteProvider
        {
            public FakeRouteProvider(string routeTemplate)
            {
                HttpMethods = new Collection<HttpMethod> { HttpMethod.Get };
                RouteTemplate = routeTemplate;
            }

            public Collection<HttpMethod> HttpMethods { get; private set; }
            
            public string RouteName { get; private set; }
            
            public string RouteTemplate { get; private set; }
        }
    }
}
