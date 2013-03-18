// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class AttributeRoutingTest
    {
        [Theory]
        [InlineData("GET", "Controller/42", "Get42")]
        // Tests inline route constraints
        [InlineData("GET", "Controller/Ethan", "GetByNameEthan")]
        // Tests the HTTP method constraint
        [InlineData("PUT", "Controller/42", "Put42")]
        // Tests optional parameters
        [InlineData("GET", "Optional/1/2", "Optional12")]
        [InlineData("GET", "Optional/1", "Optional1")]
        [InlineData("GET", "Optional", "Optional")]
        [InlineData("GET", "OptionalWithConstraint", "OptionalWithConstraint")]
        // Tests default values
        [InlineData("GET", "Default/1/2", "Default12")]
        [InlineData("GET", "Default/1", "Default1D2")]
        [InlineData("GET", "Default", "DefaultD1D2")]
        // Test wildcard parameters
        [InlineData("GET", "Wildcard/a/b/c", "Wildcarda/b/c")]
        public void AttributeRouting_RoutesToAction(string httpMethod, string uri, string responseBody)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri);

            var response = SubmitRequest(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(responseBody, GetContentValue<string>(response));
        }

        private static HttpResponseMessage SubmitRequest(HttpRequestMessage request)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

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
        [HttpGet("Controller/{id:int}")]
        public string Get(int id)
        {
            return "Get" + id;
        }

        [HttpGet("Controller/{name}")]
        public string GetByName(string name)
        {
            return "GetByName" + name;
        }

        [HttpGet("Controller/Get1")]
        [HttpGet("Controller/Get2")]
        public string MultipleGet()
        {
            return "MultipleGet";
        }

        [HttpPut("Controller/{id}")]
        public string Put(string id)
        {
            return "Put" + id;
        }

        [HttpGet("Optional/{opt1?}/{opt2?}")]
        public string Optional(string opt1 = null, string opt2 = null)
        {
            return "Optional" + opt1 + opt2;
        }

        [HttpGet("OptionalWithConstraint/{opt:int?}")]
        public string OptionalWithConstraint(string opt = null)
        {
            return "OptionalWithConstraint" + opt;
        }

        [HttpGet("Default/{default1=D1}/{default2=D2}")]
        public string Default(string default1, string default2)
        {
            return "Default" + default1 + default2;
        }

        [HttpGet("Wildcard/{*wildcard}")]
        public string Wildcard(string wildcard)
        {
            return "Wildcard" + wildcard;
        }
    }
}
