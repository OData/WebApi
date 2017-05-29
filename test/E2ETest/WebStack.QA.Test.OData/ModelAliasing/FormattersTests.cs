using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using ModelAliasing;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ModelAliasing
{
    [NuwaFramework]
    public class FormattersTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("convention", "convention", GetConventionModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ModelAliasingMetadataCustomer> customers = builder.EntitySet<ModelAliasingMetadataCustomer>("ModelAliasingCustomers");
            customers.EntityType.Name = "Customer";
            customers.EntityType.Namespace = "ModelAliasing";
            customers.EntityType.ComplexProperty(c => c.BillingAddress).Name = "FinancialAddress";
            customers.EntityType.Property(c => c.Name).Name = "ClientName";
            customers.EntityType.HasMany(c => c.Orders).Name = "Purchases";
            EntitySetConfiguration<ModelAliasingMetadataOrder> orders = builder.EntitySet<ModelAliasingMetadataOrder>("Orders");
            orders.EntityType.Name = "Order";
            orders.EntityType.Namespace = "AliasedNamespace";
            EntityTypeConfiguration<ModelAliasingMetadataExpressOrder> expressOrder = builder.EntityType<ModelAliasingMetadataExpressOrder>().DerivesFrom<ModelAliasingMetadataOrder>();
            expressOrder.Name = "ExpressOrder";
            expressOrder.Namespace = "Purchasing";
            expressOrder.Property(eo => eo.ExpressFee).Name = "Fee";
            EntityTypeConfiguration<ModelAliasingMetadataFreeDeliveryOrder> freeDeliveryOrder = builder.EntityType<ModelAliasingMetadataFreeDeliveryOrder>().DerivesFrom<ModelAliasingMetadataOrder>();
            freeDeliveryOrder.Name = "FreeDeliveryOrder";
            freeDeliveryOrder.Namespace = "Purchasing";
            EntitySetConfiguration<ModelAliasingMetadataProduct> products = builder.EntitySet<ModelAliasingMetadataProduct>("Products");
            EntitySetConfiguration<ModelAliasingMetadataOrderLine> ordersLines = builder.EntitySet<ModelAliasingMetadataOrderLine>("OrdersLines");
            ordersLines.EntityType.Property(ol => ol.Price).Name = "Cost";
            ComplexTypeConfiguration<ModelAliasingMetadataRegion> region = builder.ComplexType<ModelAliasingMetadataRegion>();
            region.Name = "PoliticalRegion";
            region.Namespace = "Location";

            ComplexTypeConfiguration<ModelAliasingMetadataAddress> address = builder.ComplexType<ModelAliasingMetadataAddress>();
            address.Name = "Direction";
            address.Namespace = "Location";
            address.ComplexProperty<ModelAliasingMetadataRegion>(c => c.Country).Name = "Reign";
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.streaming=false")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        public void CanReadRenamedPayloads(string acceptHeader)
        {
            Customer customer = new Customer();
            customer.Id = 3;
            customer.ClientName = "Convention Name 3";
            customer.FinancialAddress = new Location.Direction
            {
                FirstLine = "Convention First Line 3",
                SecondLine = "Convention Second Line 3",
                City = "Convention City",
                ZipCode = 333,
                Reign = new Location.PoliticalRegion
                {
                    Country = "Convention country",
                    State = "Convention state"
                }
            };

            customer.DefaultShippingAddress = new Location.Direction
            {
                FirstLine = "DefaultShippingAddress Convention First Line 3",
                SecondLine = "DefaultShippingAddress Convention Second Line 3",
                City = "DefaultShippingAddress Convention City",
                ZipCode = 3333,
                Reign = new Location.PoliticalRegion
                {
                    Country = "DefaultShippingAddress Convention country",
                    State = "DefaultShippingAddress Convention state"
                }
            };

            customer.Purchases = new Collection<AliasedNamespace.Order>{
                new Purchasing.ExpressOrder{
                    Id = 1,
                    DeliveryDate = new DateTime(2013,1,12),
                    PurchaseDate = new DateTime(2013,10,31),
                    ShippingAddress = new Location.Direction
                    {
                        FirstLine = "ShippingAddress Convention First Line 3",
                        SecondLine = "ShippingAddress Convention Second Line 3",
                        City = "ShippingAddress Convention City",
                        ZipCode = 3333,
                        Reign = new Location.PoliticalRegion
                        {
                            Country = "ShippingAddress Convention country",
                            State = "ShippingAddress Convention state"
                        }
                    },
                    Details = new Collection<Billing.OrderLine>{
                        new Billing.OrderLine{
                            Id = 1,
                            Ammount = 1,
                            Product = new Purchasing.Product
                            {
                                ProductName = "Product 1",
                                Id = 1,
                                AvailableRegions = new Collection<Location.PoliticalRegion>{
                                    new Location.PoliticalRegion
                                    {
                                        Country = "Country 1",
                                        State = "State 1"
                                    },
                                    new Location.PoliticalRegion{
                                        Country = "Country 2",
                                        State = "State 2"
                                    }
                                }
                            },
                            Cost = 1
                        },
                        new Billing.OrderLine{
                            Id = 2,
                            Ammount = 2,
                            Cost = 2,
                            Product = new Purchasing.Product
                            {
                                ProductName = "Product 1",
                                Id = 1,
                                AvailableRegions = new Collection<Location.PoliticalRegion>{
                                    new Location.PoliticalRegion
                                    {
                                        Country = "Country 1",
                                        State = "State 1"
                                    },
                                    new Location.PoliticalRegion{
                                        Country = "Country 2",
                                        State = "State 2"
                                    }
                                }                            
                            }
                        },
                        new Billing.OrderLine{
                            Id = 3,
                            Ammount = 3,
                            Cost = 3,
                            Product = new Purchasing.Product
                            {
                                ProductName = "Product 1",
                                Id = 1,
                                AvailableRegions = new Collection<Location.PoliticalRegion>{
                                    new Location.PoliticalRegion
                                    {
                                        Country = "Country 1",
                                        State = "State 1"
                                    },
                                    new Location.PoliticalRegion{
                                        Country = "Country 2",
                                        State = "State 2"
                                    }
                                }
                            }
                        }
                    }
                },
                new Purchasing.FreeDeliveryOrder{
                    Id = 2,
                    PurchaseDate = new DateTime(2013,10,30),
                    ShippingAddress = new Location.Direction
                    {
                        FirstLine = "ShippingAddress Convention First Line 3",
                        SecondLine = "ShippingAddress Convention Second Line 3",
                        City = "ShippingAddress Convention City",
                        ZipCode = 3333,
                        Reign = new Location.PoliticalRegion
                        {
                            Country = "ShippingAddress Convention country",
                            State = "ShippingAddress Convention state"
                        }
                    },
                    Details = new Collection<Billing.OrderLine>()
                }
            };
            dynamic jsonCustomer = JObject.FromObject(customer);
            jsonCustomer.Purchases[0]["@odata.type"] = "Purchasing.ExpressOrder";
            jsonCustomer.Purchases[1]["@odata.type"] = "Purchasing.FreeDeliveryOrder";
            HttpRequestMessage postRequest = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/convention/ModelAliasingCustomers/");
            postRequest.Content = new StringContent(jsonCustomer.ToString());
            postRequest.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            postRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage postResponse = Client.SendAsync(postRequest).Result;
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/convention/ModelAliasingCustomers(3)?$expand=Purchases($expand=Details($expand=Product))");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JToken queriedCustomer = JToken.Parse(response.Content.ReadAsStringAsync().Result);
            if (acceptHeader.Contains("application/json;odata.metadata=none"))
            {
                Assert.Equal(JObject.FromObject(customer), queriedCustomer, JObject.EqualityComparer);
            }
        }
    }

    public class ModelAliasingCustomersController : ODataController
    {
        private static ModelAliasingMetadataCustomer customer = new ModelAliasingMetadataCustomer();
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 4)]
        public IHttpActionResult Get([FromODataUri]int key)
        {
            return Ok(customer);
        }

        public IHttpActionResult Post([FromBody] ModelAliasingMetadataCustomer entity)
        {
            customer = entity;
            return Created(customer);
        }
    }
}
