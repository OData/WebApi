using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Microsoft.Data.Edm;
using System.Web.Http.OData.Builder;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web.Http.OData;
using System.Net.Http.Headers;
using System.Web.Http.OData.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class SingleResultExpandTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //configuration.EnableQuerySupport();
            configuration.Routes.MapODataServiceRoute("selectexpand", "selectexpand", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            EntitySetConfiguration<SingleResultCustomer> customers = builder.EntitySet<SingleResultCustomer>("SingleResultCustomers");
            EntitySetConfiguration<SingleResultOrderDetail> orderDetails = builder.EntitySet<SingleResultOrderDetail>("SingleResultOrderDetail");
            EntityTypeConfiguration<SingleResultPremiumCustomer> premiumCustomer = builder.Entity<SingleResultPremiumCustomer>();
            customers.EntityType.Action("CreditRating").Returns<double>();
            EntitySetConfiguration<SingleResultOrder> orders = builder.EntitySet<SingleResultOrder>("SingleResultOrder");
            EntitySetConfiguration<SingleResultBonus> bonuses = builder.EntitySet<SingleResultBonus>("SingleResultBonus");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        [Fact]
        public void QueryJustThePropertiesOfTheEntriesOnAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata"));
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
            Assert.True(result.Properties().All(p => p.Name != "#Container.CreditRating"));
        }

        [Fact]
        public void QueryJustTheSingleResultWithoutParameters()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;
            Assert.NotNull(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;
            ;
            result = JObject.Parse(content);
            Assert.NotNull(result);
        }

        [Fact]//(Skip = "OData Uri Parser doesn't support selecting actions. TFS = #681120")]
        public void QueryJustTheActionsOfTheEntriesOnAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=Id,Container.*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata"));
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
            Assert.True(result.Properties().Any(p => p.Name == "#Container.CreditRating"));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryOnAnEntryQuery()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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
            Assert.Equal(1, result.Properties().Count());
            Assert.Equal("Name", result.Properties().Single().Name);
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=Id,Name,SingleResultOrders/Id&$expand=SingleResultOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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
            Assert.True((int)result["Id"] == ((JArray)result["SingleResultOrders"]).Count);
        }

        [Fact]
        public void QueryASubSetOfThePropertiesPresentOnlyInADerivedEntryOnAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(10)?$select=Id,WebStack.QA.Test.OData.QueryComposition.SingleResultPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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
        public void QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInlineForAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)?$select=Id,SingleResultOrders&$expand=SingleResultOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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

            JArray orders = result["SingleResultOrders"] as JArray;
            Assert.Equal((int)result["Id"], orders.Count);
            foreach (JObject order in orders)
            {
                Assert.Equal(3, order.Properties().Count());
            }
        }

        [Fact]
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationPropertiesForAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(10)?$select=Id,SingleResultOrders,WebStack.QA.Test.OData.QueryComposition.SingleResultPremiumCustomer/Bonuses&$expand=SingleResultOrders,WebStack.QA.Test.OData.QueryComposition.SingleResultPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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

            JArray orders = result["SingleResultOrders"] as JArray;
            Assert.Equal((int)result["Id"], orders.Count);
            foreach (JObject order in orders)
            {
                Assert.Equal(3, order.Properties().Count());
            }

            JArray bonuses = result["Bonuses"] as JArray;
            Assert.Equal((int)result["Id"], bonuses.Count);
            foreach (JObject bonus in bonuses)
            {
                Assert.Equal(2, bonus.Properties().Count());
            }
        }

        [Fact]
        public void QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPath()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)?$select=Id,SingleResultOrders,SingleResultOrders/OrderDetails&$expand=SingleResultOrders/OrderDetails", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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

            JArray orders = result["SingleResultOrders"] as JArray;
            Assert.Equal((int)result["Id"], orders.Count);
            foreach (JObject order in orders)
            {
                Assert.Equal(4, order.Properties().Count());
                JArray orderDetails = order["OrderDetails"] as JArray;
                Assert.Equal((int)order["Id"], orderDetails.Count);
                foreach (JObject orderDetail in orderDetails)
                {
                    Assert.Equal(4, orderDetail.Properties().Count());
                }
            }
        }

        [Fact]
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntriesOnAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)?$select=Id,WebStack.QA.Test.OData.QueryComposition.SingleResultPremiumCustomer/Bonuses&$expand=WebStack.QA.Test.OData.QueryComposition.SingleResultPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata"));
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
            JArray bonuses = result["Bonuses"] as JArray;
            Assert.Equal((int)result["Id"], bonuses.Count);
            foreach (JObject bonus in bonuses)
            {
                Assert.Equal(2, bonus.Properties().Count());
            }
        }

    }

    public class SingleResultCustomersController : ODataController
    {
        public IList<SingleResultCustomer> Customers { get; set; }

        public SingleResultCustomersController()
        {
            Customers = Enumerable.Range(0, 10).Select(i => new SingleResultCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                SingleResultOrders = Enumerable.Range(0, i).Select(j => new SingleResultOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new SingleResultAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("Country {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new SingleResultOrderDetail
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
        public SingleResult<SingleResultCustomer> Get(int key)
        {
            return new SingleResult<SingleResultCustomer>(Enumerable.Range(0, 10).Select(i => new SingleResultPremiumCustomer
                {
                    Id = i,
                    Name = string.Format("Customer{0}", i),
                    Category = string.Format("Category{0}", i),
                    Bonuses = Enumerable.Range(0, i).Select(j => new SingleResultBonus
                    {
                        Id = j,
                        Ammount = j * 1000
                    }).ToList(),
                    SingleResultOrders = Enumerable.Range(0, i).Select(j => new SingleResultOrder
                    {
                        Id = j,
                        Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                        BillingAddress = new SingleResultAddress
                        {
                            FirstLine = string.Format("First line {0}", j),
                            SecondLine = string.Format("Second line {0}", j),
                            ZipCode = j * 100,
                            City = string.Format("City {0}", j),
                            State = string.Format("State {0}", j),
                            Country = string.Format("Country {0}", j),
                        },
                        OrderDetails = new List<SingleResultOrderDetail>()
                    }).ToList(),

                }).Concat(Enumerable.Range(10, 10).Select(i => new SingleResultPremiumCustomer
                {
                    Id = i,
                    Name = string.Format("Customer{0}", i),
                    Category = string.Format("Category{0}", i),
                    Bonuses = Enumerable.Range(0, i).Select(j => new SingleResultBonus
                    {
                        Id = j,
                        Ammount = j * 1000
                    }).ToList(),
                    SingleResultOrders = Enumerable.Range(0, i).Select(j => new SingleResultOrder
                    {
                        Id = j,
                        Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                        BillingAddress = new SingleResultAddress
                        {
                            FirstLine = string.Format("First line {0}", j),
                            SecondLine = string.Format("Second line {0}", j),
                            ZipCode = j * 100,
                            City = string.Format("City {0}", j),
                            State = string.Format("State {0}", j),
                            Country = string.Format("Country {0}", j),
                        },
                        OrderDetails = new List<SingleResultOrderDetail>()
                    }).ToList(),

                })).AsQueryable().Where(x => x.Id == key));
        }

    }

    public class SingleResultCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<SingleResultOrder> SingleResultOrders { get; set; }
    }

    public class SingleResultPremiumCustomer : SingleResultCustomer
    {
        public string Category { get; set; }
        public IList<SingleResultBonus> Bonuses { get; set; }
    }

    public class SingleResultBonus
    {
        public int Id { get; set; }
        public double Ammount { get; set; }
    }

    public class SingleResultOrder
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public SingleResultAddress BillingAddress { get; set; }
        public virtual IList<SingleResultOrderDetail> OrderDetails { get; set; }
    }

    public class SingleResultAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class SingleResultOrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Ammount { get; set; }
        public double Price { get; set; }
    }
}
