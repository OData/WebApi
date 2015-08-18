using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

namespace ODataSample.Web.Models
{
    public interface ISampleService
    {
        IEnumerable<Product> Products { get; }
        IEnumerable<Customer> Customers { get; }
        [ODataFunction]
        IEnumerable<Customer> FindCustomersWithProduct(int productId);
    }
}
