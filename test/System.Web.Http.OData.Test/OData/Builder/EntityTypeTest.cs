// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class EntityTypeTest
    {
        [Fact]
        public void CreateEntityType()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType();
            var model = builder.GetServiceModel();
            var customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(t => t.Name == "Customer");
            Assert.NotNull(customerType);
            Assert.Equal("Customer", customerType.Name);
            Assert.Equal(typeof(Customer).Namespace, customerType.Namespace);
            Assert.Equal("CustomerId", customerType.DeclaredKey.Single().Name);
            Assert.Equal(5, customerType.DeclaredProperties.Count());
        }

        [Fact]
        public void CreateEntityTypeWithRelationship()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType().Add_Order_EntityType().Add_OrderCustomer_Relationship();

            var model = builder.GetServiceModel();
            var orderType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(t => t.Name == "Order");
            Assert.NotNull(orderType);
            Assert.Equal("Order", orderType.Name);
            Assert.Equal(typeof(Order).Namespace, orderType.Namespace);
            Assert.Equal("OrderId", orderType.DeclaredKey.Single().Name);
            Assert.Equal(5, orderType.DeclaredProperties.Count());
            Assert.Equal(1, orderType.NavigationProperties().Count());
            var deliveryDateProperty = orderType.DeclaredProperties.Single(dp => dp.Name == "DeliveryDate");
            Assert.NotNull(deliveryDateProperty);
            Assert.True(deliveryDateProperty.Type.IsNullable);

            Assert.Equal("Customer", orderType.NavigationProperties().First().Name);
            Assert.Equal("Customer", orderType.NavigationProperties().First().ToEntityType().Name);

            var customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(t => t.Name == "Customer");
            Assert.NotNull(customerType);
            Assert.Equal("Customer", customerType.Name);
            Assert.Equal(typeof(Customer).Namespace, customerType.Namespace);
            Assert.Equal("CustomerId", customerType.DeclaredKey.Single().Name);
            Assert.Equal(5, customerType.DeclaredProperties.Count());
        }

        [Fact]
        public void CanCreateEntityWithCompoundKey()
        {
            var builder = new ODataModelBuilder();
            var customer = builder.Entity<Customer>();
            customer.HasKey(c => new { c.CustomerId, c.Name });
            customer.Property(c => c.SharePrice);
            customer.Property(c => c.ShareSymbol);
            customer.Property(c => c.Website);

            var model = builder.GetServiceModel();
            var customerType = model.FindType(typeof(Customer).FullName) as IEdmEntityType;
            Assert.Equal(5, customerType.Properties().Count());
            Assert.Equal(2, customerType.DeclaredKey.Count());
            Assert.NotNull(customerType.DeclaredKey.SingleOrDefault(k => k.Name == "CustomerId"));
            Assert.NotNull(customerType.DeclaredKey.SingleOrDefault(k => k.Name == "Name"));
        }

        [Fact]
        public void CanCreateEntityWithCollectionProperties()
        {
            var builder = new ODataModelBuilder();
            var customer = builder.Entity<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.CollectionProperty(c => c.Aliases);
            customer.CollectionProperty(c => c.Addresses);


            var aliasesProperty = customer.Properties.OfType<CollectionPropertyConfiguration>().SingleOrDefault(p => p.Name == "Aliases");
            var addressesProperty = customer.Properties.OfType<CollectionPropertyConfiguration>().SingleOrDefault(p => p.Name == "Addresses");

            Assert.Equal(3, customer.Properties.Count());
            Assert.Equal(2, customer.Properties.OfType<CollectionPropertyConfiguration>().Count());
            Assert.NotNull(aliasesProperty);
            Assert.Equal(typeof(string), aliasesProperty.ElementType);
            Assert.NotNull(addressesProperty);
            Assert.Equal(typeof(Address), addressesProperty.ElementType);

            Assert.Equal(2, builder.StructuralTypes.Count());
            var addressType = builder.StructuralTypes.Skip(1).FirstOrDefault();
            Assert.NotNull(addressType);
            Assert.Equal(EdmTypeKind.Complex, addressType.Kind);
            Assert.Equal(typeof(Address).FullName, addressType.FullName);

            var model = builder.GetServiceModel();
            var edmCustomerType = model.FindType(typeof(Customer).FullName) as IEdmEntityType;
            var edmAddressType = model.FindType(typeof(Address).FullName) as IEdmComplexType;
        }
    }
}
