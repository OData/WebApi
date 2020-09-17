using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchHttpRequestMessageExtensionsTest
    {
        private static ODataMessageQuotas _odataMessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };

        [Theory]
        // if no accept header, return multipart/mixed
        [InlineData(null, "multipart/mixed")]

        // if accept is multipart/mixed, return multipart/mixed
        [InlineData(new[] { "multipart/mixed" }, "multipart/mixed")]
        
        // if accept is application/json, return application/json
        [InlineData(new[] { "application/json" }, "application/json")]
        
        // if accept is application/json with charset, return application/json
        [InlineData(new[] { "application/json; charset=utf-8" }, "application/json")]
        
        // if multipart/mixed is high proprity, return multipart/mixed
        [InlineData(new[] { "multipart/mixed;q=0.9", "application/json;q=0.5" }, "multipart/mixed")]
        [InlineData(new[] { "application/json;q=0.5", "multipart/mixed;q=0.9" }, "multipart/mixed")]
        
        // if application/json is high proprity, return application/json
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed;q=0.5" }, "application/json")]
        [InlineData(new[] { "multipart/mixed;q=0.5", "application/json;q=0.9" }, "application/json")]

        // if priorities are same, return first
        [InlineData(new[] { "multipart/mixed;q=0.9", "application/json;q=0.9" }, "multipart/mixed")]
        [InlineData(new[] { "multipart/mixed", "application/json" }, "multipart/mixed")]

        // if priorities are same, return first
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed;q=0.9" }, "application/json")]
        [InlineData(new[] { "application/json", "multipart/mixed" }, "application/json")]

        // no priority has q=1.0
        [InlineData(new[] { "application/json", "multipart/mixed;q=0.9" }, "application/json")]
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed" }, "multipart/mixed")]

        public async Task CreateODataBatchResponseAsync(string[] accept, string expected)
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/$batch");
            var responses = new[] { new ChangeSetResponseItem(Enumerable.Empty<HttpContext>()) };

            if (accept != null)
            {
                request.Headers.Add("Accept", accept);
            }

            await request.CreateODataBatchResponseAsync(responses, _odataMessageQuotas);

            Assert.StartsWith(expected, request.HttpContext.Response.ContentType);
        }

        [Theory]
        // if no contentType, return multipart/mixed
        [InlineData(null, "multipart/mixed")]
        // if contentType is application/json, return application/json
        [InlineData("application/json", "application/json")]
        [InlineData("application/json; charset=utf-8", "application/json")]
        // if contentType is multipart/mixed, return multipart/mixed
        [InlineData("multipart/mixed", "multipart/mixed")]

        public async Task CreateODataBatchResponseAsyncWhenNoAcceptHeader(string contentType, string expected)
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/$batch");
            var responses = new[] { new ChangeSetResponseItem(Enumerable.Empty<HttpContext>()) };

            if (contentType != null)
            {
                request.ContentType = contentType;
            }
            
            await request.CreateODataBatchResponseAsync(responses, _odataMessageQuotas);

            Assert.False(request.Headers.ContainsKey("Accept")); // check no accept header
            Assert.StartsWith(expected, request.HttpContext.Response.ContentType);
        }
    }
}
