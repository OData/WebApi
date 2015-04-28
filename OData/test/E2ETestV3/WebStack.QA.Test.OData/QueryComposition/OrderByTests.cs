using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    [NuwaFramework]
    public class OrderByTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<OrderByCustomer> customers = builder.EntitySet<OrderByCustomer>("OrderByCustomers");
            EntitySetConfiguration<OrderByOrder> orders = builder.EntitySet<OrderByOrder>("OrderByOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        public void CanOrderByNestedPropertiesOnComplexObjects()
        {
            string query = "/odata/OrderByCustomers?$orderby=Address/ZipCode desc";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic parsedContent = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(parsedContent.value);
            for (int i = 1; i < parsedContent.value.Count; i++)
            {
                dynamic previousElement = parsedContent.value[i - 1];
                dynamic currentElement = parsedContent.value[i];
                Assert.Equal(1, previousElement.Address.ZipCode.CompareTo(currentElement.Address.ZipCode));
            }
        }

        [Fact]
        public void CanOrderByMultipleNestedPropertiesOnComplexObjects()
        {
            string query = "/odata/OrderByCustomers?$orderby=Address/Country/Name asc, Address/ZipCode asc";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic parsedContent = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(parsedContent.value);
            for (int i = 1; i < parsedContent.value.Count; i++)
            {
                dynamic previousElement = parsedContent.value[i - 1];
                dynamic currentElement = parsedContent.value[i];
                Assert.True(previousElement.Address.Country.Name.Equals(currentElement.Address.Country.Name) && 1 > previousElement.Address.ZipCode.CompareTo(currentElement.Address.ZipCode)
                            || previousElement.Address.Country.Name.CompareTo(currentElement.Address.Country.Name) < 1);
            }
        }
    }


    public class OrderByCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            int max = 10;
            return Ok(from int i in Enumerable.Range(0, max)
                      let j = max - i
                      select new OrderByCustomer
                      {
                          Id = j,
                          Name = "Customer " + i,
                          Address = new OrderByAddress
                          {
                              FirstLine = "FirstLine " + j,
                              SecondLine = "SecondLine " + i,
                              ZipCode = (13 * 7 * j).ToString(),
                              Country = new OrderByCountry
                              {
                                  Name = "Country " + j % 2,
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
                                            Country = new OrderByCountry
                                            {
                                                Name = "Country " + k,
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
        public OrderByAddress Address { get; set; }
        public IList<OrderByOrder> Orders { get; set; }
    }

    public class OrderByAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public string ZipCode { get; set; }
        public OrderByCountry Country { get; set; }
    }

    public class OrderByCountry
    {
        public string Name { get; set; }
        public string State { get; set; }
    }

    public class OrderByOrder
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; }
        public OrderByAddress ShippingAddress { get; set; }
        public bool IsAGift { get; set; }
    }
}
