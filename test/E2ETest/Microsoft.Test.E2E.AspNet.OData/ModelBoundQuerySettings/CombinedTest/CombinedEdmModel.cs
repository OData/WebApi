//-----------------------------------------------------------------------------
// <copyright file="CombinedEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CombinedTest
{
    [Expand("Orders", "Friend", "CountableOrders", MaxDepth = 10)]
    [Expand("AutoExpandOrder", ExpandType = SelectExpandType.Automatic, MaxDepth = 8)]
    [Page(MaxTop = 5, PageSize = 1)]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Expand(ExpandType = SelectExpandType.Disabled)]
        public Order Order { get; set; }

        public Order AutoExpandOrder { get; set; }

        public Address Address { get; set; }

        [Expand("Customers", MaxDepth = 2)]
        [Count(Disabled = true)]
        [Page(MaxTop = 2, PageSize = 1)]
        public List<Order> Orders { get; set; }

        public List<Order> CountableOrders { get; set; }

        public List<Order> NoExpandOrders { get; set; }

        public List<Address> Addresses { get; set; }

        [Expand(MaxDepth = 2)]
        public Customer Friend { get; set; }
    }

    [Count]
    [Expand(MaxDepth = 6)]
    [Expand("NoExpandCustomers", ExpandType = SelectExpandType.Disabled)]
    [Filter("Id", Disabled = true)]
    [Filter]
    [OrderBy("Id", Disabled = true)]
    [OrderBy]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [Expand("Orders")]
        [Page(MaxTop = 2, PageSize = 1)]
        [Count(Disabled = true)]
        public List<Customer> Customers { get; set; }

        [Expand("Order")]
        [Page(MaxTop = 1)]
        [Count]
        public List<Customer> Customers2 { get; set; }

        public List<Customer> NoExpandCustomers { get; set; } 

        public Order RelatedOrder { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class CombinedEdmModel
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
                .EntityType.Expand(10, "Orders", "Friend", "CountableOrders")
                .Expand(8, SelectExpandType.Automatic, "AutoExpandOrder")
                .Page(5, 2);
            builder.EntityType<Customer>()
                .HasMany(p => p.Orders)
                .Expand(2, "Customers")
                .Page(2, 1)
                .Count(QueryOptionSetting.Disabled);
            builder.EntityType<Customer>()
                .HasMany(p => p.CountableOrders)
                .Count();
            builder.EntityType<Customer>()
                .HasOptional(p => p.Order)
                .Expand(SelectExpandType.Disabled);

            builder.EntitySet<Order>("Orders")
                .EntityType.Expand(6)
                .Expand(SelectExpandType.Disabled, "NoExpandCustomers")
                .Count()
                .Filter()
                .Filter(QueryOptionSetting.Disabled, "Id")
                .OrderBy()
                .OrderBy(QueryOptionSetting.Disabled, "Id");
            builder.EntityType<Order>()
                .HasMany(p => p.Customers)
                .Expand("Orders")
                .Page(2, 1)
                .Count(QueryOptionSetting.Disabled);
            builder.EntityType<Order>().HasMany(p => p.Customers2).Expand("Order").Page(1, null).Count();

            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
