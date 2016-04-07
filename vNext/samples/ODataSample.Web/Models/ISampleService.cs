using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ODataSample.Web.Models
{
	public class ApplicationUser : IdentityUser<string>
	{
	}


	public interface ISampleService
    {
        IQueryable<ApplicationUser> Users { get; }
        IQueryable<Product> Products { get; }
		IQueryable<Customer> Customers { get; }
    }
}
