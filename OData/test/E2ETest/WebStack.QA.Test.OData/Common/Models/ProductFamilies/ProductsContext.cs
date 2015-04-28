using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace WebStack.QA.Test.OData.Common.Models.ProductFamilies
{
    public class ProductsContext : DbContext
    {
        public ProductsContext()
            : base("Products")
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductFamily> ProductFamilies { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<ProductFamily>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Supplier>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Supplier>().Property(p => p.Country).HasColumnName("Country");
            base.OnModelCreating(modelBuilder);
        }
    }
}
