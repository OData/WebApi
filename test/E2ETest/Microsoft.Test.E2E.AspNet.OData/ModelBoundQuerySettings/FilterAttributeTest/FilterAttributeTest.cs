// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest
{
    public class FilterAttributeTest : WebHostTestBase<FilterAttributeTest>
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string CarBaseUrl = "{0}/enablequery/Cars";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";
        private const string ModelBoundCarBaseUrl = "{0}/modelboundapi/Cars";

        public FilterAttributeTest(WebHostTestFixture<FilterAttributeTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController),
                    typeof(CarsController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                FilterAttributeEdmModel.GetEdmModel(configuration));
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                FilterAttributeEdmModel.GetEdmModelByModelBoundAPI(configuration));
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$filter=Id eq 1")]
        [InlineData(CustomerBaseUrl + "?$filter=Id eq 1 and Name eq 'test'")]
        [InlineData(OrderBaseUrl + "?$expand=UnFilterableCustomers($filter=Id eq 1)")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Id eq 1")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Id eq 1 and Name eq 'test'")]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=UnFilterableCustomers($filter=Id eq 1)")]
        public async Task NonFilterableByDefault(string url)
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
            Assert.Contains("cannot be used in the $filter query option.", result);
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$filter=Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$filter=Id eq 1", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Name eq 'test'",
            (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Price eq 1",
            (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=SpecialName eq 'test'",
            (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($filter=Id eq 1 and Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($filter=CarNumber eq 1)", (int)HttpStatusCode.BadRequest)]
        [InlineData(CarBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(CarBaseUrl + "?$filter=CarNumber eq 1", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$filter=Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$filter=Id eq 1", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Name eq 'test'",
            (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Price eq 1",
            (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=SpecialName eq 'test'",
            (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($filter=Id eq 1 and Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($filter=CarNumber eq 1)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCarBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCarBaseUrl + "?$filter=CarNumber eq 1", (int)HttpStatusCode.BadRequest)]
        public async Task FilterOnEntityType(string entitySetUrl, int statusCode)
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

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $filter query option.", result);
            }
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$expand=Customers($filter=Id eq 1 and Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "(1)/Customers?$filter=Id eq 1 and Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($filter=Name eq 'test')", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$filter=Name eq 'test'", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($filter=Id eq 1)", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$filter=Id eq 1", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($filter=Id eq 1 and Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "(1)/Customers?$filter=Id eq 1 and Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($filter=Name eq 'test')", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$filter=Name eq 'test'", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($filter=Id eq 1)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$filter=Id eq 1", (int)HttpStatusCode.OK)]
        public async Task FilterOnProperty(string entitySetUrl, int statusCode)
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

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $filter query option.", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$filter=AutoExpandOrder/Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$filter=AutoExpandOrder/Id eq 1", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "?$filter=Address/Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$filter=Address/Street eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($filter=AutoExpandOrder/Id eq 1)", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($filter=AutoExpandOrder/Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($filter=Address/Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=AutoExpandOrder/Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=AutoExpandOrder/Id eq 1", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Address/Name eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Address/Street eq 'test'", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($filter=AutoExpandOrder/Id eq 1)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($filter=AutoExpandOrder/Name eq 'test')", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($filter=Address/Name eq 'test')", (int)HttpStatusCode.OK)]
        public async Task FilterSingleNavigationOrComplexProperty(string entitySetUrl, int statusCode)
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

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $filter query option.", result);
            }
        }
    }
}