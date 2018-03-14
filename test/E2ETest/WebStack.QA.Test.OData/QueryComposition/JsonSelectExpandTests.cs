using System;
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
    public class JsonSelectExpandTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Formatters.JsonFormatter.Indent = true;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public void QueryJustThePropertiesOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer/?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 2));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntry()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 1 && x.Properties().All(p => p.Name == "Name")));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntry()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer/?$select=Id,Name&$expand=JsonSelectOrders($select=Id)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);

            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 3));
            Assert.True(result.OfType<JObject>().All(x => (int)x["Id"] == ((JArray)x["JsonSelectOrders"]).Count));
        }

        [Fact]
        public void QueryASubSetOfThePropertiesPresentOnlyInADerivedEntry()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer/PremiumCustomers?" +
                "$select=Id,WebStack.QA.Test.OData.QueryComposition.JsonSelectPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);

            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 2));
        }

        [Fact]
        public void QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInline()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer?$select=Id&$expand=JsonSelectOrders($select=*)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);

            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)result)
            {
                JArray orders = customer["JsonSelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(2, order.Properties().Count());
                }
            }
        }

        [Fact]
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationProperties()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer/PremiumCustomers?" +
                "$select=Id&" +
                "$expand=JsonSelectOrders,WebStack.QA.Test.OData.QueryComposition.JsonSelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);

            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 3));
            foreach (JObject customer in (IEnumerable<JToken>)result)
            {
                JArray orders = customer["JsonSelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(2, order.Properties().Count());
                }

                JArray bonuses = customer["Bonuses"] as JArray;
                Assert.Equal((int)customer["Id"], bonuses.Count);
                foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
                {
                    Assert.Equal(2, bonus.Properties().Count());
                }
            }
        }

        [Fact]
        public void QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPath()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer?$select=Id,JsonSelectOrders&$expand=JsonSelectOrders($expand=OrderDetails)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);

            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)result)
            {
                JArray orders = customer["JsonSelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
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
        }

        [Fact]
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntries()
        {
            string queryUrl = string.Format("{0}/api/JsonSelectCustomer/PremiumCustomers?" +
                "$select=Id&" +
                "$expand=WebStack.QA.Test.OData.QueryComposition.JsonSelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JArray result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JArray.Parse(content);
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);

            Assert.True(result.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)result)
            {
                JArray bonuses = customer["Bonuses"] as JArray;
                Assert.Equal((int)customer["Id"], bonuses.Count);
                foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
                {
                    Assert.Equal(2, bonus.Properties().Count());
                }
            }
        }

    }

    public class JsonSelectCustomerController : ApiController
    {
        public IList<JsonSelectCustomer> Customers { get; set; }

        public JsonSelectCustomerController()
        {
            Customers = Enumerable.Range(0, 10).Select(i => new JsonSelectCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                JsonSelectOrders = Enumerable.Range(0, i).Select(j => new JsonSelectOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new JsonSelectAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new JsonSelectOrderDetail
                    {
                        Id = k,
                        Ammount = k,
                        Name = string.Format("Name {0}", k),
                        Price = k * 1000
                    }).ToList()
                }).ToList()
            }).ToList();
        }

        [EnableQuery(MaxExpansionDepth = 10)]
        public IQueryable<JsonSelectCustomer> Get()
        {
            return Customers.AsQueryable();
        }

        [EnableQuery(MaxExpansionDepth = 10)]
        public IQueryable<JsonSelectCustomer> PremiumCustomers()
        {
            return Enumerable.Range(0, 10).Select(i => new JsonSelectPremiumCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                Category = string.Format("Category{0}", i),
                Bonuses = Enumerable.Range(0, i).Select(j => new JsonSelectBonus
                {
                    Id = j,
                    Ammount = j * 1000
                }).ToList(),
                JsonSelectOrders = Enumerable.Range(0, i).Select(j => new JsonSelectOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new JsonSelectAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new JsonSelectOrderDetail
                    {
                        Id = k,
                        Ammount = k,
                        Name = string.Format("Name {0}", k),
                        Price = k * 1000
                    }).ToList()
                }).ToList(),
            }).AsQueryable();
        }

        public double CreditRating(int key)
        {
            return 0;
        }
    }

    public class JsonSelectCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<JsonSelectOrder> JsonSelectOrders { get; set; }
    }

    public class JsonSelectPremiumCustomer : JsonSelectCustomer
    {
        public string Category { get; set; }
        public IList<JsonSelectBonus> Bonuses { get; set; }
    }

    public class JsonSelectBonus
    {
        public int Id { get; set; }
        public double Ammount { get; set; }
    }

    public class JsonSelectOrder
    {
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public JsonSelectAddress BillingAddress { get; set; }
        public virtual IList<JsonSelectOrderDetail> OrderDetails { get; set; }
    }

    public class JsonSelectAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class JsonSelectOrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Ammount { get; set; }
        public double Price { get; set; }
    }
}
