using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ODataSample.Web.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, ISampleService
	{
		public DbSet<Product> Products { get; set; }
		public DbSet<Customer> Customers { get; set; }

		public ApplicationDbContext()
			: base("name=DataContext")
		{

		}

		IQueryable<ApplicationUser> ISampleService.Users => Users;
		IQueryable<Product> ISampleService.Products => Products;
		IQueryable<Customer> ISampleService.Customers => Customers;

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	optionsBuilder.UseSqlServer(
		//		"Server=.;Database=Microsoft.AspNetCore.OData.App.Data;User ID=morselsLogin;Password=PPm|Wb(An!Cb1~{}&]UPxO@nf;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
		//	base.OnConfiguring(optionsBuilder);
		//}
	}
}
