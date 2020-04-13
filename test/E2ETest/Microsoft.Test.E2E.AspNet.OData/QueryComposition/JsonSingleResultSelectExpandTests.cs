// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class JsonSingleResultExpandTests : WebHostTestBase<JsonSingleResultExpandTests>
    {
        public JsonSingleResultExpandTests(WebHostTestFixture<JsonSingleResultExpandTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
#if NETCORE
            configuration.MapHttpRoute("api", "api/{controller}/{id?}", defaults: new { action = "Get" });
#else
            configuration.MapHttpRoute("api", "api/{controller}/{id}", new { id = System.Web.Http.RouteParameter.Optional });
#endif
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public async Task QueryJustThePropertiesOfTheEntriesOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.Equal(2, result.Properties().Count());
        }

        [Fact]
        public async Task QueryASubsetOfThePropertiesOfAnEntryOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.True(result.Properties().Count() == 1 && result.Properties().All(p => p.Name == "Name"));
        }

        [Fact]
        public async Task QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntryOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1/?$select=Id,Name&$expand=JsonSingleResultOrders($select=Id)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.Equal(3, result.Properties().Count());
            Assert.Equal((int)result["Id"], ((JArray)result["JsonSingleResultOrders"]).Count);
        }

        [Fact]
        public async Task QueryASubSetOfThePropertiesPresentOnlyInADerivedEntryOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10/?" +
                "$select=Id,Microsoft.Test.E2E.AspNet.OData.QueryComposition.JsonSingleResultPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(2, result.Properties().Count());
        }

        [Fact]
        public async Task QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInlineOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1?$select=Id&$expand=JsonSingleResultOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.Equal(2, result.Properties().Count());
            JArray orders = result["JsonSingleResultOrders"] as JArray;
            Assert.Equal((int)result["Id"], orders.Count);
            foreach (JObject order in (IEnumerable<JToken>)orders)
            {
                Assert.Equal(2, order.Properties().Count());
            }
        }

        [Fact]
        public async Task QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationPropertiesOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10?" +
                "$select=Id&" +
                "$expand=JsonSingleResultOrders,Microsoft.Test.E2E.AspNet.OData.QueryComposition.JsonSingleResultPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);


            Assert.True(result.Properties().Count() == 3);

            JArray orders = result["JsonSingleResultOrders"] as JArray;
            Assert.Equal((int)result["Id"], orders.Count);
            foreach (JObject order in (IEnumerable<JToken>)orders)
            {
                Assert.Equal(2, order.Properties().Count());
            }

            JArray bonuses = result["Bonuses"] as JArray;
            Assert.Equal((int)result["Id"], bonuses.Count);
            foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
            {
                Assert.Equal(2, bonus.Properties().Count());
            }
        }

        [Fact]
        public async Task QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPathOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10?$select=Id,JsonSingleResultOrders&$expand=JsonSingleResultOrders($expand=OrderDetails)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);


            Assert.True(result.Properties().Count() == 2);

            JArray orders = result["JsonSingleResultOrders"] as JArray;
            Assert.Equal((int)result["Id"], orders.Count);
            foreach (JObject order in (IEnumerable<JToken>)orders)
            {
                Assert.Equal(3, order.Properties().Count());
                JArray orderDetails = order["OrderDetails"] as JArray;
                Assert.Equal((int)order["Id"], orderDetails.Count);
                foreach (JObject orderDetail in (IEnumerable<JToken>)orderDetails)
                {
                    Assert.Equal(4, orderDetail.Properties().Count());
                }
            }
        }

        [Fact]
        public async Task QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntriesOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10?" +
                "$select=Id&" +
                "$expand=Microsoft.Test.E2E.AspNet.OData.QueryComposition.JsonSingleResultPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);


            Assert.Equal(2, result.Properties().Count());
            JArray bonuses = result["Bonuses"] as JArray;
            Assert.Equal((int)result["Id"], bonuses.Count);
            foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
            {
                Assert.Equal(2, bonus.Properties().Count());
            }
        }
    }

    public class JsonSingleResultCustomerController : TestNonODataController
    {
        [EnableQuery(MaxExpansionDepth = 10)]
        public TestSingleResult<JsonSingleResultCustomer> Get(int id)
        {
            return TestSingleResult.Create(Enumerable.Range(0, 10).Select(i => new JsonSingleResultCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                JsonSingleResultOrders = Enumerable.Range(0, i).Select(j => new JsonSingleResultOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new JsonSingleResultAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new JsonSingleResultOrderDetail
                    {
                        Id = k,
                        Ammount = k,
                        Name = string.Format("Name {0}", k),
                        Price = k * 1000
                    }).ToList()
                }).ToList()
            }).Concat(
            Enumerable.Range(10, 10).Select(i => new JsonSingleResultPremiumCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                Category = string.Format("Category{0}", i),
                Bonuses = Enumerable.Range(0, i).Select(j => new JsonSingleResultBonus
                {
                    Id = j,
                    Ammount = j * 1000
                }).ToList(),
                JsonSingleResultOrders = Enumerable.Range(0, i).Select(j => new JsonSingleResultOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new JsonSingleResultAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new JsonSingleResultOrderDetail
                    {
                        Id = k,
                        Ammount = k,
                        Name = string.Format("Name {0}", k),
                        Price = k * 1000
                    }).ToList()
                }).ToList()
            })).AsQueryable().Where(x => x.Id == id));
        }
    }

    public class JsonSingleResultCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<JsonSingleResultOrder> JsonSingleResultOrders { get; set; }
    }

    public class JsonSingleResultPremiumCustomer : JsonSingleResultCustomer
    {
        public string Category { get; set; }
        public IList<JsonSingleResultBonus> Bonuses { get; set; }
    }

    public class JsonSingleResultBonus
    {
        public int Id { get; set; }
        public double Ammount { get; set; }
    }

    public class JsonSingleResultOrder
    {
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public JsonSingleResultAddress BillingAddress { get; set; }
        public virtual IList<JsonSingleResultOrderDetail> OrderDetails { get; set; }
    }

    public class JsonSingleResultAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class JsonSingleResultOrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Ammount { get; set; }
        public double Price { get; set; }
    }
}
