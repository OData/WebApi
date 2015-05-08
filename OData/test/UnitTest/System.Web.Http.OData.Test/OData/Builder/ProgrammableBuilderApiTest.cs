﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class ProgrammableBuilderApiTest
    {
        [Fact]
        [Trait("Description", "ODataModelBuilder can build model without using Generic methods")]
        public void CreateModelUsingProgrammableApi()
        {
            var builder = new ODataModelBuilder();
            var customerConfig = builder.AddEntity(typeof(Customer));
            customerConfig.HasKey(typeof(Customer).GetProperty("CustomerId"));
            customerConfig.AddProperty(typeof(Customer).GetProperty("Name"));
            var ordersPropertyConfig = customerConfig.AddNavigationProperty(typeof(Customer).GetProperty("Orders"), EdmMultiplicity.Many);

            var orderConfig = builder.AddEntity(typeof(Order));
            orderConfig.HasKey(typeof(Order).GetProperty("OrderId"));
            orderConfig.AddProperty(typeof(Order).GetProperty("Cost"));

            var customersSetConfig = builder.AddEntitySet("Customers", customerConfig);
            var ordersSetConfig = builder.AddEntitySet("Orders", orderConfig);
            customersSetConfig.AddBinding(ordersPropertyConfig, ordersSetConfig);

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

            var entityContainer = model.EntityContainers().Single();
            Assert.NotNull(entityContainer);

            var customers = entityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers);
            Assert.Equal(typeof(Customer).FullName, customers.ElementType.FullName());

            var orders = entityContainer.FindEntitySet("Orders");
            Assert.NotNull(orders);
            Assert.Equal(typeof(Order).FullName, orders.ElementType.FullName());
        }
    }
}
