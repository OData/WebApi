// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Mvc.Routing;
using System.Web.Mvc.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Routing
{
    public class AttributeRoutingMapperTests
    {
        [Fact]
        public void MapAttributeRoutesFromController_MapRouteAttributes()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(SimpleRoutingController));

            // Act
            var routes = GetMapper().MapMvcAttributeRoutes(controllerDescriptor)
                .Select(e => e.Route)
                .ToArray();

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
                Route route = routes.Single(r => r.Url == url);
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
        public void MapAttributeRoutesFromController_WithInlineConstraints_ParseConstraintsDefaultsAndOptionals()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(SimpleRoutingController));

            // Act
            var routes = GetMapper().MapMvcAttributeRoutes(controllerDescriptor)
                .Select(e => e.Route)
                .ToArray();

            // Assert
            Route route = routes.Single(r => r.GetTargetActionMethod().Name == "Parameterized");
            Assert.NotNull(route);

            Assert.Equal("i/{have}/{id}/{defaultsto}/{name}", route.Url);
            Assert.IsAssignableFrom<IntRouteConstraint>(route.Constraints["id"]);
            Assert.Equal("VAL", route.Defaults["defaultsto"]);
            Assert.Equal("", route.Defaults["name"].ToString());
        }

        [Fact]
        public void MapAttributeRoutesFromController_WithPrefixedController()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(PrefixedController));

            // Act
            var routes = GetMapper().MapMvcAttributeRoutes(controllerDescriptor)
                .Select(e => e.Route)
                .ToArray();

            // Assert
            Assert.Equal(1, routes.Length);

            Route route = routes.Single();
            Assert.Equal("prefpref/getme", route.Url);
            Assert.Equal("GetMe", route.GetTargetActionMethod().Name);
        }

        [Fact]
        public void MapAttributeRoutesFromController_WithMultiPrefixedController()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MultiPrefixedController));

            // Act
            var routes = GetMapper().MapMvcAttributeRoutes(controllerDescriptor)
                .Select(e => e.Route)
                .ToArray();

            // Assert
            Assert.Equal(4, routes.Length);

            var actualRouteUrls = routes.Select(route => route.Url).OrderBy(url => url).ToArray();
            var expectedRouteUrls = new[]
                {
                    "pref1/getme",
                    "pref1/getmeaswell",
                    "pref2/getme",
                    "pref2/getmeaswell",
                };

            Assert.Equal(expectedRouteUrls, actualRouteUrls);
        }
        
        [Fact]
        public void MapAttributeRoutesFromController_WithArea()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(PugetSoundController));

            // Act
            var routes = GetMapper().MapMvcAttributeRoutes(controllerDescriptor)
                .Select(e => e.Route)
                .ToArray();

            // Assert
            Assert.Equal(1, routes.Length);

            Route route = routes.Single();

            Assert.Equal("puget-sound/getme", route.Url);
            Assert.Equal("PugetSound", route.DataTokens["area"]);
            Assert.Equal(false, route.DataTokens["usenamespacefallback"]);
            Assert.Equal("GetMe", route.GetTargetActionMethod().Name);
            Assert.Equal(typeof(PugetSoundController).Namespace, ((string[])route.DataTokens["namespaces"])[0]);
        }

        [Fact]
        public void MapAttributeRoutesFromController_WithPrefixedArea()
        {
            // Arrange
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(PrefixedPugetSoundController));

            // Act
            var routes = GetMapper().MapMvcAttributeRoutes(controllerDescriptor)
                .Select(e => e.Route)
                .ToArray();

            // Assert
            Assert.Equal(1, routes.Length);

            Route route = routes.Single();

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
        public void CombinePrefixAndAreaWithTemplate(string areaPrefix, string prefix, string template, string expected)
        {
            var result = AttributeRoutingMapper.CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, template);

            Assert.Equal(expected, result);
        }

        private static AttributeRoutingMapper GetMapper()
        {
            return new AttributeRoutingMapper(new RouteBuilder());
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

        [RoutePrefix("pref1")]
        [RoutePrefix("pref2")]
        private class MultiPrefixedController : Controller
        {
            [HttpGet("getme")]
            [HttpGet("getmeaswell")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }
    }
}
