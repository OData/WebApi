// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class SingleResultExpandTests : WebHostTestBase<SingleResultExpandTests>
    {
        public SingleResultExpandTests(WebHostTestFixture<SingleResultExpandTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("selectexpand", "selectexpand", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<SingleResultCustomer>("SingleResultCustomers")
                   .EntityType
                   .Action("CreditRating")
                   .Returns<double>();

            builder.EntitySet<SingleResultOrderDetail>("SingleResultOrderDetail");
            builder.EntityType<SingleResultPremiumCustomer>();
            builder.EntitySet<SingleResultOrder>("SingleResultOrder");
            builder.EntitySet<SingleResultBonus>("SingleResultBonus");

            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        [Fact]
        public async Task QueryJustThePropertiesOfTheEntriesOnAnEntry()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=*", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata=fullmetadata");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.True(json.Properties().All(p => p.Name != "#Container.CreditRating"));
        }

        [Fact]
        public async Task QueryJustTheSingleResultWithoutParameters()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=full");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
        }

        [Fact]
        public async Task QueryJustTheActionsOfTheEntriesOnAnEntry()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=Id,Default.*", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=full");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.Contains(json.Properties(), (p) => p.Name == "#Default.CreditRating");
        }

        [Fact]
        public async Task QueryASubsetOfThePropertiesOfAnEntryOnAnEntryQuery()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=Name", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.Single(json.Properties());
            Assert.Equal("Name", json.Properties().Single().Name);
        }

        [Fact]
        public async Task QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntry()
        {

            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)/?$select=Id,Name&$expand=SingleResultOrders($select=Id)", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.Equal(3, json.Properties().Count());
            Assert.True((int)json["Id"] == ((JArray)json["SingleResultOrders"]).Count);
        }

        [Fact]
        public async Task QueryASubSetOfThePropertiesPresentOnlyInADerivedEntryOnAnEntry()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(10)?" +
                "$select=Id,Microsoft.Test.E2E.AspNet.OData.QueryComposition.SingleResultPremiumCustomer/Category", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.Equal(2, json.Properties().Count());
        }

        [Fact]
        public async Task QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInlineForAnEntry()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)?$select=Id&$expand=SingleResultOrders", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.True(json.Properties().Count() == 2);

            JArray orders = json["SingleResultOrders"] as JArray;
            Assert.Equal((int)json["Id"], orders.Count);
            foreach (JObject order in (IEnumerable<JToken>)orders)
            {
                Assert.Equal(3, order.Properties().Count());
            }
        }

        [Fact]
        public async Task QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationPropertiesForAnEntry()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(10)?" +
                "$select=Id&" +
                "$expand=SingleResultOrders,Microsoft.Test.E2E.AspNet.OData.QueryComposition.SingleResultPremiumCustomer/Bonuses", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.True(json.Properties().Count() == 3);

            JArray orders = json["SingleResultOrders"] as JArray;
            Assert.Equal((int)json["Id"], orders.Count);
            foreach (JObject order in (IEnumerable<JToken>)orders)
            {
                Assert.Equal(3, order.Properties().Count());
            }

            JArray bonuses = json["Bonuses"] as JArray;
            Assert.Equal((int)json["Id"], bonuses.Count);
            foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
            {
                Assert.Equal(2, bonus.Properties().Count());
            }
        }

        [Fact]
        public async Task QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPath()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)?$select=Id,SingleResultOrders&$expand=SingleResultOrders($expand=OrderDetails)", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.True(json.Properties().Count() == 2);

            JArray orders = json["SingleResultOrders"] as JArray;
            Assert.Equal((int)json["Id"], orders.Count);
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

        [Fact]
        public async Task QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntriesOnAnEntry()
        {
            var queryUrl = string.Format("{0}/selectexpand/SingleResultCustomers(1)?" +
                "$select=Id&" +
                "$expand=Microsoft.Test.E2E.AspNet.OData.QueryComposition.SingleResultPremiumCustomer/Bonuses", BaseAddress);
            var response = await Client.GetWithAcceptAsync(queryUrl, "application/json;odata.metadata=none");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);
            Assert.True(json.Properties().Count() == 2);
            JArray bonuses = json["Bonuses"] as JArray;
            Assert.Equal((int)json["Id"], bonuses.Count);
            foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
            {
                Assert.Equal(2, bonus.Properties().Count());
            }
        }

    }

    public class SingleResultCustomersController : TestODataController
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
                        Country = string.Format("CountryOrRegion {0}", j),
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
        public TestSingleResult<SingleResultCustomer> Get(int key)
        {
            return new TestSingleResult<SingleResultCustomer>(Enumerable.Range(0, 10).Select(i => new SingleResultPremiumCustomer
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
                        Country = string.Format("CountryOrRegion {0}", j),
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
                        Country = string.Format("CountryOrRegion {0}", j),
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
        public DateTimeOffset Date { get; set; }
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
