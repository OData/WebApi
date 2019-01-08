// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


using Microsoft.EntityFrameworkCore;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationContextCore : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AggregationContextCore");
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
