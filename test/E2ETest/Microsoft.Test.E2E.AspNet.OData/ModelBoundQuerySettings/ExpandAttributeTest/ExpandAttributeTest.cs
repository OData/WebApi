//-----------------------------------------------------------------------------
// <copyright file="ExpandAttributeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.ExpandAttributeTest
{
    public class ExpandAttributeTest : WebHostTestBase
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";

        public ExpandAttributeTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                ExpandAttributeEdmModel.GetEdmModel(configuration));
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                ExpandAttributeEdmModel.GetEdmModelByModelBoundAPI(configuration));
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=NoExpandOrders")]
        [InlineData(CustomerBaseUrl + "?$expand=Order")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=NoExpandOrders")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Order")]
        public async Task NonExpandByDefault(string url)
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
            Assert.Contains("cannot be used in the $expand query option", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "Orders", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "Customers", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "Customers2", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "NoExpandCustomers", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "Customers", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "Customers2", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "NoExpandCustomers", (int)HttpStatusCode.BadRequest)]
        public async Task ExpandOnEntityType(string entitySetUrl, string expandOption, int statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(statusCode, (int)response.StatusCode);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "?$expand=Orders($expand=Customers2)", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "(1)/Orders?$expand=Customers2", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "?$expand=Orders($expand=Customers)", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl, "(1)/Orders?$expand=Customers", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($expand=Orders)", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl, "(1)/Customers2?$expand=Orders", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($expand=Order)", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "(1)/Customers2?$expand=Order", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl, "?$expand=Order($expand=Customers2)", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "(1)/Order?$expand=Customers2", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "?$expand=Order($expand=Customers)", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "(1)/Order?$expand=Customers", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders($expand=Customers2)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Orders?$expand=Customers2", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders($expand=Customers)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Orders?$expand=Customers", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($expand=Orders)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl, "(1)/Customers2?$expand=Orders", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($expand=Order)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "(1)/Customers2?$expand=Order", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Order($expand=Customers2)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Order?$expand=Customers2", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Order($expand=Customers)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Order?$expand=Customers", (int)HttpStatusCode.BadRequest)]
        public async Task ExpandOnProperty(string entitySetUrl, string url, int statusCode)
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
            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $expand query option", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "?$expand=Orders($expand=Customers($expand=Orders($expand=Customers)))", 3)]
        [InlineData(CustomerBaseUrl, "(1)/Orders?$expand=Customers($expand=Orders($expand=Customers))", 2)]
        [InlineData(CustomerBaseUrl, "?$expand=AutoExpandOrder($expand=RelatedOrder($levels=3)),Friend($levels=3)", 3)]
        [InlineData(CustomerBaseUrl, "(1)/Friend?$expand=Friend($levels=3)", 2)]
        [InlineData(CustomerBaseUrl, "?$expand=Friend($expand=Friend($expand=Friend($levels=max)))", 3)]
        [InlineData(CustomerBaseUrl, "(1)/Friend?$expand=Friend($expand=Friend($levels=max))", 2)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders($expand=Customers($expand=Orders($expand=Customers)))", 3)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Orders?$expand=Customers($expand=Orders($expand=Customers))", 2)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=AutoExpandOrder($expand=RelatedOrder($levels=3)),Friend($levels=3)", 3)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Friend?$expand=Friend($levels=3)", 2)]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Friend($expand=Friend($expand=Friend($levels=max)))", 3)]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Friend?$expand=Friend($expand=Friend($levels=max))", 2)]
        public async Task ExpandDepth(string url, string queryoption, int maxDepth)
        {
            string queryUrl =
                string.Format(
                    url + queryoption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The maximum depth allowed is " + maxDepth, result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl)]
        [InlineData(ModelBoundCustomerBaseUrl)]
        public async Task AutomaticExpand(string url)
        {
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("AutoExpandOrder", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl)]
        [InlineData(CustomerBaseUrl + "(9)")]
        [InlineData(ModelBoundCustomerBaseUrl)]
        [InlineData(ModelBoundCustomerBaseUrl + "(9)")]
        public async Task AutomaticExpandInDerivedType(string url)
        {
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("SpecialOrder", result);
        }
    }
}
