// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class ProgrammableBuilderApiTest
    {
        [Fact]
        [Trait("Description", "ODataModelBuilder can build model without using Generic methods")]
        public void CreateModelUsingProgrammableApi()
        {
            var builder = new ODataModelBuilder();
            var customerConfig = builder.AddEntityType(typeof(Customer));
            customerConfig.HasKey(typeof(Customer).GetProperty("CustomerId"));
            customerConfig.AddProperty(typeof(Customer).GetProperty("Name"));
            var ordersPropertyConfig = customerConfig.AddNavigationProperty(typeof(Customer).GetProperty("Orders"), EdmMultiplicity.Many);

            var orderConfig = builder.AddEntityType(typeof(Order));
            orderConfig.HasKey(typeof(Order).GetProperty("OrderId"));
            orderConfig.AddProperty(typeof(Order).GetProperty("Cost"));

            var customersSetConfig = builder.AddEntitySet("Customers", customerConfig);
            var ordersSetConfig = builder.AddEntitySet("Orders", orderConfig);
            customersSetConfig.AddBinding(ordersPropertyConfig, ordersSetConfig);

            var meConfig = builder.AddSingleton("Me", customerConfig);

            var model = builder.GetServiceModel();
            var customerType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Customer");
            Assert.NotNull(customerType);
            Assert.Equal(typeof(Customer).Namespace, customerType.Namespace);
            Assert.Equal(3, customerType.DeclaredProperties.Count());

            var key = customerType.DeclaredKey.SingleOrDefault();
            Assert.NotNull(key);
            Assert.Equal("CustomerId", key.Name);
            Assert.True(key.Type.IsInt32());
            Assert.False(key.Type.IsNullable);

            var nameProperty = customerType.DeclaredProperties.SingleOrDefault(dp => dp.Name == "Name");
            Assert.NotNull(nameProperty);
            Assert.True(nameProperty.Type.IsString());
            Assert.True(nameProperty.Type.IsNullable);

            Assert.Equal(1, customerType.NavigationProperties().Count());
            var ordersProperty = customerType.NavigationProperties().Single();
            Assert.Equal("Orders", ordersProperty.Name);
            Assert.Equal(EdmTypeKind.Collection, ordersProperty.Type.Definition.TypeKind);
            Assert.Equal(typeof(Order).FullName, (ordersProperty.Type.Definition as IEdmCollectionType).ElementType.FullName());

            var entityContainer = model.EntityContainer;
            Assert.NotNull(entityContainer);

            var customers = entityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers);
            Assert.Equal(typeof(Customer).FullName, customers.EntityType().FullName());

            var orders = entityContainer.FindEntitySet("Orders");
            Assert.NotNull(orders);

            Assert.Equal(typeof(Order).FullName, orders.EntityType().FullName());

            var me = entityContainer.FindSingleton("Me");
            Assert.NotNull(me);
            Assert.Equal(typeof(Customer).FullName, me.EntityType().FullName());
        }
    }
}
