//-----------------------------------------------------------------------------
// <copyright file="FilterAttributeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.FilterAttributeTest
{
    [Filter("AutoExpandOrder", "Address")]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order AutoExpandOrder { get; set; }

        [Filter]
        public Address Address { get; set; }

        [Filter("Id")]
        public List<Order> Orders { get; set; }
    }

    [Filter("Id", Disabled = true)]
    [Filter]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [Filter]
        public List<Customer> Customers { get; set; }

        public List<Customer> UnFilterableCustomers { get; set; }

        public List<Car> Cars { get; set; }
    }

    [Filter("Name", Disabled = true)]
    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }

    [Filter("Id")]
    [Filter(Disabled = true)]
    [Filter("Name")]
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

    [Filter("Books")]
    public class Author
    {
        public long AuthorId { get; set; }
        public List<Book> Books { get; set; }
    }

    [Filter("BookId")]
    public class Book
    {
        public long BookId { get; set; }
    }

    public class FilterAttributeEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Car>("Cars");
            builder.EntitySet<Author>("Authors");
            builder.EntitySet<Book>("Books");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers").EntityType.Filter("AutoExpandOrder", "Address");
            builder.EntityType<Customer>().HasMany(c => c.Orders).Filter("Id");
            builder.EntityType<Customer>().ComplexProperty(c => c.Address).Filter();

            builder.EntitySet<Order>("Orders").EntityType.Filter().Filter(QueryOptionSetting.Disabled, "Id");
            builder.EntityType<Order>().HasMany(o => o.Customers).Filter();

            builder.EntitySet<Car>("Cars")
                .EntityType.Filter("Name")
                .Filter(QueryOptionSetting.Disabled)
                .Filter("Id");
            // Need call API just like Order for SepcialOrder because model bound API doesn't support inheritance
            builder.EntityType<SpecialOrder>()
                .Filter()
                .Filter(QueryOptionSetting.Disabled, "Id")
                .Filter(QueryOptionSetting.Disabled, "Name");

            builder.EntitySet<Author>("Authors").EntityType.Filter("Books");
            builder.EntitySet<Book>("Books").EntityType.Filter("BookId");

            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
