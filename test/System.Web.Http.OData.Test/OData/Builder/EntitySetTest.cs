// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Xunit;

namespace System.Web.Http.OData.Builder
{
    public class EntitySetTest
    {
        [Fact]
        [Trait("Description", "ODataModelBuilder can create an Entity and EntitySet")]
        public void CreateModelWithEntitySet()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet();
            builder.EntitySet<Customer>("Customers");

            var model = builder.GetServiceModel();
            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);
            var customers = container.EntitySets().SingleOrDefault(e => e.Name == "Customers");
            Assert.NotNull(customers);
            Assert.Equal("Customer", customers.ElementType.Name);
        }

        [Fact]
        [Trait("Description", "ODataModelBuilder can create two Entities, two Entities sets and a binding")]
        public void CreateModelWithTwoEntitySetsAndABinding()
        {
            var builder = new ODataModelBuilder()
                            .Add_Customer_EntityType()
                            .Add_Order_EntityType()
                            .Add_CustomerOrders_Relationship()
                            .Add_CustomerOrders_Binding();

            var model = builder.GetServiceModel();
            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);
            var customers = container.EntitySets().SingleOrDefault(e => e.Name == "Customers");
            Assert.NotNull(customers);
            var orders = container.EntitySets().SingleOrDefault(e => e.Name == "Orders");
            Assert.NotNull(orders);
            var customerOrders = customers.NavigationTargets.First(nt => nt.NavigationProperty.Name == "Orders");
            Assert.NotNull(customerOrders);
            Assert.Equal("Orders", customerOrders.TargetEntitySet.Name);
        }
    }
}
