//-----------------------------------------------------------------------------
// <copyright file="SingleResultDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Entity;

namespace Microsoft.Test.E2E.AspNet.OData.SingleResultTest
{
    public class SingleResultContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=SingleResultTest1";

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
