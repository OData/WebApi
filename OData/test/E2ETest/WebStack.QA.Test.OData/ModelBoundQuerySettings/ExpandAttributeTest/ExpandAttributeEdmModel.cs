using System.Collections.Generic;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.ExpandAttributeTest
{
    [Expand("Orders", "Friend", MaxDepth = 10)]
    [Expand("AutoExpandOrder", ExpandType = ExpandType.Automatic, MaxDepth = 8)]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Expand(ExpandType = ExpandType.Disabled)]
        public Order Order { get; set; }

        public Order AutoExpandOrder { get; set; }

        public Address Address { get; set; }

        [Expand("Customers", MaxDepth = 2)]
        public List<Order> Orders { get; set; }

        public List<Order> NoExpandOrders { get; set; }

        public List<Address> Addresses { get; set; }

        [Expand(MaxDepth = 2)]
        public Customer Friend { get; set; }
    }

    [Expand(MaxDepth = 6)]
    [Expand("NoExpandCustomers", ExpandType = ExpandType.Disabled)]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [Expand("Orders")]
        public List<Customer> Customers { get; set; }

        [Expand("Order")]
        public List<Customer> Customers2 { get; set; }

        public List<Customer> NoExpandCustomers { get; set; } 

        public Order RelatedOrder { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class ExpandAttributeEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers")
                .EntityType.Expand(10, "Orders", "Friend")
                .Expand(8, ExpandType.Automatic, "AutoExpandOrder");
            builder.EntityType<Customer>().HasMany(p => p.Orders).Expand(2, "Customers");

            builder.EntitySet<Order>("Orders")
                .EntityType.Expand(6)
                .Expand(ExpandType.Disabled, "NoExpandCustomers");
            builder.EntityType<Order>().HasMany(p => p.Customers).Expand("Orders");
            builder.EntityType<Order>().HasMany(p => p.Customers2).Expand("Order");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
