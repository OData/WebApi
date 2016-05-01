using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ODataSample.Web.Models
{
	public interface ISampleService
    {
        IQueryable<ApplicationUser> Users { get; }
        //IQueryable<IdentityUserRole<string>> UserRoles { get; }
        //IQueryable<IdentityRole> Roles { get; }
        IQueryable<Order> Orders { get; }
        IQueryable<Product> Products { get; }
		IQueryable<Customer> Customers { get; }
    }
}
