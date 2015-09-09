using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.ModelAliasing
{
    [NuwaFramework]
    public class ModelBuildersSwaggerTests
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
            config.MapODataServiceRoute("explicit", "explicit", GetExplicitModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ModelAliasingMetadataCustomer> customers = builder.EntitySet<ModelAliasingMetadataCustomer>("Customers");
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

        private static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            EntitySetConfiguration<ModelAliasingMetadataCustomer> customers = builder.EntitySet<ModelAliasingMetadataCustomer>("Customers");
            customers.EntityType.HasKey(c => c.Id);
            customers.EntityType.Property(c => c.Name);
            customers.EntityType.ComplexProperty(c => c.BillingAddress);
            customers.EntityType.ComplexProperty(c => c.DefaultShippingAddress);
            customers.EntityType.HasMany(c => c.Orders);

            EntityTypeConfiguration<ModelAliasingMetadataExpressOrder> expressOrder = builder.EntityType<ModelAliasingMetadataExpressOrder>().DerivesFrom<ModelAliasingMetadataOrder>();
            expressOrder.Property(eo => eo.ExpressFee);
            expressOrder.Property(eo => eo.GuaranteedDeliveryDate);

            EntityTypeConfiguration<ModelAliasingMetadataFreeDeliveryOrder> freeDeliveryOrder = builder.EntityType<ModelAliasingMetadataFreeDeliveryOrder>().DerivesFrom<ModelAliasingMetadataOrder>();
            freeDeliveryOrder.Property(fdo => fdo.EstimatedDeliveryDate);

            EntitySetConfiguration<ModelAliasingMetadataOrder> orders = builder.EntitySet<ModelAliasingMetadataOrder>("Orders");
            orders.EntityType.HasKey(o => o.Id);
            orders.EntityType.Property(o => o.PurchaseDate);
            orders.EntityType.ComplexProperty(o => o.ShippingAddress);
            orders.EntityType.HasMany(o => o.Details);
            EntitySetConfiguration<ModelAliasingMetadataOrderLine> ordersLines = builder.EntitySet<ModelAliasingMetadataOrderLine>("OrdersLines");
            ordersLines.EntityType.HasKey(ol => ol.Id);
            ordersLines.EntityType.HasOptional(ol => ol.Item);
            ordersLines.EntityType.Property(ol => ol.Price);
            ordersLines.EntityType.Property(ol => ol.Ammount);
            ComplexTypeConfiguration<ModelAliasingMetadataAddress> address = builder.ComplexType<ModelAliasingMetadataAddress>();
            address.Property(a => a.FirstLine);
            address.Property(a => a.SecondLine);
            address.Property(a => a.ZipCode);
            address.Property(a => a.City);
            address.ComplexProperty(a => a.Country);
            ComplexTypeConfiguration<ModelAliasingMetadataRegion> region = builder.ComplexType<ModelAliasingMetadataRegion>();
            region.Property(r => r.Country);
            region.Property(r => r.State);

            EntitySetConfiguration<ModelAliasingMetadataProduct> products = builder.EntitySet<ModelAliasingMetadataProduct>("Products");
            products.EntityType.HasKey(p => p.Id);
            products.EntityType.Property(p => p.Name);
            products.EntityType.CollectionProperty(p => p.Regions);

            customers.EntityType.Name = "Customer";
            customers.EntityType.Namespace = "ModelAliasing";
            customers.EntityType.ComplexProperty(c => c.BillingAddress).Name = "FinancialAddress";
            customers.EntityType.Property(c => c.Name).Name = "ClientName";
            customers.EntityType.HasMany(c => c.Orders).Name = "Purchases";

            orders.EntityType.Name = "Order";
            orders.EntityType.Namespace = "AliasedNamespace";

            expressOrder.Name = "ExpressOrder";
            expressOrder.Namespace = "Purchasing";
            expressOrder.Property(eo => eo.ExpressFee).Name = "Fee";

            freeDeliveryOrder.Name = "FreeOrder";
            freeDeliveryOrder.Namespace = "Purchasing";

            ordersLines.EntityType.Property(ol => ol.Price).Name = "Cost";
            region.Name = "PoliticalRegion";
            region.Namespace = "Location";

            address.Name = "Direction";
            address.Namespace = "Location";
            address.ComplexProperty<ModelAliasingMetadataRegion>(c => c.Country).Name = "Reign";
            return builder.GetEdmModel();
        }

        [Fact]
        public void CanGetPayload()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/convention/$swagger");
            HttpResponseMessage response = Client.SendAsync(request).Result;

            Assert.NotNull(response.Content);
        }
    }
}
