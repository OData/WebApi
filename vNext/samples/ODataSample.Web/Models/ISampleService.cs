using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataSample.Web.Models
{
    public interface ISampleService
    {
        IQueryable<Product> Products { get; }
		IQueryable<Customer> Customers { get; }
    }
}
