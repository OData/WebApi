// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder.TestModels;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class RoutingTest
    {
        [Fact]
        [Trait("Description", "Edmlib can emit a model with a single EntityType only")]
        public void CanUseRelativeLinks()
        {
            var builder = new ODataModelBuilder()
                            .Add_Customer_EntityType()
                            .Add_Order_EntityType()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customers_EntitySet()
                            .Add_Orders_EntitySet()
                            .Add_CustomerOrders_Binding();


            var customersSet = builder.EntitySet<Customer>("Customers");
            customersSet.HasEditLink(o => new Uri(string.Format("Customers({0})", o.EntityInstance.CustomerId), UriKind.Relative), followsConventions: false);
            customersSet.FindBinding("Orders").HasLinkFactory(o => string.Format("Orders/ByCustomerId/{0}", ((Customer)o.EntityInstance).CustomerId));

            var model = builder.GetEdmModel();

            var container = model.FindDeclaredEntityContainer("Container");
            var customerEdmEntitySet = container.FindEntitySet("Customers");
            // TODO: Fix later, need to add a reference
            //var entityContext = new EntityInstanceContext<Customer>(model, customerEdmEntitySet, (IEdmEntityType)customerEdmEntitySet.ElementType, null, new Customer { CustomerId = 24 });

            //Assert.Equal("Customers", customersSet.GetUrl());
            ///Assert.Equal("Customers(24)", customersSet.GetEditLink(entityContext).ToString());
            //Assert.Equal("Orders/ByCustomerId/24", customersSet.FindBinding("Orders").GetLink(entityContext));
        }
    }
}
