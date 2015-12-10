using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Builder;

namespace ODataSample.Web.Models
{
    public interface ISampleService
    {
        IEnumerable<Product> Products { get; }
        IEnumerable<Customer> Customers { get; }
        [ODataFunction(IsBound = false)]
        IEnumerable<Customer> FindCustomersWithProduct(int productId);
        [ODataAction(IsBound = true, BindingName = "customer")]
        Customer AddCustomerProduct(int customerId, int productId);
        [ODataAction(IsBound = true, BindingName = "customer")]
        Customer AddCustomerProducts(int customerId, IEnumerable<int> products);
        [ODataFunction]
        bool TestPrimitiveReturnType();
    }

}
