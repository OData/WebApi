using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WebStack.QA.Test.OData.ModelAliasing
{
    [NuwaFramework]
    public class QueryTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("convention", "convention", GetConventionModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ModelAliasingMetadataCustomer> customers = builder.EntitySet<ModelAliasingMetadataCustomer>("ModelAliasingQueryCustomers");
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

        [Fact]
        public void QueriesWorkOnAliasedModels()
        {
            IEnumerable<Customer> customers = Enumerable.Range(0, 10).Select(i => new Customer
            {
                Id = i,
                ClientName = "Customer Name " + i,
                FinancialAddress = new Location.Direction
                {
                    FirstLine = "Billing First line" + i,
                    SecondLine = "Billing Second line " + i,
                    ZipCode = i * 5,
                    City = "Billing City " + i,
                    Reign = new Location.PoliticalRegion
                    {
                        Country = "Billing Region Country" + i,
                        State = "Billing Region State" + i
                    }
                },
                DefaultShippingAddress = new Location.Direction
                {
                    FirstLine = "DefaultShipping First line" + i,
                    SecondLine = "DefaultShipping Second line " + i,
                    ZipCode = i * 5,
                    City = "DefaultShipping City " + i,
                    Reign = new Location.PoliticalRegion
                    {
                        Country = "DefaultShipping Region Country" + i,
                        State = "DefaultShipping Region State" + i
                    }
                },
                Purchases = new System.Collections.ObjectModel.Collection<AliasedNamespace.Order>(Enumerable.Range(0, i).Select<int, AliasedNamespace.Order>(j =>
                    {
                        switch (j % 3)
                        {
                            case 0:
                                return new AliasedNamespace.Order
                                {
                                    Id = i * 10 + j,
                                    PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    ShippingAddress = new Location.Direction
                                    {
                                        FirstLine = "Order Shipping First line" + i,
                                        SecondLine = "Order Shipping Second line " + i,
                                        ZipCode = i * 5,
                                        City = "Order Shipping City " + i,
                                        Reign = new Location.PoliticalRegion
                                        {
                                            Country = "Order Shipping Region Country" + i,
                                            State = "Order Shipping Region State" + i
                                        }
                                    },
                                    Details = new System.Collections.ObjectModel.Collection<Billing.OrderLine>(Enumerable.Range(0, j).Select(k => new Billing.OrderLine
                                    {
                                        Id = i * 100 + j * 10 + k,
                                        Ammount = i * 100 + j * 10 + k,
                                        Cost = i * 100 + j * 10 + k,
                                        Product = new Purchasing.Product
                                        {
                                            Id = i * 100 + j * 10 + k,
                                            ProductName = "Product " + i * 100 + j * 10 + k,
                                            AvailableRegions = new System.Collections.ObjectModel.Collection<Location.PoliticalRegion>(Enumerable.Range(0, k).Select(l => new Location.PoliticalRegion
                                            {
                                                Country = "Product Country " + 1000 * i + 100 * j + 10 * k + l,
                                                State = "Product State " + 1000 * i + 100 * j + 10 * k + l
                                            }).ToList())
                                        }
                                    }).ToList())
                                };
                            case 1:
                                return new Purchasing.ExpressOrder
                                {
                                    Id = i * 10 + j,
                                    PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    ShippingAddress = new Location.Direction
                                    {
                                        FirstLine = "Order Shipping First line" + i,
                                        SecondLine = "Order Shipping Second line " + i,
                                        ZipCode = i * 5,
                                        City = "Order Shipping City " + i,
                                        Reign = new Location.PoliticalRegion
                                        {
                                            Country = "Order Shipping Region Country" + i,
                                            State = "Order Shipping Region State" + i
                                        }
                                    },
                                    DeliveryDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    Details = new System.Collections.ObjectModel.Collection<Billing.OrderLine>(Enumerable.Range(0, j).Select(k => new Billing.OrderLine
                                    {
                                        Id = i * 100 + j * 10 + k,
                                        Ammount = i * 100 + j * 10 + k,
                                        Cost = i * 100 + j * 10 + k,
                                        Product = new Purchasing.Product
                                        {
                                            Id = i * 100 + j * 10 + k,
                                            ProductName = "Product " + i * 100 + j * 10 + k,
                                            AvailableRegions = new System.Collections.ObjectModel.Collection<Location.PoliticalRegion>(Enumerable.Range(0, k).Select(l => new Location.PoliticalRegion
                                            {
                                                Country = "Product Country " + 1000 * i + 100 * j + 10 * k + l,
                                                State = "Product State " + 1000 * i + 100 * j + 10 * k + l
                                            }).ToList())
                                        }
                                    }).ToList())

                                };
                            case 2:
                                return new Purchasing.FreeDeliveryOrder
                                {
                                    Id = i * 10 + j,
                                    PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    ShippingAddress = new Location.Direction
                                    {
                                        FirstLine = "Order Shipping First line" + i,
                                        SecondLine = "Order Shipping Second line " + i,
                                        ZipCode = i * 5,
                                        City = "Order Shipping City " + i,
                                        Reign = new Location.PoliticalRegion
                                        {
                                            Country = "Order Shipping Region Country" + i,
                                            State = "Order Shipping Region State" + i
                                        }
                                    },

                                    Details = new System.Collections.ObjectModel.Collection<Billing.OrderLine>(Enumerable.Range(0, j).Select(k => new Billing.OrderLine
                                    {
                                        Id = i * 100 + j * 10 + k,
                                        Ammount = i * 100 + j * 10 + k,
                                        Cost = i * 100 + j * 10 + k,
                                        Product = new Purchasing.Product
                                        {
                                            Id = i * 100 + j * 10 + k,
                                            ProductName = "Product " + i * 100 + j * 10 + k,
                                            AvailableRegions = new System.Collections.ObjectModel.Collection<Location.PoliticalRegion>(Enumerable.Range(0, k).Select(l => new Location.PoliticalRegion
                                            {
                                                Country = "Product Country " + 1000 * i + 100 * j + 10 * k + l,
                                                State = "Product State " + 1000 * i + 100 * j + 10 * k + l
                                            }).ToList())
                                        }
                                    }).ToList())
                                };
                            default:
                                throw new ArgumentOutOfRangeException("j");
                        }
                    }).ToList())
            });
            string expand = "$expand=Purchases($select=Details;$expand=Details($select=Product;$expand=Product($select=AvailableRegions)))";
            string filter = "$filter=FinancialAddress/ZipCode le 30 and startswith(FinancialAddress/Reign/Country,'Billing Region Country')";
            string orderBy = "$orderby=DefaultShippingAddress/Reign/State desc, FinancialAddress/ZipCode asc";
            string select = "$select=Id";
            string query = string.Format("?{0}&{1}&{2}&{3}", expand, filter, orderBy, select);

            customers = customers.Where(c => c.FinancialAddress.ZipCode <= 30 && c.FinancialAddress.Reign.Country.StartsWith("Billing Region Country"))
                                 .OrderByDescending(c => c.DefaultShippingAddress.Reign.State).ThenBy(c => c.FinancialAddress.ZipCode);
            var projectedCustomers = customers.Select(c =>
                new
                {
                    Purchases = c.Purchases.Select(p => new
                    {
                        Details = p.Details.Select(d => new
                        {
                            Product = new { AvailableRegions = d.Product.AvailableRegions }
                        })
                    }),
                    Id = c.Id
                });
            dynamic jsonCustomers = JToken.FromObject(projectedCustomers);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/convention/ModelAliasingQueryCustomers" + query);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage response = Client.SendAsync(request).Result;
            dynamic queriedObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(jsonCustomers, queriedObject.value, JToken.EqualityComparer);
        }
    }

    public class ModelAliasingQueryCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 10).Select(i => new ModelAliasingMetadataCustomer
            {
                Id = i,
                Name = "Customer Name " + i,
                BillingAddress = new ModelAliasingMetadataAddress
                {
                    FirstLine = "Billing First line" + i,
                    SecondLine = "Billing Second line " + i,
                    ZipCode = i * 5,
                    City = "Billing City " + i,
                    Country = new ModelAliasingMetadataRegion
                    {
                        Country = "Billing Region Country" + i,
                        State = "Billing Region State" + i
                    }
                },
                DefaultShippingAddress = new ModelAliasingMetadataAddress
                {
                    FirstLine = "DefaultShipping First line" + i,
                    SecondLine = "DefaultShipping Second line " + i,
                    ZipCode = i * 5,
                    City = "DefaultShipping City " + i,
                    Country = new ModelAliasingMetadataRegion
                    {
                        Country = "DefaultShipping Region Country" + i,
                        State = "DefaultShipping Region State" + i
                    }
                },
                Orders = Enumerable.Range(0, i).Select(j =>
                    {
                        switch (j % 3)
                        {
                            case 0:
                                return new ModelAliasingMetadataOrder
                                {
                                    Id = i * 10 + j,
                                    PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    ShippingAddress = new ModelAliasingMetadataAddress
                                    {
                                        FirstLine = "Order Shipping First line" + i,
                                        SecondLine = "Order Shipping Second line " + i,
                                        ZipCode = i * 5,
                                        City = "Order Shipping City " + i,
                                        Country = new ModelAliasingMetadataRegion
                                        {
                                            Country = "Order Shipping Region Country" + i,
                                            State = "Order Shipping Region State" + i
                                        }
                                    },
                                    Details = Enumerable.Range(0, j).Select(k => new ModelAliasingMetadataOrderLine
                                    {
                                        Id = i * 100 + j * 10 + k,
                                        Ammount = i * 100 + j * 10 + k,
                                        Price = i * 100 + j * 10 + k,
                                        Item = new ModelAliasingMetadataProduct
                                        {
                                            Id = i * 100 + j * 10 + k,
                                            Name = "Product " + i * 100 + j * 10 + k,
                                            Regions = Enumerable.Range(0, k).Select(l => new ModelAliasingMetadataRegion
                                            {
                                                Country = "Product Country " + 1000 * i + 100 * j + 10 * k + l,
                                                State = "Product State " + 1000 * i + 100 * j + 10 * k + l
                                            }).ToList()
                                        }
                                    }).ToList()
                                };
                            case 1:
                                return new ModelAliasingMetadataExpressOrder
                                {
                                    Id = i * 10 + j,
                                    PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    ShippingAddress = new ModelAliasingMetadataAddress
                                    {
                                        FirstLine = "Order Shipping First line" + i,
                                        SecondLine = "Order Shipping Second line " + i,
                                        ZipCode = i * 5,
                                        City = "Order Shipping City " + i,
                                        Country = new ModelAliasingMetadataRegion
                                        {
                                            Country = "Order Shipping Region Country" + i,
                                            State = "Order Shipping Region State" + i
                                        }
                                    },
                                    ExpressFee = i * 10 + j,
                                    GuaranteedDeliveryDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    Details = Enumerable.Range(0, j).Select(k => new ModelAliasingMetadataOrderLine
                                    {
                                        Id = i * 100 + j * 10 + k,
                                        Ammount = i * 100 + j * 10 + k,
                                        Price = i * 100 + j * 10 + k,
                                        Item = new ModelAliasingMetadataProduct
                                        {
                                            Id = i * 100 + j * 10 + k,
                                            Name = "Product " + i * 100 + j * 10 + k,
                                            Regions = Enumerable.Range(0, k).Select(l => new ModelAliasingMetadataRegion
                                            {
                                                Country = "Product Country " + 1000 * i + 100 * j + 10 * k + l,
                                                State = "Product State " + 1000 * i + 100 * j + 10 * k + l
                                            }).ToList()
                                        }
                                    }).ToList()

                                };
                            case 2:
                                return new ModelAliasingMetadataFreeDeliveryOrder
                                {
                                    Id = i * 10 + j,
                                    PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    ShippingAddress = new ModelAliasingMetadataAddress
                                    {
                                        FirstLine = "Order Shipping First line" + i,
                                        SecondLine = "Order Shipping Second line " + i,
                                        ZipCode = i * 5,
                                        City = "Order Shipping City " + i,
                                        Country = new ModelAliasingMetadataRegion
                                        {
                                            Country = "Order Shipping Region Country" + i,
                                            State = "Order Shipping Region State" + i
                                        }
                                    },
                                    EstimatedDeliveryDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                                    Details = Enumerable.Range(0, j).Select(k => new ModelAliasingMetadataOrderLine
                                    {
                                        Id = i * 100 + j * 10 + k,
                                        Ammount = i * 100 + j * 10 + k,
                                        Price = i * 100 + j * 10 + k,
                                        Item = new ModelAliasingMetadataProduct
                                        {
                                            Id = i * 100 + j * 10 + k,
                                            Name = "Product " + i * 100 + j * 10 + k,
                                            Regions = Enumerable.Range(0, k).Select(l => new ModelAliasingMetadataRegion
                                            {
                                                Country = "Product Country " + 1000 * i + 100 * j + 10 * k + l,
                                                State = "Product State " + 1000 * i + 100 * j + 10 * k + l
                                            }).ToList()
                                        }
                                    }).ToList()
                                };
                            default:
                                throw new ArgumentOutOfRangeException("j");
                        }
                    }).ToList()
            }));
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            return Ok();
        }
    }
}
