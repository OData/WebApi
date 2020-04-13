// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelAliasing
{
    public class ModelBuildersMetadataTests : WebHostTestBase<ModelBuildersMetadataTests>
    {
        public ModelBuildersMetadataTests(WebHostTestFixture<ModelBuildersMetadataTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("convention", "convention", GetConventionModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            config.MapODataServiceRoute("explicit", "explicit", GetExplicitModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
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
            address.ComplexProperty<ModelAliasingMetadataRegion>(c => c.CountryOrRegion).Name = "Reign";
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
            address.ComplexProperty(a => a.CountryOrRegion);
            ComplexTypeConfiguration<ModelAliasingMetadataRegion> region = builder.ComplexType<ModelAliasingMetadataRegion>();
            region.Property(r => r.CountryOrRegion);
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
            address.ComplexProperty<ModelAliasingMetadataRegion>(c => c.CountryOrRegion).Name = "Reign";
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanRenameTypesAndNamespacesInConventionModelBuilder()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/convention/$metadata");
            HttpResponseMessage response = await Client.SendAsync(request);
            IEdmModel model = CsdlReader.Parse(XmlReader.Create(await response.Content.ReadAsStreamAsync()));
            //Can rename an entity + namespace
            IEdmEntityType customer = model.FindDeclaredType("ModelAliasing.Customer") as IEdmEntityType;
            Assert.NotNull(customer);
            //Explicit configuration on the model builder overrides any configuration on the DataContract attribute.
            IEdmEntityType orders = model.FindDeclaredType("AliasedNamespace.Order") as IEdmEntityType;
            Assert.NotNull(orders);
            //Can rename a derived entity  name + namespace
            IEdmEntityType expressOrder = model.FindDeclaredType("Purchasing.ExpressOrder") as IEdmEntityType;
            Assert.NotNull(expressOrder);
            //Can rename a derived entity  name + namespace using DataContract
            IEdmEntityType freeDeliveryOrder = model.FindDeclaredType("Billing.FreeDeliveryOrder") as IEdmEntityType;
            Assert.NotNull(freeDeliveryOrder);
            //Can configure an entity with the data contract attribute
            IEdmEntityType ordersLines = model.FindDeclaredType("Billing.OrderLine") as IEdmEntityType;
            Assert.NotNull(ordersLines);
            //Can change the name and the namespaces for complex types
            IEdmComplexType region = model.FindDeclaredType("Location.PoliticalRegion") as IEdmComplexType;
            Assert.NotNull(region);
            IEdmComplexType address = model.FindDeclaredType("Location.Direction") as IEdmComplexType;
            Assert.NotNull(address);
        }

        [Fact]
        public async Task CanRenamePropertiesInConventionModelBuilder()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/convention/$metadata");
            HttpResponseMessage response = await Client.SendAsync(request);
            IEdmModel model = CsdlReader.Parse(XmlReader.Create(await response.Content.ReadAsStreamAsync()));
            // Can change the name of regular, complex and navigation properties.
            IEdmEntityType customer = model.FindDeclaredType("ModelAliasing.Customer") as IEdmEntityType;
            Assert.NotNull(customer);
            Assert.NotNull(customer.FindProperty("FinancialAddress"));
            Assert.NotNull(customer.FindProperty("ClientName"));
            Assert.NotNull(customer.FindProperty("Purchases"));
            // Can change the name of properties on complex objects
            IEdmComplexType address = model.FindDeclaredType("Location.Direction") as IEdmComplexType;
            Assert.NotNull(address);
            Assert.NotNull(address.FindProperty("Reign"));
            //Can change the name of properties on derived entities.
            IEdmEntityType expressOrder = model.FindDeclaredType("Purchasing.ExpressOrder") as IEdmEntityType;
            Assert.NotNull(expressOrder);
            //Can change the name of properties on derived entities when added explicitly.
            Assert.NotNull(expressOrder.FindProperty("Fee"));
            //Can change the name of properties on derived entities using data contract attribute.
            Assert.NotNull(expressOrder.FindProperty("DeliveryDate"));
            // Can change the name of the properties using DataContract attribute
            IEdmEntityType ordersLines = model.FindDeclaredType("Billing.OrderLine") as IEdmEntityType;
            Assert.NotNull(ordersLines);
            Assert.NotNull(ordersLines.FindProperty("Product"));
            // Data contract attribute override any explicit configuration on the model builder
            Assert.NotNull(ordersLines.FindProperty("Cost"));
        }

        [Fact]
        public async Task CanRenameTypesAndNamespacesInRegularModelBuilder()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/explicit/$metadata");
            HttpResponseMessage response = await Client.SendAsync(request);
            IEdmModel model = CsdlReader.Parse(XmlReader.Create(await response.Content.ReadAsStreamAsync()));
            //Can rename an entity + namespace
            IEdmEntityType customer = model.FindDeclaredType("ModelAliasing.Customer") as IEdmEntityType;
            Assert.NotNull(customer);
            //Explicit configuration on the model builder overrides any configuration on the DataContract attribute.
            IEdmEntityType orders = model.FindDeclaredType("AliasedNamespace.Order") as IEdmEntityType;
            Assert.NotNull(orders);
            //Can rename a derived entity  name + namespace
            IEdmEntityType expressOrder = model.FindDeclaredType("Purchasing.ExpressOrder") as IEdmEntityType;
            Assert.NotNull(expressOrder);
            //DataContract doesn't rename the entity or the namespace
            IEdmEntityType freeDeliveryOrder = model.FindDeclaredType("Purchasing.FreeOrder") as IEdmEntityType;
            Assert.NotNull(freeDeliveryOrder);
            //DataContract doesn't rename the entity or the namespace
            IEdmEntityType ordersLines = model.FindDeclaredType("Billing.OrderLine") as IEdmEntityType;
            Assert.Null(ordersLines);
            //Can change the name and the namespaces for complex types
            IEdmComplexType region = model.FindDeclaredType("Location.PoliticalRegion") as IEdmComplexType;
            Assert.NotNull(region);
            IEdmComplexType address = model.FindDeclaredType("Location.Direction") as IEdmComplexType;
            Assert.NotNull(address);
        }

        [Fact]
        public async Task CanRenamePropertiesInRegularModelBuilder()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/explicit/$metadata");
            HttpResponseMessage response = await Client.SendAsync(request);
            IEdmModel model = CsdlReader.Parse(XmlReader.Create(await response.Content.ReadAsStreamAsync()));
            // Can change the name of regular, complex and navigation properties.
            IEdmEntityType customer = model.FindDeclaredType("ModelAliasing.Customer") as IEdmEntityType;
            Assert.NotNull(customer);
            Assert.NotNull(customer.FindProperty("FinancialAddress"));
            Assert.NotNull(customer.FindProperty("ClientName"));
            Assert.NotNull(customer.FindProperty("Purchases"));
            // Can change the name of properties on complex objects
            IEdmComplexType address = model.FindDeclaredType("Location.Direction") as IEdmComplexType;
            Assert.NotNull(address);
            Assert.NotNull(address.FindProperty("Reign"));
            //Can change the name of properties on derived entities.
            IEdmEntityType expressOrder = model.FindDeclaredType("Purchasing.ExpressOrder") as IEdmEntityType;
            Assert.NotNull(expressOrder);
            //Can change the name of properties on derived entities when added explicitly.
            Assert.NotNull(expressOrder.FindProperty("Fee"));
            //Data contract attribute doesn't change the name of the property.
            Assert.Null(expressOrder.FindProperty("DeliveryDate"));
            Assert.Null(expressOrder.FindProperty("GuanteedDeliveryDate"));
            // Data contract attribute doesn't change the names of the properties
            IEdmEntityType ordersLines = model.FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.ModelAliasing.ModelAliasingMetadataOrderLine") as IEdmEntityType;
            Assert.NotNull(ordersLines);
            Assert.Null(ordersLines.FindProperty("Product"));
            Assert.NotNull(ordersLines.FindProperty("Item"));
            // Data contract attribute doesn't override any explicit configuration on the model builder
            Assert.NotNull(ordersLines.FindProperty("Cost"));
        }
    }
}
