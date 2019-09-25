// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace AspNetCoreODataSample.DynamicModels.Web.Models
{
    public class HouseContext : DbContext
    {
        public HouseContext(DbContextOptions<HouseContext> options)
            : base(options)
        {
        }

        public DbSet<House> Houses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Interior> Interior { get; set; }
        public DbSet<InteriorDefinition> InteriorDefinitions { get; set; }
        public DbSet<InteriorPropertyDefinition> InteriorPropertyDefinitions { get; set; }
    }
}
