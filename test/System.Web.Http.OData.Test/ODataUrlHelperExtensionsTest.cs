// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class ODataUrlHelperExtensionsTest
    {
        [Fact]
        public void GenerateLinkDirectly_ReturnsNull_IfHelperRequestHasNoConfiguration()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/vpath/prefix/Customers");
            UrlHelper urlHelper = new UrlHelper(request);

            Assert.Null(urlHelper.GenerateLinkDirectly("odataPath"));
        }

        [Fact]
        public void GenerateLinkDirectly_ReturnsNull_IfNoRouteCalledODataFound()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/vpath/prefix/Customers");
            HttpConfiguration config = new HttpConfiguration(new HttpRouteCollection("http://localhost/vpath"));
            config.Routes.MapHttpRoute("NotOData", "{controller}");
            request.Properties["MS_HttpConfiguration"] = config;
            UrlHelper urlHelper = new UrlHelper(request);

            Assert.Null(urlHelper.GenerateLinkDirectly("odataPath"));
        }

        [Fact]
        public void GenerateLinkDirectly_ReturnsNull_IfRouteTemplateDoesNotEndInODataPath()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/vpath/prefix/Customers");
            HttpConfiguration config = new HttpConfiguration(new HttpRouteCollection("http://localhost/vpath"));
            config.Routes.MapHttpRoute("OData", "prefix/{*notODataPath}");
            request.Properties["MS_HttpConfiguration"] = config;
            UrlHelper urlHelper = new UrlHelper(request);

            Assert.Null(urlHelper.GenerateLinkDirectly("odataPath"));
        }

        [Fact]
        public void GenerateLinkDirectly_ReturnsNull_IfRouteTemplateHasParameterInPrefix()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/vpath/prefix/Customers");
            HttpConfiguration config = new HttpConfiguration(new HttpRouteCollection("http://localhost/vpath"));
            config.Routes.MapHttpRoute("OData", "{prefix}/{*odataPath}");
            request.Properties["MS_HttpConfiguration"] = config;
            UrlHelper urlHelper = new UrlHelper(request);

            Assert.Null(urlHelper.GenerateLinkDirectly("odataPath"));
        }

        [Fact]
        public void GenerateLinkDirectly_ReturnsUri_IfConditionsSatisfied()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/vpath/prefix/Customers");
            HttpConfiguration config = new HttpConfiguration(new HttpRouteCollection("http://localhost/vpath"));
            config.Routes.MapHttpRoute("OData", "prefix/{*odataPath}");
            request.Properties["MS_HttpConfiguration"] = config;
            request.Properties["MS_HttpRouteData"] = new HttpRouteData(new HttpRoute());
            UrlHelper urlHelper = new UrlHelper(request);

            Assert.Equal("http://localhost/vpath/prefix/odataPath", urlHelper.GenerateLinkDirectly("odataPath"));
        }
    }
}
