using System.Collections.Generic;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order AutoExpandOrder { get; set; }

        public Address Address { get; set; }

        public List<Order> Orders { get; set; }
    }

    [OrderBy("Id", Disabled = true)]
    [OrderBy]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public List<Customer> Customers { get; set; }

        public List<Car> Cars { get; set; }
    }

    [OrderBy("Name", Disabled = true)]
    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }


    [OrderBy("Id")]
    [OrderBy(Disabled = true)]
    [OrderBy("Name")]
    public class Car
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CarNumber { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class OrderByAttributeEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Car>("Cars");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders").EntityType.OrderBy().OrderBy(QueryOptionSetting.Disabled, "Id");
            builder.EntitySet<Car>("Cars")
                .EntityType.OrderBy("Name")
                .OrderBy(QueryOptionSetting.Disabled)
                .OrderBy("Id");
            // Need call API just like Order for SepcialOrder because model bound API doesn't support inheritance
            builder.EntityType<SpecialOrder>()
                .OrderBy()
                .OrderBy(QueryOptionSetting.Disabled, "Id")
                .OrderBy(QueryOptionSetting.Disabled, "Name");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
