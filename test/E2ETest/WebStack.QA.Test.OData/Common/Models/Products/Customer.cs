using System.Collections.ObjectModel;

namespace WebStack.QA.Test.OData.Common.Models.Products
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerCollection : Collection<Customer>
    {
    }
}
