using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebStack.QA.Test.OData.ForeignKey
{
    public class ForeignKeyContext : DbContext
    {
        public static string _connectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=ForeignKeyE2EContext";

        public ForeignKeyContext()
            : base(_connectionString)
        {
        }

        public DbSet<ForeignKeyCustomer> Customers { get; set; }

        public DbSet<ForeignKeyOrder> Orders { get; set; }
    }

    public class ForeignKeyContextNoCascade : DbContext
    {
        public static string _connectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=ForeignKeyE2EContextNoCascade";

        public ForeignKeyContextNoCascade()
            : base(_connectionString)
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
