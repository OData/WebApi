//-----------------------------------------------------------------------------
// <copyright file="EntitySetAggregationTests.cs" company=".NET Foundation">
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
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.EntitySetAggregation
{
    public class EntitySetAggregationTests : WebHostTestBase
    {
        private const string AggregationTestBaseUrl = "{0}/aggregation/Customers";

        public EntitySetAggregationTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("aggregation", "aggregation",
                EntitySetAggregationEdmModel.GetEdmModel(configuration));
        }

        [Theory]
        [InlineData("sum",600)]
        [InlineData("min", 25)]
        [InlineData("max", 225)]
        [InlineData("average", 100)]
        public async Task AggregationOnEntitySetWorks(string method, int expected)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=aggregate(Orders(Price with " + method + " as TotalPrice))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal(expected, TotalPrice);
        }

        [Theory]
        [InlineData("?$apply=aggregate(Orders(Price with sum as TotalPrice, Id with sum as TotalId))")]
        [InlineData("?$apply=aggregate(Orders(Price with sum as TotalPrice), Orders(Id with sum as TotalId))")]
        public async Task MultipleAggregationOnEntitySetWorks(string queryString)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + queryString,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var orders = value.First["Orders"];
            var totalPrice = orders.First["TotalPrice"].ToObject<int>();
            var totalId = orders.First["TotalId"].ToObject<int>();

            // OBS: DB uses sequential ID
            // Each Customer has 2 orders that cost 25*CustomerId and 75*CustomerId
            Assert.Equal(1 * (25 + 75) + 2 * (25 + 75) + 3 * (25 + 75), totalPrice);
            // Sum of the 6 Orders IDs
            Assert.Equal(1 + 2 + 3 + 4 + 5 + 6, totalId); 
        }

        [Fact]
        public async Task AggregationOnEntitySetWorksWithPropertyAggregation()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=aggregate(Id with sum as TotalId, Orders(Price with sum as TotalPrice))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var totalId = value.First["TotalId"].ToObject<int>();
            var orders = value.First["Orders"];
            var totalPrice = orders.First["TotalPrice"].ToObject<int>();

            // OBS: DB uses sequential ID
            // Each Customer has 2 orders that cost 25*CustomerId and 75*CustomerId
            Assert.Equal(1 * (25 + 75) + 2 * (25 + 75) + 3 * (25 + 75), totalPrice);
            // Sum of the first 3 Customers IDs
            Assert.Equal(1 + 2 + 3, totalId); 
        }

        [Fact]
        public async Task TestAggregationOnEntitySetsWithArithmeticOperators()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=aggregate(Orders(Price mul Price with sum as TotalPrice))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal((1 + 4 + 9) * (25 * 25 + 75 * 75), TotalPrice);
        }

        [Fact]
        public async Task TestAggregationOnEntitySetsWithArithmeticOperatorsAndPropertyNavigation()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=aggregate(Orders(SaleInfo/Quantity mul SaleInfo/UnitPrice with sum as TotalPrice))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal(1 * (25 + 75) + 2 * (25 + 75) + 3 * (25 + 75), TotalPrice);
        }

        [Fact]
        public async Task AggregationOnEntitySetWorksWithGroupby()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Name), aggregate(Orders(Price with sum as TotalPrice)))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var value = result["value"];

            Assert.Equal("Customer0", value[0]["Name"].ToObject<string>());
            Assert.Equal("Customer1", value[1]["Name"].ToObject<string>());

            var customerZeroOrders = value[0]["Orders"];
            var customerZeroPrice = customerZeroOrders.First["TotalPrice"].ToObject<int>();
            Assert.Equal(1 * (25 + 75) + 3 * (25 + 75), customerZeroPrice);

            var customerOneOrders = value[1]["Orders"];
            var customerOnePrice = customerOneOrders.First["TotalPrice"].ToObject<int>();
            Assert.Equal(2 * (25 + 75), customerOnePrice);
        }
    }
}
