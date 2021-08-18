//-----------------------------------------------------------------------------
// <copyright file="AutoExpandDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Data.Entity;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.E2E.AspNet.OData.AutoExpand
{
    public class AutoExpandCustomerContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AutoExpandCustomerContext1";

        public AutoExpandCustomerContext()
            : base(ConnectionString)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }

    public class AutoExpandPeopleContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AutoExpandPeopleContext1";

        public AutoExpandPeopleContext()
            : base(ConnectionString)
        {
        }

        public DbSet<People> People { get; set; }
    }

    public class AutoExpandOrdersContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AutoExpandOrdersContext1";

        public AutoExpandOrdersContext()
            : base(ConnectionString)
        {
        }

        public DbSet<NormalOrder> NormalOrders { get; set; }
    }

    public class People
    {
        public int Id { get; set; }

        [AutoExpand]
        public Order Order { get; set; }

        public People Friend { get; set; }
    }

    [AutoExpand]
    public class Customer
    {
        public int Id { get; set; }

        public Order Order { get; set; }

        public Customer Friend { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        [AutoExpand]
        public ChoiceOrder Choice { get; set; }
    }

    public class ChoiceOrder
    {
        public int Id { get; set; }

        public double Amount { get; set; }
    }

    public class SpecialOrder : Order
    {
        [AutoExpand]
        public ChoiceOrder SpecialChoice { get; set; }
    }

    public class VipOrder : SpecialOrder
    {
        [AutoExpand]
        public ChoiceOrder VipChoice { get; set; }
    }

    public class NormalOrder
    {
        public int Id { get; set; }

        public NormalOrder LinkOrder { get; set; }
    }

    public class DerivedOrder : NormalOrder
    {
        [AutoExpand]
        public OrderDetail OrderDetail { get; set; }

        [AutoExpand(DisableWhenSelectPresent = true)]
        public OrderDetail NotShownDetail { get; set; }
    }

    [AutoExpand(DisableWhenSelectPresent = true)]
    public class DerivedOrder2 : NormalOrder
    {
        public OrderDetail NotShownDetail { get; set; }
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }
}
