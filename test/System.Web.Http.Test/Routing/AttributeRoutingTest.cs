// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class AttributeRoutingTest
    {
        [Fact]
        public void Routing_BindsIdParameter()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Controller/42");

            var response = SubmitRequest(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("Get42", GetContentValue<string>(response));
        }

        [Fact]
        public void Routing_ConstrainsRoutesToHttpMethod()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "http://localhost/Controller/42");

            var response = SubmitRequest(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("Put42", GetContentValue<string>(response));
        }

        [Fact]
        public void WildcardParameters_GetBound()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Wildcard/a/b/c");

            var response = SubmitRequest(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("a/b/c", GetContentValue<string>(response));
        }

        private static HttpResponseMessage SubmitRequest(HttpRequestMessage request)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpAttributeRoutes();

            HttpServer server = new HttpServer(config);
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                return client.SendAsync(request, CancellationToken.None).Result;
            }
        }

        private static T GetContentValue<T>(HttpResponseMessage response)
        {
            T value;
            response.TryGetContentValue<T>(out value);
            return value;
        }
    }

    public class AttributedController : ApiController
    {
        [HttpGet("Controller/{id}")]
        public string Get(string id)
        {
            return "Get" + id;
        }

        [HttpPut("Controller/{id}")]
        public string Put(string id)
        {
            return "Put" + id;
        }

        [HttpGet("Wildcard/{*wildcard}")]
        public string Wildcard(string wildcard)
        {
            return wildcard;
        }
    }
}
