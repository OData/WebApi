// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Routing
{
    public class UrlHelperTest
    {
        [Fact]
        public void UrlHelper_CtorThrows_WithNullContext()
        {
            Assert.ThrowsArgumentNull(
                () => new UrlHelper(null),
                "controllerContext");
        }

        [Fact]
        public void ControllerContext_HasUrlHelperWithValidContext()
        {
            HttpControllerContext cc = new HttpControllerContext();

            Assert.NotNull(cc.Url);
            Assert.IsType<UrlHelper>(cc.Url);
            Assert.Same(cc, cc.Url.ControllerContext);
        }

        [Theory]
        [PropertyData("UrlGeneratorTestData")]
        public void UrlHelper_UsesCurrentRouteDataToPopulateValues_WithObjectValues(string controller, int? id, string expectedUrl)
        {
            var url = GetUrlHelperForApi();
            object routeValues = GetRouteValuesAsObject(controller, id);

            string generatedUrl = url.Route("route1", routeValues);

            Assert.Equal(expectedUrl, generatedUrl);
        }

        [Theory]
        [PropertyData("UrlGeneratorTestData")]
        public void UrlHelper_UsesCurrentRouteDataToPopulateValues_WithDictionaryValues(string controller, int? id, string expectedUrl)
        {
            var url = GetUrlHelperForApi();
            Dictionary<string, object> routeValues = GetRouteValuesAsDictionary(controller, id);

            string generatedUrl = url.Route("route1", routeValues);

            Assert.Equal(expectedUrl, generatedUrl);
        }

        [Fact]
        public void UrlHelper_Throws_WhenWrongNameUsed_WithObjectValues()
        {
            var url = GetUrlHelperForApi();
            Assert.ThrowsArgument(
                () => url.Route("route-doesn't-exist", null),
                "name",
                "A route named 'route-doesn't-exist' could not be found in the route collection.");
        }

        [Fact]
        public void UrlHelper_Throws_WhenWrongNameUsed_WithDictionaryValues()
        {
            var url = GetUrlHelperForApi();
            Assert.ThrowsArgument(
                () => url.Route("route-doesn't-exist", (IDictionary<string, object>)null),
                "name",
                "A route named 'route-doesn't-exist' could not be found in the route collection.");
        }

        [Theory]
        [TestDataSet(
            typeof(UrlHelperTest), "UrlGeneratorTestData",
            typeof(UrlHelperTest), "RequestUrlTestData")]
        public void UrlHelper_LinkGeneration_GeneratesRightLinksWithDictionary(string controller, int? id, string expectedUrl, string requestUrl)
        {
            var urlHelper = GetUrlHelperForApi();
            urlHelper.ControllerContext.Request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            Dictionary<string, object> routeValues = GetRouteValuesAsDictionary(controller, id);
            string baseUrl = new Uri(requestUrl).GetLeftPart(UriPartial.Authority);

            string generatedlink = urlHelper.Link("route1", routeValues);

            Assert.Equal(expectedUrl != null ? baseUrl + expectedUrl : null, generatedlink);
        }

        [Theory]
        [TestDataSet(
            typeof(UrlHelperTest), "UrlGeneratorTestData",
            typeof(UrlHelperTest), "RequestUrlTestData")]
        public void UrlHelper_LinkGeneration_GeneratesRightLinksWithObject(string controller, int? id, string expectedUrl, string requestUrl)
        {
            var urlHelper = GetUrlHelperForApi();
            urlHelper.ControllerContext.Request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            object routeValues = GetRouteValuesAsObject(controller, id);
            string baseUrl = new Uri(requestUrl).GetLeftPart(UriPartial.Authority);

            string generatedlink = urlHelper.Link("route1", routeValues);

            Assert.Equal(expectedUrl != null ? baseUrl + expectedUrl : null, generatedlink);
        }

        private static UrlHelper GetUrlHelperForApi()
        {
            HttpControllerContext cc = new HttpControllerContext();

            // Set up routes
            var routes = new HttpRouteCollection("/somerootpath");
            IHttpRoute route = routes.MapHttpRoute("route1", "{controller}/{id}");
            cc.Configuration = new HttpConfiguration(routes);

            cc.RouteData = new HttpRouteData(route, new HttpRouteValueDictionary(new { controller = "people", id = "123" }));

            return cc.Url;
        }

        private static object GetRouteValuesAsObject(string controller, int? id)
        {
            object routeValues = null;
            if (controller == null)
            {
                if (id == null)
                {
                    routeValues = null;
                }
                else
                {
                    routeValues = new { id };
                }
            }
            else
            {
                if (id == null)
                {
                    routeValues = new { controller };
                }
                else
                {
                    routeValues = new { controller, id };
                }
            }

            return routeValues;
        }

        private static Dictionary<string, object> GetRouteValuesAsDictionary(string controller, int? id)
        {
            Dictionary<string, object> routeValues = new Dictionary<string, object>();
            if (controller == null)
            {
                if (id == null)
                {
                    routeValues = null;
                }
                else
                {
                    routeValues.Add("id", id);
                }
            }
            else
            {
                if (id == null)
                {
                    routeValues.Add("controller", controller);
                }
                else
                {
                    routeValues.Add("controller", controller);
                    routeValues.Add("id", id);
                }
            }

            return routeValues;
        }

        public static IEnumerable<object[]> UrlGeneratorTestData
        {
            get
            {
                return new TheoryDataSet<string, int?, string>()
                {
                    { null, 456, "/somerootpath/people/456"}, // Just override ID, so ID is replaced
                    { "people", 456, "/somerootpath/people/456"}, // Just override ID, so ID is replaced
                    { null, null, "/somerootpath/people/123"}, // Override nothing, so everything the same
                    { "people", null, "/somerootpath/people/123"}, // Override nothing, so everything the same
                    { "customers", 456, "/somerootpath/customers/456"}, // Override everything, so everything changed
                    { "customers", null, null}, // Override controller, which clears out the ID, so it doesn't match (i.e. null)
                };
            }
        }

        public static IEnumerable<object> RequestUrlTestData
        {
            get
            {
                return new[] { "http://localhost", "http://localhost/123", "http://localhost/123?q=odata&$filter=123#123" };
            }
        }
    }
}
