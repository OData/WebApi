// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Microsoft.Test.E2E.AspNet.OData.EntitySetAggregation
{
    public class EntitySetAggregationContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EntitySetAggregationTest1";

        public EntitySetAggregationContext() : base(ConnectionString) { }

        public DbSet<Customer> Customers { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Order> Orders { get; set; }

        public Address Address { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public SaleInfo SaleInfo { get; set; }
    }

    public class SaleInfo
    {
        public int Quantity { get; set; }

        public int UnitPrice { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }
}
