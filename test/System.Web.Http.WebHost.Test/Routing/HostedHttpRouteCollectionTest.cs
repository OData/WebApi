// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Web.Hosting;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.WebHost.Routing
{
    public class HostedHttpRouteCollectionTest
    {
        RouteCollection _aspNetRoutes;
        HostedHttpRouteCollection _webApiRoutes;

        public HostedHttpRouteCollectionTest()
        {
            _aspNetRoutes = new RouteCollection();
            _webApiRoutes = new HostedHttpRouteCollection(_aspNetRoutes);
        }

        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => new HostedHttpRouteCollection(routeCollection: null), "routeCollection");
        }

        [Fact]
        public void Add_WithHostedHttpRoute_RegistersInnerRouteWithAspNetRouteCollection()
        {
            var route = new HostedHttpRoute("uri", null, null, null, null);

            _webApiRoutes.Add("name", route);

            var httpWebRoute = Assert.IsType<HttpWebRoute>(_aspNetRoutes["name"]);
            Assert.Same(route.OriginalRoute, httpWebRoute);
            Assert.Same(route, httpWebRoute.HttpRoute);
        }

        [Fact]
        public void Add_WithNonHostedHttpRoute_WrapsCustomRouteWithHttpWebRoute()
        {
            var route = new Mock<IHttpRoute>().Object;

            _webApiRoutes.Add("name", route);

            var httpWebRoute = Assert.IsType<HttpWebRoute>(_aspNetRoutes["name"]);
            Assert.Same(route, httpWebRoute.HttpRoute);
        }

        [Fact]
        public void Clear_RemovesAllValuesFromAspNetRouteCollection()
        {
            _aspNetRoutes.Add(new Mock<RouteBase>().Object);

            _webApiRoutes.Clear();

            Assert.Empty(_aspNetRoutes);
        }

        [Fact]
        public void Contains_OnlyMatchesRegisteredHttpRouteInstances()
        {
            var route = new Mock<IHttpRoute>().Object;
            _webApiRoutes.Add("bar", route);

            Assert.True(_webApiRoutes.Contains(route));
            Assert.False(_webApiRoutes.Contains(new Mock<IHttpRoute>().Object));
        }

        [Fact]
        public void ContainsKey_MatchesAllRoutesInAspNetRouteCollection()
        {
            _aspNetRoutes.Add("foo", new Mock<RouteBase>().Object);
            _webApiRoutes.Add("bar", new Mock<IHttpRoute>().Object);

            Assert.True(_webApiRoutes.ContainsKey("foo"));
            Assert.True(_webApiRoutes.ContainsKey("bar"));
        }

        [Fact]
        public void Count_ReturnsCountOfAllRoutesInAspNetRouteCollection()
        {
            _aspNetRoutes.Add("foo", new Mock<RouteBase>().Object);
            _webApiRoutes.Add("bar", new Mock<IHttpRoute>().Object);

            Assert.Equal(2, _webApiRoutes.Count);
        }

        [Fact]
        public void CreateRoute_CreatesHostedHttpRoute()
        {
            var defaults = new HttpRouteValueDictionary { { "Foo", "Bar" } };
            var constraints = new HttpRouteValueDictionary { { "Bar", "Baz" } };
            var dataTokens = new HttpRouteValueDictionary { { "Baz", "Biff" } };
            var handler = new Mock<HttpMessageHandler>().Object;

            IHttpRoute result = _webApiRoutes.CreateRoute("uri", defaults, constraints, dataTokens, handler);

            Assert.IsType<HostedHttpRoute>(result);
            Assert.Equal("Bar", result.Defaults["Foo"]);
            Assert.Equal("Baz", result.Constraints["Bar"]);
            Assert.Equal("Biff", result.DataTokens["Baz"]);
            Assert.Same(handler, result.Handler);
        }

        [Fact]
        public void Enumerating_OnlyIncludesHttpRoutes()
        {
            var aspNetRoute = new Mock<RouteBase>().Object;
            _aspNetRoutes.Add("foo", aspNetRoute);
            var httpRoute = new Mock<IHttpRoute>().Object;
            _webApiRoutes.Add("bar", httpRoute);

            List<object> objects = new List<object>(_webApiRoutes);

            Assert.Contains(httpRoute, objects);
            Assert.DoesNotContain(aspNetRoute, objects);
        }

        [Fact]
        public void GetRouteData_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => _webApiRoutes.GetRouteData(request: null), "request");
        }

        [Fact]
        public void GetRouteData_WithHttpContext_UnmatchedRoute_ReturnsNull()
        {
            var request = new HttpRequestMessage();
            request.SetHttpContext(CreateHttpContext("~/api2"));
            IHttpRoute route = _webApiRoutes.CreateRoute("api", null, null);
            _webApiRoutes.Add("default", route);

            IHttpRouteData result = _webApiRoutes.GetRouteData(request);

            Assert.Null(result);
        }

        [Fact]
        public void GetRouteData_WithHttpContext_MatchedRoute_ReturnsRouteData()
        {
            var request = new HttpRequestMessage();
            request.SetHttpContext(CreateHttpContext("~/api"));
            IHttpRoute route = _webApiRoutes.CreateRoute("api", null, null);
            _webApiRoutes.Add("default", route);

            IHttpRouteData result = _webApiRoutes.GetRouteData(request);

            Assert.Same(route, result.Route);
        }

        [Fact]
        public void SendAsync_CallsDefaultHandlerWhenCustomASPNETRoute()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/controllerName");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpDomainRoute domainRoute = new HttpDomainRoute("test", new { controller = "Values", action = "GetTenant" });
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HostedHttpRouteData(domainRoute.GetRouteData(null));
            var dispatcher = new HttpRoutingDispatcher(config, defaultHandler: mockHandler.Object);
            var invoker = new HttpMessageInvoker(dispatcher);

            // Act
            invoker.SendAsync(request, CancellationToken.None);

            // Assert
            mockHandler.Protected().Verify("SendAsync", Times.Once(), request, CancellationToken.None);
        }

        [Fact]
        public void GetVirtualPath_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => _webApiRoutes.GetVirtualPath(request: null, name: null, values: null), "request");
        }

        [Fact]
        public void GetVirtualPath_ReturnsVirtualPathData()
        {
            var request = new HttpRequestMessage();
            request.SetHttpContext(CreateHttpContext("~/api", "APP PATH MODIFIER RETURN VALUE"));
            var config = new HttpConfiguration(_webApiRoutes);
            IHttpRoute route = _webApiRoutes.CreateRoute("api", null, null);
            _webApiRoutes.Add("default", route);
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = _webApiRoutes.GetRouteData(request);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            IHttpVirtualPathData result = _webApiRoutes.GetVirtualPath(request, null, new HttpRouteValueDictionary { { "httproute", true } });

            Assert.NotNull(result);
            Assert.Same(route, result.Route);
            Assert.Equal("APP PATH MODIFIER RETURN VALUE", result.VirtualPath);
        }

        [Fact]
        public void Indexer_ForHttpRoute_ReturnsRoute()
        {
            var route = new Mock<IHttpRoute>().Object;
            _webApiRoutes.Add("foo", route);

            var result = _webApiRoutes["foo"];

            Assert.NotNull(result);
            Assert.Same(route, result);
        }

        [Fact]
        public void Indexer_ForAspNetRoute_Throws()
        {
            _aspNetRoutes.Add("foo", new Mock<RouteBase>().Object);

            Assert.Throws<KeyNotFoundException>(() => _webApiRoutes["foo"], "The given key was not present in the dictionary.");
        }

        [Fact]
        public void Indexer_ForUnknownRoute_Throws()
        {
            Assert.Throws<KeyNotFoundException>(() => _webApiRoutes["foo"], "The given key was not present in the dictionary.");
        }

        [Fact]
        public void UnsupportedFunctions()
        {
            Assert.Throws<NotSupportedException>(() => _webApiRoutes.CopyTo((IHttpRoute[])null, 0), "This operation is only supported by directly calling it on 'RouteCollection'.");

            Assert.Throws<NotSupportedException>(() => _webApiRoutes.CopyTo((KeyValuePair<string, IHttpRoute>[])null, 0), "This operation is not supported by 'HostedHttpRouteCollection'.");
            Assert.Throws<NotSupportedException>(() => _webApiRoutes.Insert(0, null, null), "This operation is not supported by 'HostedHttpRouteCollection'.");
            Assert.Throws<NotSupportedException>(() => _webApiRoutes.Remove(null), "This operation is not supported by 'HostedHttpRouteCollection'.");
        }

        [Fact]
        public void ConvertHttpRouteDataToRouteDataRunsCustomHttpRoute()
        {
            // Arrange
            DomainHttpRoute route = new DomainHttpRoute("myDomain", "api/{controller}/{action}", new { controller = "Values", action = "GetTenant" });
            HostedHttpRouteCollection collection = new HostedHttpRouteCollection(new RouteCollection());
            collection.Add("domainRoute", route);
            HttpRequestMessage request = CreateHttpRequestMessageWithContext();
            IHttpRouteData httpRouteData = collection.GetRouteData(request);
            
            // Act
            RouteData routeData = httpRouteData.ToRouteData();

            // Assert
            Assert.NotNull(routeData.Values);
            Assert.Equal(3, routeData.Values.Count);
            Assert.Equal("controllerName", routeData.Values["controller"]);
            Assert.Equal("actionName", routeData.Values["action"]);
            Assert.Equal("myDomain", routeData.Values["domain"]);
        }

        [Fact]
        public void CustomHttpRouteGetVitualPathRunsCustomHttpRoute()
        {
            // Arrange
            DomainHttpRoute route = new DomainHttpRoute("myDomain", "api/{controller}/{action}", new { controller = "SomeValue", action = "SomeAction" });
            HostedHttpRouteCollection collection = new HostedHttpRouteCollection(new RouteCollection());
            collection.Add("domainRoute", route);
            HttpRequestMessage request = CreateHttpRequestMessageWithContext();
            HttpRouteValueDictionary routeValues = new HttpRouteValueDictionary()
                {
                    {"controller", "controllerName"},
                    {"action", "actionName"},
                    {"httproute", true}
                };

            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(route, routeValues);
            
            // Act
            IHttpVirtualPathData httpvPathData = collection.GetVirtualPath(request, "domainRoute", routeValues);

            // Assert
            Assert.NotNull(httpvPathData);
            Assert.Equal("/api/controllerName/actionNameFromDomain", httpvPathData.VirtualPath);
        }

        private static HttpRequestMessage CreateHttpRequestMessageWithContext()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/controllerName/actionName");
            request.SetConfiguration(new HttpConfiguration());
            request.SetHttpContext(CreateHttpContext("~/api"));

            return request;
        }

        private static HttpContextBase CreateHttpContext(string relativeUrl, string appPathModifierReturnValue = "")
        {
            var mockContext = new Mock<HttpContextBase>();

            mockContext.SetupGet(c => c.Request.ApplicationPath).Returns(String.Empty);
            mockContext.SetupGet(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns(relativeUrl);
            mockContext.SetupGet(c => c.Request.PathInfo).Returns("");
            mockContext.SetupGet(c => c.Items).Returns(new Dictionary<string, object>());
            mockContext.SetupGet(c => c.Request.HttpMethod).Returns("GET");
            mockContext.SetupGet(c => c.Request.InputStream).Returns(new MemoryStream());
            mockContext.SetupGet(c => c.Request.Headers).Returns(new NameValueCollection());
            mockContext.SetupGet(c => c.Request.ApplicationPath).Returns("/");

            if (appPathModifierReturnValue == string.Empty)
            {
                mockContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                               .Returns((string s) => { return s; });
            }
            else
            {
                mockContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                             .Returns(appPathModifierReturnValue);
            }
            
            return mockContext.Object;
        }

        public class HttpDomainRoute : Route
        {
            public HttpDomainRoute(string routeTemplate, object defaults, object constraints = null)
                : base(routeTemplate, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), new RouteValueDictionary(), HttpControllerRouteHandler.Instance)
            {
            }

            public override RouteData GetRouteData(HttpContextBase context)
            {
                RouteData data = new RouteData(this, RouteHandler);
                data.Values.Add("domain", "customer");
                return data;
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
            {
                return base.GetVirtualPath(requestContext, values);
            }
        }

        public class DomainHttpRoute : HttpRoute
        {
            public DomainHttpRoute(string domain, string routeTemplate, object defaults, object constraints = null)
                : base(routeTemplate, new HttpRouteValueDictionary(defaults), new HttpRouteValueDictionary(constraints))
            {
                Domain = domain;
            }

            public string Domain { get; set; }

            public override IHttpRouteData GetRouteData(string virtualPathRoot, System.Net.Http.HttpRequestMessage request)
            {
                // Route data
                IHttpRouteData data = base.GetRouteData(virtualPathRoot, request);
                data.Values.Add("domain", Domain);
                return data;
            }

            public override IHttpVirtualPathData GetVirtualPath(System.Net.Http.HttpRequestMessage request, IDictionary<string, object> values)
            {
                // customize the action token
                values["action"] = "actionNameFromDomain";

                return base.GetVirtualPath(request, values);
            }
        }

    }
}
