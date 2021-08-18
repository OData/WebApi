//-----------------------------------------------------------------------------
// <copyright file="ExpandAttributeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.ExpandAttributeTest
{
    [Expand("Orders", "Friend", MaxDepth = 10)]
    [Expand("AutoExpandOrder", ExpandType = SelectExpandType.Automatic, MaxDepth = 8)]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Expand(ExpandType = SelectExpandType.Disabled)]
        public Order Order { get; set; }

        public Order AutoExpandOrder { get; set; }

        public Address Address { get; set; }

        [Expand("Customers", MaxDepth = 2)]
        public List<Order> Orders { get; set; }

        public List<Order> NoExpandOrders { get; set; }

        public List<Address> Addresses { get; set; }

        [Expand(MaxDepth = 2)]
        public Customer Friend { get; set; }
    }

    [Expand("SpecialOrder", ExpandType = SelectExpandType.Automatic)]
    public class SpecialCustomer : Customer
    {
        public Order SpecialOrder { get; set; }
    }

    [Expand(MaxDepth = 6)]
    [Expand("NoExpandCustomers", ExpandType = SelectExpandType.Disabled)]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [Expand("Orders")]
        public List<Customer> Customers { get; set; }

        [Expand("Order")]
        public List<Customer> Customers2 { get; set; }

        public List<Customer> NoExpandCustomers { get; set; } 

        public Order RelatedOrder { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class ExpandAttributeEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers")
                .EntityType.Expand(10, "Orders", "Friend")
                .Expand(8, SelectExpandType.Automatic, "AutoExpandOrder");
            builder.EntityType<Customer>().HasMany(p => p.Orders).Expand(2, "Customers");

            builder.EntityType<SpecialCustomer>()
                .HasOptional(p => p.SpecialOrder)
                .Expand(SelectExpandType.Automatic, "SpecialOrder");

            builder.EntitySet<Order>("Orders")
                .EntityType.Expand(6)
                .Expand(SelectExpandType.Disabled, "NoExpandCustomers");
            builder.EntityType<Order>().HasMany(p => p.Customers).Expand("Orders");
            builder.EntityType<Order>().HasMany(p => p.Customers2).Expand("Order");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
