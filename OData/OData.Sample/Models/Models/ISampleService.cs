using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ODataSample.Web.Models
{
	public class ApplicationUser : IdentityUser
	{
	}


	public interface ISampleService
    {
        IQueryable<ApplicationUser> Users { get; }
        IQueryable<Product> Products { get; }
		IQueryable<Customer> Customers { get; }
    }
}
