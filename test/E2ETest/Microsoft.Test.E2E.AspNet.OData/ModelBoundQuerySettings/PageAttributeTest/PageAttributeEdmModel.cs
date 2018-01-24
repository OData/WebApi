// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest
{
    [Page(MaxTop = 5, PageSize = 1)]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }
        
        [Page(MaxTop = 2, PageSize = 1)]
        public List<Order> Orders { get; set; }

        public List<Address> Addresses { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [Page]
        public List<Customer> Customers { get; set; }
    }

    [Page(MaxTop = 5)]
    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class PageAttributeEdmModel
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
                .EntityType.Page(5, 1);
            builder.EntityType<Customer>().HasMany(p => p.Orders).Page(2, 1);

            builder.EntitySet<Order>("Orders");
            builder.EntityType<Order>().HasMany(p => p.Customers).Page();
            builder.EntityType<SpecialOrder>().Page(5, null);
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
