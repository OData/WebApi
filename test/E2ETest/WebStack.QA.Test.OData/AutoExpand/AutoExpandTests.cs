﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.AutoExpand
{
    public class AutoExpandTests : ODataTestBase
    {
        private const string AutoExpandTestBaseUrl = "{0}/autoexpand/Customers(5)";

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

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(
                    typeof (CustomersController), 
                    typeof (PeopleController),
                    typeof (NormalOrdersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute(
                "autoexpand", 
                "autoexpand", 
                AutoExpandEdmModel.GetEdmModel(configuration));
        }

        [Theory]
        [PropertyData("AutoExpandTestData")]
        public void QueryForAnEntryIncludeTheAutoExpandNavigationProperty(string url, int propCount)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = response.Content.ReadAsAsync<JObject>().Result;
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
        public void LevelsWithAutoExpandInSameNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format(AutoExpandTestBaseUrl + "?$expand=Friend($levels=0)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var customer = response.Content.ReadAsAsync<JObject>().Result;
            Assert.NotNull(customer);
            VerifyOrderAndChoiceOrder(customer);
            Assert.Null(customer["Friend"]);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("3", 3)]
        [InlineData("max", 4)]
        public void LevelsWithAutoExpandInDifferentNavigationProperty(string level, int levelNumber)
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/People?$expand=Friend($levels={1})", BaseAddress, level);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseJson = response.Content.ReadAsAsync<JObject>().Result;
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
        public void QueryForAnEntryIncludeTheDerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Customers(8)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = response.Content.ReadAsAsync<JObject>().Result;
            VerifyOrderAndChoiceOrder(customer, special: true);

            // level one
            JObject friend = customer["Friend"] as JObject;
            JObject order = friend["Order"] as JObject;
            Assert.NotNull(order);
            Assert.Null(order["Choice"]);
        }

        [Fact]
        public void QueryForAnEntryIncludeTheMultiDerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Customers(9)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = response.Content.ReadAsAsync<JObject>().Result;
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
        public void DerivedAutoExpandNavigationPropertyTest(string url)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("OrderDetail", result);
        }

        [Theory]
        [InlineData("{0}/autoexpand/NormalOrders?$select=Id", true)]
        [InlineData("{0}/autoexpand/NormalOrders", false)]
        [InlineData("{0}/autoexpand/NormalOrders(2)?$select=Id", true)]
        [InlineData("{0}/autoexpand/NormalOrders(2)", false)]
        [InlineData("{0}/autoexpand/NormalOrders(3)?$select=Id", true)]
        [InlineData("{0}/autoexpand/NormalOrders(3)", false)]
        public void DisableAutoExpandWhenSelectIsPresentTest(string url, bool isSelectPresent)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = response.Content.ReadAsStringAsync().Result;
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
        public void DisableAutoExpandWhenSelectIsPresentDollarExpandTest(string url, bool isSelectPresent)
        {
            // Arrange
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = response.Content.ReadAsStringAsync().Result;
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
