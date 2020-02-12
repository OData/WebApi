// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    public class EndpointDbContext : DbContext
    {
        public EndpointDbContext(DbContextOptions<EndpointDbContext> options)
            : base(options)
        {
        }

        public DbSet<EpCustomer> Customers { get; set; }

        public DbSet<EpOrder> Orders { get; set; }

        protected override void OnModelCreating(EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EpCustomer>().OwnsOne(c => c.HomeAddress).WithOwner();
        }
    }
}
