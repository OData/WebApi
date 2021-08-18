//-----------------------------------------------------------------------------
// <copyright file="FormattersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using ModelAliasing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelAliasing
{
    public class FormattersTests : WebHostTestBase
    {
        public FormattersTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("convention", "convention", GetConventionModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetConventionModel(WebRouteConfiguration config)
        {
            ODataConventionModelBuilder builder = config.CreateConventionModelBuilder();
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
            address.ComplexProperty<ModelAliasingMetadataRegion>(c => c.CountryOrRegion).Name = "Reign";
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
        public async Task CanReadRenamedPayloads(string acceptHeader)
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
                    CountryOrRegion = "Convention country",
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
                    CountryOrRegion = "DefaultShippingAddress Convention country",
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
                            CountryOrRegion = "ShippingAddress Convention country",
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
                                        CountryOrRegion = "CountryOrRegion 1",
                                        State = "State 1"
                                    },
                                    new Location.PoliticalRegion{
                                        CountryOrRegion = "CountryOrRegion 2",
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
                                        CountryOrRegion = "CountryOrRegion 1",
                                        State = "State 1"
                                    },
                                    new Location.PoliticalRegion{
                                        CountryOrRegion = "CountryOrRegion 2",
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
                                        CountryOrRegion = "CountryOrRegion 1",
                                        State = "State 1"
                                    },
                                    new Location.PoliticalRegion{
                                        CountryOrRegion = "CountryOrRegion 2",
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
                            CountryOrRegion = "ShippingAddress Convention country",
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
            HttpResponseMessage postResponse = await Client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/convention/ModelAliasingCustomers(3)?$expand=Purchases($expand=Details($expand=Product))");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage response = await Client.SendAsync(request);
            JToken queriedCustomer = JToken.Parse(await response.Content.ReadAsStringAsync());
            if (acceptHeader.Contains("application/json;odata.metadata=none"))
            {
                Assert.Equal(JObject.FromObject(customer), queriedCustomer, JObject.EqualityComparer);
            }
        }
    }

    public class ModelAliasingCustomersController : TestODataController
    {
        private static ModelAliasingMetadataCustomer customer = new ModelAliasingMetadataCustomer();
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 4)]
        public ITestActionResult Get([FromODataUri]int key)
        {
            return Ok(customer);
        }

        public ITestActionResult Post([FromBody] ModelAliasingMetadataCustomer entity)
        {
            customer = entity;
            return Created(customer);
        }
    }
}
