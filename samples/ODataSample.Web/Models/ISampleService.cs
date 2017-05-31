using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataSample.Web.Models
{
    public interface ISampleService
    {
        IEnumerable<Product> Products { get; }
        IEnumerable<Customer> Customers { get; }
    }
}
