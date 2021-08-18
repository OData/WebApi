//-----------------------------------------------------------------------------
// <copyright file="ForeignKeyContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Microsoft.Test.E2E.AspNet.OData.ForeignKey
{
    public class ForeignKeyContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=ForeignKeyE2EContextV4_1";

        public ForeignKeyContext()
            : base(ConnectionString)
        {
        }

        public DbSet<ForeignKeyCustomer> Customers { get; set; }

        public DbSet<ForeignKeyOrder> Orders { get; set; }
    }

    public class ForeignKeyContextNoCascade : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=ForeignKeyE2EContextNoCascadeV4_1";

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
