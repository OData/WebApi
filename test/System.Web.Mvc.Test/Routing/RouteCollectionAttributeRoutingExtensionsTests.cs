// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Mvc.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Routing
{
    public class RouteCollectionAttributeRoutingExtensionsTests
    {
        [Fact]
        public void MapMvcAttributeRoutes_MapRouteAttributes()
        {
            // Arrange
            var controllerTypes = new[] { typeof(SimpleRoutingController) };
            var routes = new RouteCollection();

            // Act
            routes.MapMvcAttributeRoutes(controllerTypes);

            // Assert
            var expectedResults = new List<Tuple<string, string, string[]>>
             {
                 new Tuple<string, string, string[]>("getme", "GetMe", new[] { "GET" }),
                 new Tuple<string, string, string[]>("postme", "PostMe", new[] { "POST" }),
                 new Tuple<string, string, string[]>("getorpostme", "GetOrPostMe", new[] { "GET", "POST" }),
                 new Tuple<string, string, string[]>("routeme", "RouteMe", null),
                 new Tuple<string, string, string[]>("once", "FoolMe", new[] { "GET" }),
                 new Tuple<string, string, string[]>("twice", "FoolMe", new[] { "GET" }),
                 new Tuple<string, string, string[]>("twice", "FoolMe", new[] { "GET" }),
             };

            foreach (var expected in expectedResults)
            {
                var url = expected.Item1;
                var methodName = expected.Item2;
                var expectedHttpMethods = expected.Item3;
                Route route = routes.Cast<Route>().Single(r => r.Url == url);
                Assert.Equal(methodName, route.GetTargetActionMethod().Name);
                var httpMethodConstraint = (HttpMethodConstraint)route.Constraints["httpMethod"];
                if (expectedHttpMethods == null)
                {
                    Assert.Null(httpMethodConstraint);
                }
                else
                {
                    Assert.NotNull(httpMethodConstraint);

                    var actualHttpMethods = httpMethodConstraint.AllowedMethods.ToArray();
                    Array.Sort(expectedHttpMethods);
                    Array.Sort(actualHttpMethods);
                    Assert.Equal(expectedHttpMethods, actualHttpMethods);
                }
            }
        }

        [Fact]
        public void MapMvcAttributeRoutes_CustomConstraintResolver()
        {
            // Arrange
            var controllerTypes = new[] { typeof(FruitConstraintController) };
            var routes = new RouteCollection();

            // Act
            routes.MapMvcAttributeRoutes(controllerTypes, new FruitConstraintResolver());

            // Assert
            Assert.Equal(1, routes.Count);
            Route route = (Route)routes.Single();

            Assert.Equal("fruits/{apple}", route.Url);
            Assert.IsAssignableFrom<FruitConstraint>(route.Constraints["apple"]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithInlineConstraints_ParseConstraintsDefaultsAndOptionals()
        {
            // Arrange
            var controllerTypes = new[] { typeof(SimpleRoutingController) };
            var routes = new RouteCollection();

            // Act
            routes.MapMvcAttributeRoutes(controllerTypes);

            // Assert
            Route route = routes.Cast<Route>().Single(r => r.GetTargetActionMethod().Name == "Parameterized");
            Assert.NotNull(route);

            Assert.Equal("i/{have}/{id}/{defaultsto}/{name}", route.Url);
            Assert.IsAssignableFrom<IntRouteConstraint>(route.Constraints["id"]);
            Assert.Equal("VAL", route.Defaults["defaultsto"]);
            Assert.Equal("", route.Defaults["name"].ToString());
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithPrefixedController()
        {
            // Arrange
            var controllerTypes = new[] { typeof(PrefixedController) };
            var routes = new RouteCollection();

            // Act
            routes.MapMvcAttributeRoutes(controllerTypes);

            // Assert
            Assert.Equal(1, routes.Count);

            Route route = (Route)routes.Single();
            Assert.Equal("prefpref/getme", route.Url);
            Assert.Equal("GetMe", route.GetTargetActionMethod().Name);
        }

        [Fact]
        public void RoutePrefixAttribute_IsSingleInstance()
        {
            var attr = typeof(RoutePrefixAttribute);
            var attrs = attr.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var usage = (AttributeUsageAttribute)attrs[0];

            Assert.Equal(AttributeTargets.Class, usage.ValidOn);
            Assert.False(usage.AllowMultiple); // only 1 per class
            Assert.False(usage.Inherited); // RoutePrefix is not inherited. 
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithArea()
        {
            // Arrange
            var controllerTypes = new[] { typeof(PugetSoundController) };
            var routes = new RouteCollection();

            // Act
            routes.MapMvcAttributeRoutes(controllerTypes);


            // Assert
            Assert.Equal(1, routes.Count);

            Route route = (Route)routes.Single();

            Assert.Equal("puget-sound/getme", route.Url);
            Assert.Equal("PugetSound", route.DataTokens["area"]);
            Assert.Equal(false, route.DataTokens["usenamespacefallback"]);
            Assert.Equal("GetMe", route.GetTargetActionMethod().Name);
            Assert.Equal(typeof(PugetSoundController).Namespace, ((string[])route.DataTokens["namespaces"])[0]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithPrefixedArea()
        {
            // Arrange
            var controllerTypes = new[] { typeof(PrefixedPugetSoundController) };
            var routes = new RouteCollection();

            // Act
            routes.MapMvcAttributeRoutes(controllerTypes);


            // Assert
            Assert.Equal(1, routes.Count);

            Route route = (Route)routes.Single();

            Assert.Equal("puget-sound/prefpref/getme", route.Url);
            Assert.Equal("PugetSound", route.DataTokens["area"]);
            Assert.Equal(false, route.DataTokens["usenamespacefallback"]);
            Assert.Equal("GetMe", route.GetTargetActionMethod().Name);
            Assert.Equal(typeof(PrefixedPugetSoundController).Namespace, ((string[])route.DataTokens["namespaces"])[0]);
        }

        [Theory]
        [InlineData(null, null, "", "")]
        [InlineData(null, null, "whatever", "whatever")]
        [InlineData(null, "", "", "")]
        [InlineData(null, "", "whatever", "whatever")]
        [InlineData(null, "pref", "", "pref")]
        [InlineData(null, "pref", "whatever", "pref/whatever")]
        [InlineData("", null, "", "")]
        [InlineData("", null, "whatever", "whatever")]
        [InlineData("", "", "", "")]
        [InlineData("", "", "whatever", "whatever")]
        [InlineData("", "pref", "", "pref")]
        [InlineData("", "pref", "whatever", "pref/whatever")]
        [InlineData("puget-sound", null, "", "puget-sound")]
        [InlineData("puget-sound", null, "whatever", "puget-sound/whatever")]
        [InlineData("puget-sound", "", "", "puget-sound")]
        [InlineData("puget-sound", "", "whatever", "puget-sound/whatever")]
        [InlineData("puget-sound", "pref", "", "puget-sound/pref")]
        [InlineData("puget-sound", "pref", "whatever", "puget-sound/pref/whatever")]
        [InlineData(null, null, "~/whatever", "whatever")]
        [InlineData("puget-sound", "pref", "~/", "")]
        [InlineData("puget-sound", "pref", "~/whatever", "whatever")]
        [InlineData("puget-sound", null, "~/whatever", "whatever")]
        [InlineData(null, "pref", "~/whatever", "whatever")]
        public void CombinePrefixAndAreaWithTemplate(string areaPrefix, string prefix, string template, string expected)
        {
            var result = AttributeRoutingMapper.CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, template);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(typeof(Bad1Controller), "The route prefix '/pref' on the controller named 'Bad1' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad2Controller), "The route prefix 'pref/' on the controller named 'Bad2' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad3Controller), "The route template '/getme' on the action named 'GetMe' on the controller named 'Bad3' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad4Controller), "The route template 'getme/' on the action named 'GetMe' on the controller named 'Bad4' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad5Controller), "The prefix '/puget-sound' of the route area named 'PugetSound' on the controller named 'Bad5' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad6Controller), "The prefix 'puget-sound/' of the route area named 'PugetSound' on the controller named 'Bad6' cannot begin or end with a forward slash.")]
        public void TemplatesAreValidated(Type controllerType, string expectedErrorMessage)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => routes.MapMvcAttributeRoutes(controllerTypes), expectedErrorMessage);
        }

        private class SimpleRoutingController : Controller
        {
            [HttpGet("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            [HttpPost("postme")]
            public ActionResult PostMe()
            {
                throw new NotImplementedException();
            }

            [HttpRoute("routeme")]
            public ActionResult RouteMe()
            {
                throw new NotImplementedException();
            }

            [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post, RouteTemplate = "getorpostme")]
            public ActionResult GetOrPostMe()
            {
                throw new NotImplementedException();
            }

            [HttpGet("once")]
            [HttpGet("twice")]
            public ActionResult FoolMe()
            {
                throw new NotImplementedException("Shame on you!");
            }

            [HttpGet("i/{have}/{id:int}/{defaultsto=VAL}/{name?}")]
            public ActionResult Parameterized(string have, int id, string optional)
            {
                throw new NotImplementedException();
            }
        }

        [RoutePrefix("prefpref")]
        private class PrefixedController : Controller
        {
            [HttpGet("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "puget-sound")]
        private class PugetSoundController : Controller
        {
            [HttpGet("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "puget-sound")]
        [RoutePrefix("prefpref")]
        private class PrefixedPugetSoundController : Controller
        {
            [HttpGet("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }

        [RoutePrefix("/pref")]
        private class Bad1Controller : Controller
        {
            [HttpGet("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        [RoutePrefix("pref/")]
        private class Bad2Controller : Controller
        {
            [HttpGet("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        private class Bad3Controller : Controller
        {
            [HttpGet("/getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        private class Bad4Controller : Controller
        {
            [HttpGet("getme/")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "/puget-sound")]
        private class Bad5Controller : Controller
        {
        }

        [RouteArea("PugetSound", AreaPrefix = "puget-sound/")]
        private class Bad6Controller : Controller
        {
        }

        private class FruitConstraintController : Controller
        {
            [HttpGet("fruits/{apple:fruit}")]
            public ActionResult Eat(string apple)
            {
                throw new NotImplementedException();
            }
        }

        class FruitConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
                              RouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }

        class FruitConstraintResolver : IInlineConstraintResolver
        {
            public IRouteConstraint ResolveConstraint(string inlineConstraint)
            {
                if (inlineConstraint == "fruit")
                {
                    return new FruitConstraint();
                }

                throw new InvalidOperationException();
            }
        }
    }
}
