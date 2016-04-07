using System.Linq;

namespace ODataSample.Web.Models
{
    public interface ISampleService
    {
        IQueryable<Product> Products { get; }
		IQueryable<Customer> Customers { get; }
    }
}
