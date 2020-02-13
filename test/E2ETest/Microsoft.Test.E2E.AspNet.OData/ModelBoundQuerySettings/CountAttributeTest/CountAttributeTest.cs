// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CountAttributeTest
{
    public class CountAttributeTest : WebHostTestBase<CountAttributeTest>
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";

        public CountAttributeTest(WebHostTestFixture<CountAttributeTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                CountAttributeEdmModel.GetEdmModel(configuration));
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                CountAttributeEdmModel.GetEdmModelByModelBoundAPI(configuration));
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$count=true", "entity set 'Customers'")]
        [InlineData(CustomerBaseUrl + "(1)/Addresses?$count=true", "property 'Addresses'")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$count=true", "entity set 'Customers'")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Addresses?$count=true", "property 'Addresses'")]
        public async Task NonCountByDefault(string entitySetUrl, string error)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error + " cannot be used for $count", result);
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$count=true", (int)HttpStatusCode.OK, "")]
        [InlineData(CustomerBaseUrl + "?$expand=CountableOrders($count=true)", (int)HttpStatusCode.OK, "")]
        [InlineData(CustomerBaseUrl + "(1)/CountableOrders?$count=true", (int)HttpStatusCode.OK, "")]
        [InlineData(CustomerBaseUrl + "(1)/Addresses2?$count=true", (int)HttpStatusCode.OK, "")]
        [InlineData(OrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder?$count=true",
            (int)HttpStatusCode.BadRequest,
            "entity set 'Orders/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder'")]
        [InlineData(ModelBoundOrderBaseUrl + "?$count=true", (int)HttpStatusCode.OK, "")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=CountableOrders($count=true)", (int)HttpStatusCode.OK, "")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/CountableOrders?$count=true", (int)HttpStatusCode.OK, "")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Addresses2?$count=true", (int)HttpStatusCode.OK, "")]
        [InlineData(ModelBoundOrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder?$count=true",
            (int)HttpStatusCode.BadRequest,
            "entity set 'Orders/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder'")]
        public async Task CountOnStructuredType(string url, int statusCode, string error)
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

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.OK)
            {
                Assert.Contains("odata.count", result);
            }
            else
            {
                Assert.Contains(error + " cannot be used for $count", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$count=true", "property 'Orders'")]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($count=true)", "property 'Orders'")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$count=true", "property 'Orders'")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($count=true)", "property 'Orders'")]
        public async Task CountOnProperty(string url, string error)
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
            Assert.Contains(error + " cannot be used for $count", result);
        }
    }
}