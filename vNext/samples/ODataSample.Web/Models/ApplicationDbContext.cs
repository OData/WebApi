using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ODataSample.Web.Models
{
	public class ApplicationDbContext : DbContext, ISampleService
	{
		public DbSet<Product> Products { get; set; }
		public DbSet<Customer> Customers { get; set; }

		IQueryable<Product> ISampleService.Products => Products;
		IQueryable<Customer> ISampleService.Customers => Customers;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(
				"Server=.;Database=Microsoft.AspNetCore.OData.App.Data;User ID=morselsLogin;Password=PPm|Wb(An!Cb1~{}&]UPxO@nf;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
			base.OnConfiguring(optionsBuilder);
		}
	}
}
