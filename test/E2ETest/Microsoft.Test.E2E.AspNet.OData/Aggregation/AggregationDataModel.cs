// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info=True;Database=AggregationTest1";

        public static string LastCommand { get; private set; } = "";

        public AggregationContext()
            : base(ConnectionString)
        {
            this.Database.Log = (sql) =>
            {
                if (sql.Contains("SELECT") 
                    && !sql.Contains("_Migration")
                    && !sql.Contains("INFORMATION_SCHEMA"))
                {
                    LastCommand += sql;
                }
            };
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            IDatabaseInitializer<AggregationContext> addBucket = new DropCreateDatabaseIfModelChanges<AggregationContext>();
            Database.SetInitializer<AggregationContext>(addBucket);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Customer> Customers { get; set; }

        public static void CleanCommands()
        {
            LastCommand = "";
        }
    }

#if !NETCOREAPP3_0
    [System.Data.Linq.Mapping.Table(Name = "Customer")]
#endif
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public CustomerBucket? Bucket { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }
    }

    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Street { get; set; }
    }

    public enum CustomerBucket
    {
        Small,
        Medium,
        Big
    }
}
