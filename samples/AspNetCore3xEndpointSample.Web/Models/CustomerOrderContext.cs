// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

//using Microsoft.EntityFrameworkCore;

using System.Data.Entity;

namespace AspNetCore3xEndpointSample.Web.Models
{
    public class CustomerOrderContext : DbContext
    {
        //public CustomerOrderContext(DbContextOptions<CustomerOrderContext> options)
        //    : base(options)
        //{
        //}

        public CustomerOrderContext(string connectString)
            : base(connectString)
        {
        }

        public DbSet<Customer> Customers { get; set; }

     //   public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerPhone>().HasOptional(a => a.Formatted).WithRequired();
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.CustomerReferrals)
                .WithRequired(c => c.Customer)
                .HasForeignKey(c => c.CustomerID);
        }
    }
}
