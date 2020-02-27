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
            // In EF Core 2.x, we have to config the collection of owned types using the following settings.
            modelBuilder.Entity<EpCustomer>().OwnsOne(c => c.HomeAddress);
            modelBuilder.Entity<EpCustomer>().OwnsMany(c => c.FavoriteAddresses, a =>
            {
                a.HasForeignKey("OwnerId");
                a.Property<int>("Id");
                a.HasKey("Id");
            });

            // But, in EF Core 3.x, it seems we can only use the following codes:
            // modelBuilder.Entity<EpCustomer>().OwnsOne(c => c.HomeAddress).WithOwner();
        }
    }
}
