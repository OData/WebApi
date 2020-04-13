// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CombinedTest
{
    public class CombinedTest : WebHostTestBase<CombinedTest>
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";

        public CombinedTest(WebHostTestFixture<CombinedTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                CombinedEdmModel.GetEdmModel(configuration));
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                CombinedEdmModel.GetEdmModelByModelBoundAPI(configuration));
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=NoExpandOrders", "expand")]
        [InlineData(CustomerBaseUrl + "?$count=true", "count")]
        [InlineData(CustomerBaseUrl + "?$filter=Id eq 1", "filter")]
        [InlineData(CustomerBaseUrl + "?$orderby=Id", "orderby")]
        [InlineData(OrderBaseUrl + "?$top=3", "top")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=NoExpandOrders", "expand")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$count=true", "count")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Id eq 1", "filter")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=Id", "orderby")]
        [InlineData(ModelBoundOrderBaseUrl + "?$top=4", "top")]
        public async Task DefaultQuerySettings(string url, string error)
        {
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error, result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "Orders($count=true)", "count")]
        [InlineData(CustomerBaseUrl, "Orders($filter=Id eq 1)", "filter")]
        [InlineData(CustomerBaseUrl, "Orders($orderby=Id)", "orderby")]
        [InlineData(CustomerBaseUrl, "Orders($top=3)", "top")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($count=true)", "count")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($filter=Id eq 1)", "filter")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($orderby=Id)", "orderby")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($top=3)", "top")]
        public async Task QueryAttributeOnEntityTypeNegative(string entitySetUrl, string expandOption, string error)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error, result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "CountableOrders($count=true)")]
        [InlineData(CustomerBaseUrl, "Orders($filter=Name eq 'test')")]
        [InlineData(CustomerBaseUrl, "Orders($orderby=Name)")]
        [InlineData(CustomerBaseUrl, "Orders($top=2)")]
        [InlineData(ModelBoundCustomerBaseUrl, "CountableOrders($count=true)")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($filter=Name eq 'test')")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($orderby=Name)")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($top=2)")]
        public async Task QueryAttributeOnEntityTypePositive(string entitySetUrl, string expandOption)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "?$expand=Orders($expand=Customers($count=true))", "count")]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($expand=Order($top=3))", "top")]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders($expand=Customers($count=true))", "count")]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($expand=Order($top=3))", "top")]
        public async Task QuerySettingsOnPropertyNegative(string entitySetUrl, string url, string error)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error, result);
        }

        [Theory]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($count=true)")]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($top=1)")]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($count=true)")]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($top=1)")]
        public async Task QuerySettingsOnPropertyPositive(string entitySetUrl, string url)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}