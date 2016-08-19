using System;
using System.Collections.Generic;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.DependencyInjection
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }

        public List<Order> Orders { get; set; }

        public List<Address> Addresses { get; set; }
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

    public enum CustomerType
    {
        Normal,
        Vip
    }

    public class EdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EnumType<CustomerType>();
            builder.EntityType<Customer>().Collection.Function("EnumFunction").Returns<Enum>();
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
