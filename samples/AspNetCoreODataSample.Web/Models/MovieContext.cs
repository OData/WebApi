//-----------------------------------------------------------------------------
// <copyright file="MovieContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace AspNetCoreODataSample.Web.Models
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieStar> MovieStars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieStar>().HasKey(_ => new
            {
                _.FirstName,
                _.LastName
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
