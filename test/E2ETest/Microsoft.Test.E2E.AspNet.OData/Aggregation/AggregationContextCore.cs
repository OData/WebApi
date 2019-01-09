// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


using Microsoft.EntityFrameworkCore;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationContextCoreBase : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }


    public class AggregationContextCoreInMemory : AggregationContextCoreBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AggregationContextCore");
            base.OnConfiguring(optionsBuilder);
        }
    }


    public class AggregationContextCoreSql : AggregationContextCoreBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info = True;Database = AggregationTestCore1");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
