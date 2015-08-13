using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataSample.Web.Models
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.OData.Edm.Library;

    public interface ISampleService
    {
        IEnumerable<Product> Products { get; }
        IEnumerable<Customer> Customers { get; }
        [ODataFunction]
        IEnumerable<Customer> FindCustomersWithProduct(int productId);
    }
}
