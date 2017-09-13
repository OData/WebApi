// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity;

namespace WebStack.QA.Test.OData.SingleResultTest
{
    public class SingleResultContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=SingleResultTest";

        public SingleResultContext()
            : base(ConnectionString)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }

        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
    }
}
