using System.Data.Entity;

namespace WebStack.QA.Test.OData.Aggregation
{
    public class AggregationContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=AggregationTest";

        public AggregationContext()
            : base(ConnectionString)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }
}
