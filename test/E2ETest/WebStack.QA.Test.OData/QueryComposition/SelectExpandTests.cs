using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class SelectExpandTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                  typeof(IAssembliesResolver),
                  new TestAssemblyResolver(
                      typeof(SelectCustomerController),
                      typeof(EFSelectCustomersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("selectexpand", "selectexpand", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);

            EntitySetConfiguration<SelectCustomer> customers = builder.EntitySet<SelectCustomer>("SelectCustomer");
            customers.EntityType.Action("CreditRating").Returns<double>();
            customers.EntityType.Collection.Action("PremiumCustomers").ReturnsCollectionFromEntitySet<SelectCustomer>("SelectCustomer");

            builder.EntitySet<EFSelectCustomer>("EFSelectCustomers");
            builder.EntitySet<EFSelectOrder>("EFSelectOrders");
            builder.EntitySet<SelectOrderDetail>("SelectOrderDetail");
            builder.EntityType<SelectPremiumCustomer>();
            builder.EntitySet<SelectOrder>("SelectOrder");
            builder.EntitySet<SelectBonus>("SelectBonus");
            builder.Action("ResetDataSource");

            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        [Fact]
        public void QueryJustThePropertiesOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
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
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().All(p => p.Name != "#Container.CreditRating")));
        }

        [Fact]
        public void QueryJustTheActionsOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Default.*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
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
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Any(p => p.Name == "#Default.CreditRating")));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=minimalmetadata"));
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
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 1 && x.Properties().All(p => p.Name == "Name")));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Id,Name&$expand=SelectOrders($select=Id)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
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
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 3));
            Assert.True(customers.OfType<JObject>().All(x => (int)x["Id"] == ((JArray)x["SelectOrders"]).Count));
        }

        [Fact]
        public void QueryASubSetOfThePropertiesPresentOnlyInADerivedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id,WebStack.QA.Test.OData.QueryComposition.SelectPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = response.Content.ReadAsAsync<JObject>().Result;

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
        }

        [Fact]
        public void QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInline()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$select=Id&$expand=SelectOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
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
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationProperties()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id&" +
                "$expand=SelectOrders,WebStack.QA.Test.OData.QueryComposition.SelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = response.Content.ReadAsAsync<JObject>().Result;

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 3));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
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
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$select=Id,SelectOrders&$expand=SelectOrders($expand=OrderDetails)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
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
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(4, order.Properties().Count());
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
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id&" +
                "$expand=WebStack.QA.Test.OData.QueryComposition.SelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
           
            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = response.Content.ReadAsAsync<JObject>().Result;

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray bonuses = customer["Bonuses"] as JArray;
                Assert.Equal((int)customer["Id"], bonuses.Count);
                foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
                {
                    Assert.Equal(2, bonus.Properties().Count());
                }
            }
        }

        [Fact]
        public void QueryForAnEntryWithExpandNavigationPropertyExceedPageSize()
        {
            // Arrange
            RestoreData();
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders", BaseAddress);
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
            
            var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Equal(expandProp.Count, 2);
            Assert.Equal(expandProp[0]["Id"], 1);
            Assert.Equal(expandProp[1]["Id"], 2);
        }

        [Fact]
        public void NestedDollarCountInDollarExpandWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($count=true)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 4));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                Assert.Equal((int)customer["Id"], (int)customer["SelectOrders@odata.count"]);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public void NestedDollarCountInDollarExpandWithNestedDollarFilterWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($filter=Id lt 1;$count=true)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);
            Console.WriteLine(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 4));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.NotNull(orders);

                int customerId = (int)customer["Id"];
                if (customerId == 0)
                {
                    // the "SelectOrders" in the first customer is empty.
                    Assert.Equal(0, orders.Count);
                    Assert.Equal(0, (int)customer["SelectOrders@odata.count"]);
                }
                else
                {
                    // the "SelectOrders" in other customers has only one entity, because the result is filtered by "Id" < 1.
                    Assert.Equal(1, orders.Count);
                    Assert.Equal(1, (int)customer["SelectOrders@odata.count"]);
                }

                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public void NestedNestedDollarCountInDollarExpandWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($count=true;$expand=OrderDetails($count=true))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 4));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                Assert.Equal((int)customer["Id"], (int)customer["SelectOrders@odata.count"]);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(5, order.Properties().Count());
                    Assert.Equal((int)order["Id"], (int)order["OrderDetails@odata.count"]);
                }
            }
        }

        [Fact]
        public void NestedNestedSkipInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$expand=SelectOrders($skip=1;$expand=OrderDetails($skip=1))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal(Math.Max(0, (int)customer["Id"] - 1), orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    JArray orderdetails = order["OrderDetails"] as JArray;
                    Assert.Equal(Math.Max(0, (int)order["Id"] - 1), orderdetails.Count());
                }
            }
        }

        [Fact]
        public void NestedNestedTopInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$skip=2&$expand=SelectOrders($skip=1;$top=1;$expand=OrderDetails($top=1))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(8, "value", result);
            JArray customers = (JArray)result["value"];
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal(1, orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    JArray orderdetails = order["OrderDetails"] as JArray;
                    Assert.Equal(1, orderdetails.Count());
                }
            }
        }

        [Fact]
        public void NestedNestedOrderByInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$expand=SelectOrders($orderby=BillingAddress/ZipCode;$expand=OrderDetails($orderby=Price desc))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;
                                                                                                                                                                                                                                                                                                                                                    
            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            JArray orders = customers[9]["SelectOrders"] as JArray;
            Assert.Equal(8, orders[0]["Id"]);
            JArray orderdetails = orders[0]["OrderDetails"] as JArray;
            Assert.Equal(0, orderdetails[0]["Id"]);
        }

        [Fact]
        public void NestedTopSkipOrderByInDollarExpandWorksWithEF()
        {
            // Arrange
            RestoreData();
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders($orderby=Id desc;$skip=1;$top=1)", BaseAddress);
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

            var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Equal(expandProp.Count, 1);
            Assert.Equal(expandProp[0]["Id"], 2);
        }

        private void RestoreData()
        {
            string requestUri = BaseAddress + "/selectexpand/ResetDataSource";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class SelectCustomerController : ODataController
    {
        public IList<SelectCustomer> Customers { get; set; }

        public SelectCustomerController()
        {
            Customers = Enumerable.Range(0, 10).Select(i => new SelectCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                SelectOrders = Enumerable.Range(0, i).Select(j => new SelectOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new SelectAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * -100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new SelectOrderDetail
                    {
                        Id = k,
                        Ammount = k,
                        Name = string.Format("Name {0}", k),
                        Price = k * -1000
                    }).ToList().AsQueryable()
                }).ToList()
            }).ToList();
        }

        [EnableQuery]
        public IQueryable<SelectCustomer> Get()
        {
            return Customers.AsQueryable();
        }

        [EnableQuery]
        public IQueryable<SelectCustomer> PremiumCustomers()
        {
            return Enumerable.Range(0, 10).Select(i => new SelectPremiumCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                Category = string.Format("Category{0}", i),
                Bonuses = Enumerable.Range(0, i).Select(j => new SelectBonus
                {
                    Id = j,
                    Ammount = j * 1000
                }).ToList(),
                SelectOrders = Enumerable.Range(0, i).Select(j => new SelectOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new SelectAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = new List<SelectOrderDetail>().AsQueryable()
                }).ToList(),

            }).AsQueryable();
        }

        public double CreditRating(int key)
        {
            return 0;
        }
    }

    public class EFSelectCustomersController : ODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(PageSize = 2)]
        public IHttpActionResult Get()
        {
            return Ok(_db.Customers);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource")]
        public IHttpActionResult ResetDataSource()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
            return Ok();
        }

        public void Generate()
        {
            var customer = new EFSelectCustomer
            {
                Id = 1,
                SelectOrders = new List<EFSelectOrder>
                {
                    new EFSelectOrder
                    {
                        Id = 3,
                    },
                    new EFSelectOrder
                    {
                        Id = 1,
                    },
                    new EFSelectOrder
                    {
                        Id = 2,
                    }
                }
            };
            _db.Customers.Add(customer);
            _db.SaveChanges();
        }
    }

    public class SampleContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=SelectExpandTest";

        public SampleContext()
            : base(ConnectionString)
        {
        }

        public DbSet<EFSelectCustomer> Customers { get; set; }

        public DbSet<SelectCustomer> SelectCustomers { get; set; }
    }

    public class EFSelectCustomer
    {
        public int Id { get; set; }
        public virtual IList<EFSelectOrder> SelectOrders { get; set; }
    }

    public class EFSelectOrder
    {
        public int Id { get; set; }
    }

    public class SelectCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<SelectOrder> SelectOrders { get; set; }
    }

    public class SelectPremiumCustomer : SelectCustomer
    {
        public string Category { get; set; }
        public IList<SelectBonus> Bonuses { get; set; }
    }

    public class SelectBonus
    {
        public int Id { get; set; }
        public double Ammount { get; set; }
    }

    public class SelectOrder
    {
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public SelectAddress BillingAddress { get; set; }
        public virtual IQueryable<SelectOrderDetail> OrderDetails { get; set; }
    }

    public class SelectAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class SelectOrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Ammount { get; set; }
        public double Price { get; set; }
    }
}
