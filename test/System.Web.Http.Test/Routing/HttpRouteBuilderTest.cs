// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
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
            var route = BuildRoute(@"hello/{param:regex(\d+)=8675309}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            
            Assert.Equal("8675309", route.Defaults["param"]);
            
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraintAndOptional()
        {
            var route = BuildRoute(@"hello/{param:regex(\d+)?}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            
            Assert.Equal(RouteParameter.Optional, route.Defaults["param"]);
            
            Assert.IsType<OptionalHttpRouteConstraint>(route.Constraints["param"]);
            var constraint = (OptionalHttpRouteConstraint)route.Constraints["param"];
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)constraint.InnerConstraint).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ChainedConstraints()
        {
            var route = BuildRoute(@"hello/{param:regex(\d+):regex(\w+)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            
            Assert.IsType<CompoundHttpRouteConstraint>(route.Constraints["param"]);
            var constraint = (CompoundHttpRouteConstraint)route.Constraints["param"];
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)constraint.Constraints.ElementAt(0)).Pattern);
            Assert.Equal(@"\w+", ((RegexHttpRouteConstraint)constraint.Constraints.ElementAt(1)).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_Constraint()
        {
            var route = BuildRoute(@"hello/{param:regex(\d+)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_ConstraintsDefaultsAndOptionalsInMultipleSections()
        {
            var route = BuildRoute(@"some/url-{p1:regex(\d+):regex(\w+)=hello}/{p2=abc}/{p3?}");

            Assert.Equal("some/url-{p1}/{p2}/{p3}", route.RouteTemplate);
            
            Assert.Equal("hello", route.Defaults["p1"]);
            Assert.Equal("abc", route.Defaults["p2"]);
            Assert.Equal(RouteParameter.Optional, route.Defaults["p3"]);

            Assert.IsType<CompoundHttpRouteConstraint>(route.Constraints["p1"]);
            var constraint = (CompoundHttpRouteConstraint)route.Constraints["p1"];
            Assert.Equal(@"\d+", ((RegexHttpRouteConstraint)constraint.Constraints.ElementAt(0)).Pattern);
            Assert.Equal(@"\w+", ((RegexHttpRouteConstraint)constraint.Constraints.ElementAt(1)).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_NoTokens()
        {
            var route = BuildRoute("hello/world");

            Assert.Equal("hello/world", route.RouteTemplate);
        }

        [Fact]
        public void BuildHttpRoute_OptionalParam()
        {
            var route = BuildRoute("hello/{param?}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal(RouteParameter.Optional, route.Defaults["param"]);
        }

        [Fact]
        public void BuildHttpRoute_ParamDefault()
        {
            var route = BuildRoute("hello/{param=world}");

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal("world", route.Defaults["param"]);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithClosingBraceInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(\})}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\}", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithClosingParenInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(\))}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\)", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithColonInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(:)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@":", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithEqualsSignInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(=)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.DoesNotContain("param", route.Defaults.Keys);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"=", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithOpenBraceInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(\{)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\{", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithOpenParenInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(\()}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\(", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        [Fact]
        public void BuildHttpRoute_RegexConstraintWithQuestionMarkInPattern()
        {
            var route = BuildRoute(@"hello/{param:regex(\?)}");

            Assert.Equal("hello/{param}", route.RouteTemplate);

            Assert.DoesNotContain("param", route.Defaults.Keys);
            
            Assert.IsType<RegexHttpRouteConstraint>(route.Constraints["param"]);
            Assert.Equal(@"\?", ((RegexHttpRouteConstraint)route.Constraints["param"]).Pattern);
        }

        private static IHttpRoute BuildRoute(string routeTemplate)
        {
            // Arrange
            var builder = new HttpRouteBuilder();
            var provider = new FakeRouteProvider(routeTemplate);

            // Act
            var route = builder.BuildHttpRoute(provider, "FakeController", "FakeAction");

            // Assertions for default, unspecified behavior:
            Assert.NotNull(route);
            Assert.Equal("FakeController", route.Defaults["controller"]);
            Assert.Equal("FakeAction", route.Defaults["action"]);
            Assert.IsType<HttpMethodConstraint>(route.Constraints["methodConstraint"]);

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
