// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProviderTest
    {
        [Fact]
        public void GetValue_RouteDataContainsNonStringValue()
        {
            // Arrange
            var data = new TestClass();
            HttpRouteData routeData = new HttpRouteData(new HttpRoute());
            routeData.Values["item"] = data;
            HttpActionContext context = new HttpActionContext();
            context.ControllerContext = new HttpControllerContext { RouteData = routeData };
            RouteDataValueProvider provider = new RouteDataValueProvider(context, CultureInfo.InvariantCulture);

            // Act
            ValueProviderResult result = provider.GetValue("item");

            //Assert
            Assert.Same(data, result.RawValue);
        }

        [Fact]
        public void CanModelBindNonStringData()
        {
            // Arrange
            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapHttpRoute("default", "{controller}", new { test = new TestClass { Id = 42 } });
            HttpClient client = new HttpClient(server);

            // Act
            var response = client.GetAsync("http://localhost/Test").Result;

            // Assert
            var testClass = response.Content.ReadAsAsync<TestClass>().Result;
            Assert.Equal(42, testClass.Id);
        }

        public class TestClass
        {
            public int Id { get; set; }
        }

        public class TestController : ApiController
        {
            public TestClass Get([FromUri]TestClass test)
            {
                return test;
            }
        }
    }
}
