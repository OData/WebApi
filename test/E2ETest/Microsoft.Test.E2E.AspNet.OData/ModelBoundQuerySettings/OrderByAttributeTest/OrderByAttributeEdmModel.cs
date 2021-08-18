//-----------------------------------------------------------------------------
// <copyright file="OrderByAttributeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.OrderByAttributeTest
{
    [OrderBy("AutoExpandOrder", "Address")]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order AutoExpandOrder { get; set; }

        [OrderBy]
        public Address Address { get; set; }

        [OrderBy("Id")]
        public List<Order> Orders { get; set; }
    }

    [OrderBy("Id", Disabled = true)]
    [OrderBy]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [OrderBy]
        public List<Customer> Customers { get; set; }

        public List<Customer> UnSortableCustomers { get; set; }

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
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Car>("Cars");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers").EntityType.OrderBy("AutoExpandOrder", "Address");
            builder.EntityType<Customer>().HasMany(c => c.Orders).OrderBy("Id");
            builder.EntityType<Customer>().ComplexProperty(c => c.Address).OrderBy();

            builder.EntitySet<Order>("Orders").EntityType.OrderBy().OrderBy(QueryOptionSetting.Disabled, "Id");
            builder.EntityType<Order>().HasMany(o => o.Customers).OrderBy();

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
