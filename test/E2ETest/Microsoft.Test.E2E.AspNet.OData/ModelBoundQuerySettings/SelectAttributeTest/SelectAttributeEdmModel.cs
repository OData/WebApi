//-----------------------------------------------------------------------------
// <copyright file="SelectAttributeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.SelectAttributeTest
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        [Select("Id")]
        public List<Order> Orders { get; set; }
    }

    [Select("Id", SelectType = SelectExpandType.Disabled)]
    [Select]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [Select]
        public List<Customer> Customers { get; set; }

        public List<Car> Cars { get; set; }
    }

    [Select("Name", SelectType = SelectExpandType.Disabled)]
    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }


    [Select("Id")]
    [Select("Name")]
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

    [Select("Name", SelectType = SelectExpandType.Automatic)]
    [Expand("Order", ExpandType = SelectExpandType.Automatic)]
    public class AutoSelectCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Select("Name", SelectType = SelectExpandType.Automatic)]
        public AutoSelectOrder Order { get; set; }

        public AutoSelectCar Car { get; set; }
    }

    [Select("VIPNumber", SelectType = SelectExpandType.Automatic)]
    public class SpecialCustomer : AutoSelectCustomer
    {
        public string VIPNumber { get; set; }
    }

    [Select("Id", SelectType = SelectExpandType.Automatic)]
    public class AutoSelectOrder
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public AutoSelectCustomer Customer { get; set; }
    }

    [Select(SelectType = SelectExpandType.Automatic)]
    [Select("Name", SelectType = SelectExpandType.Disabled)]
    public class AutoSelectCar
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CarNumber { get; set; }
    }

    public class SelectAttributeEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Car>("Cars");
            builder.EntitySet<AutoSelectCustomer>("AutoSelectCustomers");
            builder.EntitySet<AutoSelectOrder>("AutoSelectOrders");
            builder.EntitySet<AutoSelectCar>("AutoSelectCars");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntityType<Customer>().HasMany(c => c.Orders).Select("Id");

            builder.EntitySet<Order>("Orders").EntityType.Select().Select(SelectExpandType.Disabled, "Id");
            builder.EntityType<Order>().HasMany(o => o.Customers).Select();

            builder.EntitySet<Car>("Cars")
                .EntityType.Select("Name")
                .Select("Id");

            // Need call API just like Order for SepcialOrder because model bound API doesn't support inheritance
            builder.EntityType<SpecialOrder>()
                .Select()
                .Select(SelectExpandType.Disabled, "Id")
                .Select(SelectExpandType.Disabled, "Name");

            builder.EntitySet<AutoSelectCustomer>("AutoSelectCustomers")
                .EntityType.Select(SelectExpandType.Automatic, "Name")
                .Expand(SelectExpandType.Automatic, "Order");

            builder.EntityType<SpecialCustomer>()
                .Select(SelectExpandType.Automatic, "Name", "VIPNumber")
                .Expand(SelectExpandType.Automatic, "Order");

            builder.EntityType<AutoSelectCustomer>()
                .HasOptional(c => c.Order)
                .Select(SelectExpandType.Automatic, "Name");

            builder.EntitySet<AutoSelectOrder>("AutoSelectOrders")
                .EntityType.Select(SelectExpandType.Automatic, "Id");

            builder.EntitySet<AutoSelectCar>("AutoSelectCars")
                .EntityType.Select(SelectExpandType.Disabled, "Name")
                .Select(SelectExpandType.Automatic);

            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
