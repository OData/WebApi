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
        [InlineData("GET", "controller/42", "Get42")]
        // Tests inline route constraints
        [InlineData("GET", "controller/Ethan", "GetByNameEthan")]
        // Tests the HTTP method constraint
        [InlineData("PUT", "controller/42", "Put42")]
        // Tests optional parameters
        [InlineData("GET", "optional/1/2", "Optional12")]
        [InlineData("GET", "optional/1", "Optional1")]
        [InlineData("GET", "optional", "Optional0")]
        [InlineData("GET", "optionalwconstraint", "OptionalWithConstraint")]
        // Tests default values
        [InlineData("GET", "default/1/2", "Default12")]
        [InlineData("GET", "default/1", "Default1D2")]
        [InlineData("GET", "default", "DefaultD1D2")]
        // Test wildcard parameters
        [InlineData("GET", "wildcard/a/b/c", "Wildcarda/b/c")]
        // Test prefixes
        [InlineData("GET", "prefix", "PrefixedGet")]
        [InlineData("GET", "prefix/123", "PrefixedGetById123")]
        public void AttributeRouting_RoutesToAction(string httpMethod, string uri, string responseBody)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri);

            var response = SubmitRequest(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(responseBody, GetContentValue<string>(response));
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
        [HttpGet("controller/{id:int}")]
        public string Get(int id)
        {
            return "Get" + id;
        }

        [HttpGet("controller/{name}")]
        public string GetByName(string name)
        {
            return "GetByName" + name;
        }

        [HttpPut("controller/{id}")]
        public string Put(string id)
        {
            return "Put" + id;
        }

        [HttpGet("optional/{opt1?}/{opt2?}")]
        public string Optional(int opt1, string opt2)
        {
            return "Optional" + opt1 + opt2;
        }

        [HttpGet("optionalwconstraint/{opt:int?}")]
        public string OptionalWithConstraint(string opt)
        {
            return "OptionalWithConstraint" + opt;
        }

        [HttpGet("default/{default1=D1}/{default2=D2}")]
        public string Default(string default1, string default2)
        {
            return "Default" + default1 + default2;
        }

        [HttpGet("wildcard/{*wildcard}")]
        public string Wildcard(string wildcard)
        {
            return "Wildcard" + wildcard;
        }
    }

    [RoutePrefix("prefix")]
    public class PrefixedController : ApiController
    {
        [HttpGet("")]
        public string Get()
        {
            return "PrefixedGet";
        }

        [HttpGet("{id}")]
        public string GetById(int id)
        {
            return "PrefixedGetById" + id;
        }
    }
}
