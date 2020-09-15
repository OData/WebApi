// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DollarQuery
{
    public class DollarQueryTests : WebHostTestBase
    {
        private const string CustomersResourcePath = "/odata/DollarQueryCustomers";
        private const string SingleCustomerResourcePath = "/odata/DollarQueryCustomers(1)";
        private const string ApplicationJsonODataMinimalMetadataStreamingTrue = "application/json;odata.metadata=minimal;odata.streaming=true";
        private const string ApplicationJsonODataMinimalMetadataStreamingFalse = "application/json;odata.metadata=minimal;odata.streaming=false";

        public DollarQueryTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        public static TheoryDataSet<string, string> ODataQueryOptionsData
        {
            get
            {
                var odataQueryOptionsData = new TheoryDataSet<string, string>();

                foreach (var tuple in 
                    new[]{
                        new Tuple<string, string>(CustomersResourcePath, "$filter=Id le 5"),
                        new Tuple<string, string>(CustomersResourcePath, "$filter=contains(Name, '3')"),
                        new Tuple<string, string>(CustomersResourcePath, "$orderby=Id desc"),
                        new Tuple<string, string>(CustomersResourcePath, "$top=1"),
                        new Tuple<string, string>(CustomersResourcePath, "$top=1&$skip=3"),
                        new Tuple<string, string>(CustomersResourcePath, "$orderby=Id desc&top=2&skip=3"),
                        new Tuple<string, string>(CustomersResourcePath, "$select=Id,Name"),
                        new Tuple<string, string>(CustomersResourcePath, "$expand=Orders"),
                        new Tuple<string, string>(CustomersResourcePath, "$select=Orders&$expand=Orders"),
                        new Tuple<string, string>(CustomersResourcePath, "$format=" + Uri.EscapeDataString(ApplicationJsonODataMinimalMetadataStreamingFalse)),
                        new Tuple<string, string>(CustomersResourcePath, "$expand=SpecialOrder($select=Detail)&$filter=Id le 5&$orderby=Id desc&$select=Id&$format=" + Uri.EscapeDataString(ApplicationJsonODataMinimalMetadataStreamingFalse)),
                        new Tuple<string, string>(SingleCustomerResourcePath, "$select=Id,Name"),
                        new Tuple<string, string>(SingleCustomerResourcePath, "$expand=Orders"),
                        new Tuple<string, string>(SingleCustomerResourcePath, "$format=" + Uri.EscapeDataString(ApplicationJsonODataMinimalMetadataStreamingFalse)),
                        new Tuple<string, string>(SingleCustomerResourcePath, "$expand=SpecialOrder($select=Detail)&$select=Id&$format=" + Uri.EscapeDataString(ApplicationJsonODataMinimalMetadataStreamingFalse))
                    })
                {
                    odataQueryOptionsData.Add(tuple.Item1, tuple.Item2);
                }

                return odataQueryOptionsData;
            }
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<DollarQueryCustomer>("DollarQueryCustomers");
            builder.EntitySet<DollarQueryOrder>("DollarQueryOrders");

            return builder.GetEdmModel();
        }

        [Theory]
        [MemberData(nameof(ODataQueryOptionsData))]
        public async Task ODataQueryOptionsInRequestBody_ForSupportedMediaType(string resourcePath, string queryOptionsPayload)
        {   
            string requestUri = this.BaseAddress + resourcePath + "/$query";
            var contentType = "text/plain";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(queryOptionsPayload);
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var response = await this.Client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_ReturnsExpectedResult()
        {
            string requestUri = this.BaseAddress + CustomersResourcePath + "/$query";
            var contentType = "text/plain";
            var queryOptionsPayload = "$filter=Id eq 1";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(queryOptionsPayload);
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            var contentAsString = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("\"value\":[{\"Id\":1,\"Name\":\"Customer Name 1\"}]", contentAsString);
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_PlusQueryOptionsOnRequestUrl()
        {
            string requestUri = this.BaseAddress + CustomersResourcePath + "/$query?$orderby=Id desc";
            var contentType = "text/plain";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent("$filter=Id eq 1 or Id eq 9");
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            var contentAsString = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("\"value\":[{\"Id\":9,\"Name\":\"Customer Name 9\"},{\"Id\":1,\"Name\":\"Customer Name 1\"}]", contentAsString);
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_RepeatedOnRequestUrl()
        {
            string requestUri = this.BaseAddress + CustomersResourcePath + "/$query?$filter=Id eq 1";
            var contentType = "text/plain";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent("$filter=Id eq 1");
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var response = await this.Client.SendAsync(request);
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_ForUnsupportedMediaType()
        {
            string requestUri = this.BaseAddress + CustomersResourcePath + "/$query";
            var contentType = "application/xml";
            var queryOptionsPayload = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><QueryOptions><filter>Id le 5</filter></QueryOptions>";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(queryOptionsPayload);
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var response = await this.Client.SendAsync(request);

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_Empty()
        {
            string requestUri = this.BaseAddress + CustomersResourcePath + "/$query";
            var contentType = "text/plain";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent("");
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var response = await this.Client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_MitigateLimitViolationOnLongUrl()
        {
#if NETCORE
            // KestrelServerLimits.MaxRequestLineSize equals 8192 so setting query string length to that value
            // should be enough to make us breach the threshold
            // NOTE: Change of limit could cause test to start failing
            var queryStringLength = 8192;
#else
            // Microsoft Owin Hosting is used
            // 414 Request-URI too long failure code expected
            var queryStringLength = 16384;
#endif
            var baseUri = this.BaseAddress + CustomersResourcePath;
            var builder = new StringBuilder();
            var loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            builder.AppendFormat("$filter=Name eq '{0}", loremIpsum);
            // Ensure that query string breaches the threshold
            do
            {
                builder.AppendFormat(" {0}", loremIpsum);
            } while (builder.Length < queryStringLength);

            builder.Append("'");
            var queryOptionsString = builder.ToString();

            var getRequest = new HttpRequestMessage(HttpMethod.Get, baseUri + '?' + queryOptionsString);
            getRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJsonODataMinimalMetadataStreamingTrue));

            var getResponse = await this.Client.SendAsync(getRequest);

            // Should fail because threshold is exceeed
            Assert.False(getResponse.IsSuccessStatusCode);

            // Now send the same query string in the request body
            var postRequest = new HttpRequestMessage(HttpMethod.Post, baseUri + "/$query");
            var contentType = "text/plain";
            postRequest.Content = new StringContent(queryOptionsString);
            postRequest.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);

            var postResponse = await this.Client.SendAsync(postRequest);
            // Should pass because the query options were sent in the request body
            Assert.True(postResponse.IsSuccessStatusCode);
        }
    }
}
