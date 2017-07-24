﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class JsonSingleResultExpandTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.Routes.MapHttpRoute("api", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public void QueryJustThePropertiesOfTheEntriesOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.Equal(2, result.Properties().Count());
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.True(result.Properties().Count() == 1 && result.Properties().All(p => p.Name == "Name"));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntryOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1/?$select=Id,Name&$expand=JsonSingleResultOrders($select=Id)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JObject.Parse(content);
            Assert.NotNull(result);

            Assert.Equal(3, result.Properties().Count());
            Assert.Equal((int)result["Id"], ((JArray)result["JsonSingleResultOrders"]).Count);
        }

        [Fact]
        public void QueryASubSetOfThePropertiesPresentOnlyInADerivedEntryOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10/?" +
                "$select=Id,WebStack.QA.Test.OData.QueryComposition.JsonSingleResultPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JObject.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(2, result.Properties().Count());
        }

        [Fact]
        public void QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInlineOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/1?$select=Id&$expand=JsonSingleResultOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

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
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationPropertiesOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10?" +
                "$select=Id&" +
                "$expand=JsonSingleResultOrders,WebStack.QA.Test.OData.QueryComposition.JsonSingleResultPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

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
        public void QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPathOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10?$select=Id,JsonSingleResultOrders&$expand=JsonSingleResultOrders($expand=OrderDetails)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

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
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntriesOnASingleResult()
        {
            string queryUrl = string.Format("{0}/api/JsonSingleResultCustomer/10?" +
                "$select=Id&" +
                "$expand=WebStack.QA.Test.OData.QueryComposition.JsonSingleResultPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

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

    public class JsonSingleResultCustomerController : ApiController
    {
        [EnableQuery(MaxExpansionDepth = 10)]
        public SingleResult<JsonSingleResultCustomer> Get(int id)
        {
            return SingleResult.Create(Enumerable.Range(0, 10).Select(i => new JsonSingleResultCustomer
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
                        Country = string.Format("Country {0}", j),
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
                        Country = string.Format("Country {0}", j),
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
