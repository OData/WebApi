// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

            EntitySetConfiguration<DollarQueryCustomer> dollarQueryCustomers =
                builder.EntitySet<DollarQueryCustomer>("DollarQueryCustomers");

            EntitySetConfiguration<DollarQueryOrder> dollarQueryOrders =
                builder.EntitySet<DollarQueryOrder>("DollarQueryOrders");

            return builder.GetEdmModel();
        }

        [Theory]
        [MemberData(nameof(ODataQueryOptionsData))]
        public async Task TestODataQueryOptionsInRequestBody_ForSupportedMediaType(string resourcePath, string queryOptionsPayload)
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
        public async Task TestODataQueryOptionsInRequestBody_ReturnsExpectedResult()
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
        public async Task TestODataQueryOptionsInRequestBody_ForUnsupportedMediaType()
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
        public async Task TestODataQueryOptionsInRequestBody_Empty()
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
    }
}
