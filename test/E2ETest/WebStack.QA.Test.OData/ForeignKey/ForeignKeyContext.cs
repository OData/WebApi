// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebStack.QA.Test.OData.ForeignKey
{
    public class ForeignKeyContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=ForeignKeyE2EContextV4";

        public ForeignKeyContext()
            : base(ConnectionString)
        {
        }

        public DbSet<ForeignKeyCustomer> Customers { get; set; }

        public DbSet<ForeignKeyOrder> Orders { get; set; }
    }

    public class ForeignKeyContextNoCascade : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=ForeignKeyE2EContextNoCascadeV4";

        public ForeignKeyContextNoCascade()
            : base(ConnectionString)
        {
        }

        public DbSet<ForeignKeyCustomer> Customers { get; set; }

        public DbSet<ForeignKeyOrder> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}
