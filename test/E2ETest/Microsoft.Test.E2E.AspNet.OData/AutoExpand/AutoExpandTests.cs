// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.AutoExpand
{
    public class AutoExpandTests : WebHostTestBase<AutoExpandTests>
    {
        private const string AutoExpandTestBaseUrl = "{0}/autoexpand/Customers(5)";

        public AutoExpandTests(WebHostTestFixture<AutoExpandTests> fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string, int> AutoExpandTestData
        {
            get
            {
                return new TheoryDataSet<string, int>
                {
                    {AutoExpandTestBaseUrl + "?$select=Order", 2},
                    {AutoExpandTestBaseUrl + "?$select=Id", 3},
                    {AutoExpandTestBaseUrl + "?$expand=Order & $select=Id", 3},
                    {AutoExpandTestBaseUrl + "?$expand=Friend", 3},
                    {AutoExpandTestBaseUrl, 3},
                };
            }
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(
                typeof (CustomersController),
                typeof (PeopleController),
                typeof (NormalOrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute(
                "autoexpand",
                "autoexpand",
                AutoExpandEdmModel.GetEdmModel(configuration));
        }

        [Theory]
        [MemberData(nameof(AutoExpandTestData))]
        public async Task QueryForAnEntryIncludeTheAutoExpandNavigationProperty(string url, int propCount)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(customer.Properties().Count(), propCount);
            VerifyOrderAndChoiceOrder(customer);

            // level one
            JObject friend = customer["Friend"] as JObject;
            JObject order = friend["Order"] as JObject;
            Assert.NotNull(order);
            Assert.Null(order["Choice"]);

            // level two
            friend = friend["Friend"] as JObject;
            Assert.Null(friend["Order"]);
        }

        [Fact]
        public async Task LevelsWithAutoExpandInSameNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format(AutoExpandTestBaseUrl + "?$expand=Friend($levels=0)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(customer);
            VerifyOrderAndChoiceOrder(customer);
            Assert.Null(customer["Friend"]);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("3", 3)]
        [InlineData("max", 4)]
        public async Task LevelsWithAutoExpandInDifferentNavigationProperty(string level, int levelNumber)
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/People?$expand=Friend($levels={1})", BaseAddress, level);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseJson = await response.Content.ReadAsObject<JObject>();
            var people = responseJson["value"] as JArray;
            var he = people[8] as JObject;
            JObject friend = he;
            for (int i = 1; i <= levelNumber; i++)
            {
                friend = friend["Friend"] as JObject;
                Assert.NotNull(friend);
                if (i + 2 <= levelNumber)
                {
                    VerifyOrderAndChoiceOrder(friend);
                }
            }
            Assert.Null(friend["Friend"]);
        }

        [Fact]
        public async Task QueryForAnEntryIncludeTheDerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Customers(8)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = await response.Content.ReadAsObject<JObject>();
            VerifyOrderAndChoiceOrder(customer, special: true);

            // level one
            JObject friend = customer["Friend"] as JObject;
            JObject order = friend["Order"] as JObject;
            Assert.NotNull(order);
            Assert.Null(order["Choice"]);
        }

        [Fact]
        public async Task QueryForAnEntryIncludeTheMultiDerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Customers(9)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = await response.Content.ReadAsObject<JObject>();
            VerifyOrderAndChoiceOrder(customer, special: true, vip: true);

            // level one
            JObject friend = customer["Friend"] as JObject;
            JObject order = friend["Order"] as JObject;
            Assert.NotNull(order);
            Assert.Null(order["Choice"]);
        }

        [Theory]
        [InlineData("{0}/autoexpand/NormalOrders")]
        [InlineData("{0}/autoexpand/NormalOrders(1)")]
        public async Task DerivedAutoExpandNavigationPropertyTest(string url)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains("OrderDetail", result);
        }

        [Theory]
        [InlineData("{0}/autoexpand/NormalOrders?$select=Id", true)]
        [InlineData("{0}/autoexpand/NormalOrders", false)]
        [InlineData("{0}/autoexpand/NormalOrders(2)?$select=Id", true)]
        [InlineData("{0}/autoexpand/NormalOrders(2)", false)]
        [InlineData("{0}/autoexpand/NormalOrders(3)?$select=Id", true)]
        [InlineData("{0}/autoexpand/NormalOrders(3)", false)]
        public async Task DisableAutoExpandWhenSelectIsPresentTest(string url, bool isSelectPresent)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            if (isSelectPresent)
            {
                Assert.DoesNotContain("NotShownOrderDetail4", result);
            }
            else
            {
                Assert.Contains("NotShownOrderDetail4", result);
            }
        }

        [Theory]
        [InlineData("{0}/autoexpand/NormalOrders(2)?$expand=LinkOrder($select=Id)", true)]
        [InlineData("{0}/autoexpand/NormalOrders(2)?$expand=LinkOrder", false)]
        public async Task DisableAutoExpandWhenSelectIsPresentDollarExpandTest(string url, bool isSelectPresent)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains("NotShownOrderDetail4", result);
            if (isSelectPresent)
            {
                Assert.DoesNotContain("NotShownOrderDetail2", result);
            }
            else
            {
                Assert.Contains("NotShownOrderDetail2", result);
            }
        }

        private static void VerifyOrderAndChoiceOrder(JObject customer, bool special = false, bool vip = false)
        {
            JObject order = customer["Order"] as JObject;
            Assert.NotNull(order);

            JObject choice = order["Choice"] as JObject;
            Assert.NotNull(choice);
            Assert.Equal((int) order["Id"]*1000, choice["Amount"]);

            if (special)
            {
                choice = order["SpecialChoice"] as JObject;
                Assert.NotNull(choice);
                Assert.Equal((int) order["Id"]*2000, choice["Amount"]);
            }

            if (vip)
            {
                choice = order["VipChoice"] as JObject;
                Assert.NotNull(choice);
                Assert.Equal((int) order["Id"]*3000, choice["Amount"]);
            }
        }
    }
}
