using System.Linq;

namespace ODataSample.Web.Models
{
	public interface ISampleService
    {
        IQueryable<ApplicationUser> Users { get; }
        IQueryable<Order> Orders { get; }
        IQueryable<Product> Products { get; }
		IQueryable<Customer> Customers { get; }
    }
}
