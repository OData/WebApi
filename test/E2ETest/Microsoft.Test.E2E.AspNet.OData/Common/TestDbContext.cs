// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace Microsoft.Test.E2E.AspNetCore.OData.Common
{
    /// <summary>
    /// TestDbContext is a DbContext that works with either EntityFramework or EntityFrameworkCore
    /// </summary>
    public class TestDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDbContext"/> class.
        /// </summary>
        /// <param name="ConnectionString">The connection string.</param>
        public TestDbContext(string connectionString)
#if !NETCORE
            : base(connectionString) {}
#else
        {
            this.PrivateConnectionString = connectionString;
        }

        private string PrivateConnectionString { get; set; }

        /// <inheritdocs/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.PrivateConnectionString);
        }
#endif
    }

    /// <summary>
    /// TestDbContext is a TestDbSet that works with either EntityFramework or EntityFrameworkCore
    /// </summary>
    public abstract class TestDbSet<TEntity> : DbSet<TEntity> where TEntity : class
    {
    }
}
