// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.SelfHost;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.ContentNegotiation
{
    public class HttpResponseReturnTests : IDisposable
    {
        private HttpSelfHostServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        public HttpResponseReturnTests()
        {
            this.SetupHost();
        }

        public void Dispose()
        {
            this.CleanupHost();
        }

        [Theory]
        [InlineData("ReturnHttpResponseMessage")]
        [InlineData("ReturnHttpResponseMessageAsObject")]
        [InlineData("ReturnString")]
        public void ActionReturnsHttpResponseMessage(string action)
        {
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">Hello</string>";

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseAddress + String.Format("HttpResponseReturn/{0}", action));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Get;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal<string>(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("ReturnHttpResponseMessageAsXml")]
        public void ActionReturnsHttpResponseMessageWithExplicitMediaType(string action)
        {
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">Hello</string>";

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseAddress + String.Format("HttpResponseReturn/{0}", action));
            request.Method = HttpMethod.Get;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal<string>(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("ReturnMultipleSetCookieHeaders")]
        public void ReturnMultipleSetCookieHeadersShouldWork(string action)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseAddress + String.Format("HttpResponseReturn/{0}", action));
            request.Method = HttpMethod.Get;
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            IEnumerable<string> list;
            Assert.True(response.Headers.TryGetValues("Set-Cookie",  out list));
            Assert.Equal("cookie1,cookie2", ((string[]) list)[0]);
        }

        public void SetupHost()
        {
            httpClient = new HttpClient();

            baseAddress = String.Format("http://{0}/", Environment.MachineName);

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "HttpResponseReturn" });

            server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        public void CleanupHost()
        {
            if (server != null)
            {
                server.CloseAsync().Wait();
            }
        }
    }

    public class HttpResponseReturnController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage ReturnHttpResponseMessage()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello");
        }

        [HttpGet]
        public object ReturnHttpResponseMessageAsObject()
        {
            return ReturnHttpResponseMessage();
        }

        [HttpGet]
        public HttpResponseMessage ReturnHttpResponseMessageAsXml()
        {
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new ObjectContent<string>("Hello", new XmlMediaTypeFormatter())
            };
            return response;
        }

        [HttpGet]
        public string ReturnString()
        {
            return "Hello";
        }

        [HttpGet]
        public HttpResponseMessage ReturnMultipleSetCookieHeaders()
        {
            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Headers.Add("Set-Cookie", "cookie1");
            resp.Headers.Add("Set-Cookie", "cookie2");
            return resp; 
        }
    }
}