// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Hosting;
using Microsoft.Owin.Hosting;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Owin;

namespace System.Web.Http.Owin
{
    public class OwinHostIntegrationTest
    {
        [Fact]
        public void SimpleGet_Works()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostTwoComponents>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync(CreateUrl(port, "HelloWorld")).Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Hello from OWIN\"", response.Content.ReadAsStringAsync().Result);
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void SimplePost_Works()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostTwoComponents>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();
                var content = new StringContent("\"Echo this\"", Encoding.UTF8, "application/json");

                var response = client.PostAsync(CreateUrl(port, "Echo"), content).Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Echo this\"", response.Content.ReadAsStringAsync().Result);
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void GetThatThrowsDuringSerializations_RespondsWith500()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostTwoComponents>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync(CreateUrl(port, "Error")).Result;

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                JObject json = Assert.IsType<JObject>(JToken.Parse(response.Content.ReadAsStringAsync().Result));
                JToken exceptionMessage;
                Assert.True(json.TryGetValue("ExceptionMessage", out exceptionMessage));
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void IgnoreRoute_Owin_SingleComponent_WithoutIgnoreRoute_ReturnSuccess()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostSingleComponent_NotIgnoreRoute>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var notIgnoredResponse = client.GetAsync(CreateUrl(port, "api/HelloWorld")).Result;

                Assert.True(notIgnoredResponse.IsSuccessStatusCode);
                Assert.Equal("\"Hello from OWIN\"", notIgnoredResponse.Content.ReadAsStringAsync().Result);
            }
        }

        [Fact]
        public void IgnoreRoute_Owin_SingleComponent_WithIgnoreRoute_ReturnHard404()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostSingleComponent_IgnoreRoute>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var ignoredResponse = client.GetAsync(CreateUrl(port, "api/HelloWorld")).Result;

                Assert.Equal(ignoredResponse.StatusCode, HttpStatusCode.NotFound);
                Assert.False(ignoredResponse.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
            }
        }

        [Fact]
        public void IgnoreRoute_Owin_TwoComponents_OneWebAPIwithIgnoreRouteDoesNotAffectAnotherWebAPI()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostTwoComponents>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync(CreateUrl(port, "api/HelloWorld")).Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Hello from OWIN\"", response.Content.ReadAsStringAsync().Result);
            }
        }

        [Theory]
        [InlineData("ignoredByBothComponents/")]
        [InlineData("ignoredByBothComponents/people/1")]
        [InlineData("ignoredByBothComponents/people/literal")]
        [InlineData("ignoredByBothComponents/people/name?id=20")]
        public void IgnoreRoute_Owin_TwoComponents_GetHard404IfRoutesAreIgnoredByBothComponents(string path)
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostTwoComponents>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync(CreateUrl(port, path)).Result;

                Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
                Assert.False(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
            }
        }

        private static string CreateBaseUrl(PortReserver port)
        {
            return port.BaseUri + "vroot";
        }

        private static string CreateUrl(PortReserver port, string localPath)
        {
            return CreateBaseUrl(port) + "/" + localPath;
        }
    }

    public class OwinHostTwoComponents
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // The Owin host contains 2 Web API components.
            // The first Web API ignores certain routes.
            var config1 = new HttpConfiguration();
            config1.Routes.IgnoreRoute("foo", "api/{controller}");
            // The following route is ignored by both components.
            config1.Routes.IgnoreRoute("bar", "ignoredByBothComponents/{*pathInfo}");

            appBuilder.UseWebApi(config1);

            // The second Web API
            var config2 = new HttpConfiguration();
            // The following route is ignored by both components.
            config2.Routes.IgnoreRoute("bar", "ignoredByBothComponents/{*pathInfo}");
            config2.Routes.MapHttpRoute("Default", "{controller}");
            // It can handle the route ignored by the previous Web API.
            config2.Routes.MapHttpRoute("DefaultApi", "api/{controller}");

            appBuilder.UseWebApi(config2);
        }
    }

    public class OwinHostSingleComponent_NotIgnoreRoute
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("foo", "api/{controller}");

            appBuilder.UseWebApi(config);
        }
    }

    public class OwinHostSingleComponent_IgnoreRoute
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Routes.IgnoreRoute("bar", "api/{controller}");
            config.Routes.MapHttpRoute("foo", "api/{controller}");

            appBuilder.UseWebApi(config);
        }
    }

    public class HelloWorldController : ApiController
    {
        public string Get()
        {
            return "Hello from OWIN";
        }
    }

    public class EchoController : ApiController
    {
        public string Post([FromBody] string s)
        {
            return s;
        }
    }

    public class ErrorController : ApiController
    {
        public ExceptionThrower Get()
        {
            return new ExceptionThrower();
        }

        public class ExceptionThrower
        {
            public string Throws
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
