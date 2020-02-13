// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class OrderByTests : WebHostTestBase<OrderByTests>
    {
        public OrderByTests(WebHostTestFixture<OrderByTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Count().Filter().OrderBy().Expand().MaxTop(null);
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<OrderByCustomer> customers = builder.EntitySet<OrderByCustomer>("OrderByCustomers");
            EntitySetConfiguration<OrderByOrder> orders = builder.EntitySet<OrderByOrder>("OrderByOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanOrderByNestedPropertiesOnComplexObjects()
        {
            string query = "/odata/OrderByCustomers?$orderby=Address/ZipCode desc";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic parsedContent = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(parsedContent.value);
            for (int i = 1; i < parsedContent.value.Count; i++)
            {
                dynamic previousElement = parsedContent.value[i - 1];
                dynamic currentElement = parsedContent.value[i];
                Assert.Equal(1, previousElement.Address.ZipCode.CompareTo(currentElement.Address.ZipCode));
            }
        }

        [Fact]
        public async Task CanOrderByMultipleNestedPropertiesOnComplexObjects()
        {
            string query = "/odata/OrderByCustomers?$orderby=Address/CountryOrRegion/Name asc, Address/ZipCode asc";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic parsedContent = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(parsedContent.value);
            for (int i = 1; i < parsedContent.value.Count; i++)
            {
                dynamic previousElement = parsedContent.value[i - 1];
                dynamic currentElement = parsedContent.value[i];
                Assert.True(previousElement.Address.CountryOrRegion.Name.Equals(currentElement.Address.CountryOrRegion.Name) && 1 > previousElement.Address.ZipCode.CompareTo(currentElement.Address.ZipCode)
                            || previousElement.Address.CountryOrRegion.Name.CompareTo(currentElement.Address.CountryOrRegion.Name) < 1);
            }
        }

        [Fact]
        public async Task CanOrderByDuplicatePropertiesSimiliarPath()
        {
            string query =
                "/odata/OrderByCustomers?$orderby=CountryOrRegion/Name, Address/CountryOrRegion/Name asc, WorkAddress/CountryOrRegion/Name asc";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic parsedContent = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(parsedContent.value);
            for (int i = 1; i < parsedContent.value.Count; i++)
            {
                dynamic previousElement = parsedContent.value[i - 1];
                dynamic currentElement = parsedContent.value[i];
                Assert.True(previousElement.CountryOrRegion.Name.CompareTo(currentElement.CountryOrRegion.Name) < 1
                            ||
                            previousElement.CountryOrRegion.Name.Equals(currentElement.CountryOrRegion.Name) &&
                            previousElement.Address.CountryOrRegion.Name.CompareTo(currentElement.Address.CountryOrRegion.Name) < 1
                            ||
                            previousElement.CountryOrRegion.Name.Equals(currentElement.CountryOrRegion.Name) &&
                            previousElement.Address.CountryOrRegion.Name.Equals(currentElement.Address.CountryOrRegion.Name) &&
                            previousElement.WorkAddress.CountryOrRegion.Name.CompareTo(currentElement.WorkAddress.CountryOrRegion.Name) < 1);
            }
        }
    }

    public class OrderByCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public ITestActionResult Get()
        {
            int max = 10;
            return Ok(from int i in Enumerable.Range(0, max)
                      let j = max - i
                      select new OrderByCustomer
                      {
                          Id = j,
                          Name = "Customer " + i,
                          CountryOrRegion = new OrderByCountryOrRegion
                          {
                              Name = "CountryOrRegion " + j % 3,
                              State = "State " + j
                          },
                          Address = new OrderByAddress
                          {
                              FirstLine = "FirstLine " + j,
                              SecondLine = "SecondLine " + i,
                              ZipCode = (13 * 7 * j).ToString(),
                              CountryOrRegion = new OrderByCountryOrRegion
                              {
                                  Name = "CountryOrRegion " + j % 2,
                                  State = "State " + j
                              }
                          },
                          WorkAddress = new OrderByAddress
                          {
                              FirstLine = "FirstLine " + j,
                              SecondLine = "SecondLine " + i,
                              ZipCode = (13 * 7 * j).ToString(),
                              CountryOrRegion = new OrderByCountryOrRegion
                              {
                                  Name = "CountryOrRegion " + j,
                                  State = "State " + j
                              }
                          },
                          Orders = (from int k in Enumerable.Range(0, j)
                                    select new OrderByOrder
                                    {
                                        Id = k,
                                        PurchaseDate = DateTime.Now.Subtract(TimeSpan.FromDays(k)),
                                        ShippingAddress = new OrderByAddress
                                        {
                                            FirstLine = "FirstLine " + k,
                                            SecondLine = "SecondLine " + k,
                                            ZipCode = (13 * 7 * 5 * k).ToString(),
                                            CountryOrRegion = new OrderByCountryOrRegion
                                            {
                                                Name = "CountryOrRegion " + k,
                                                State = "State " + k
                                            }
                                        },
                                        IsAGift = k % 2 == 0
                                    }).ToList()
                      });
        }
    }

    public class OrderByCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public OrderByCountryOrRegion CountryOrRegion { get; set; }
        public OrderByAddress Address { get; set; }
        public OrderByAddress WorkAddress { get; set; }
        public IList<OrderByOrder> Orders { get; set; }
    }

    public class OrderByAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public string ZipCode { get; set; }
        public OrderByCountryOrRegion CountryOrRegion { get; set; }
    }

    public class OrderByCountryOrRegion
    {
        public string Name { get; set; }
        public string State { get; set; }
    }

    public class OrderByOrder
    {
        public int Id { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public OrderByAddress ShippingAddress { get; set; }
        public bool IsAGift { get; set; }
    }
}