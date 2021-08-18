//-----------------------------------------------------------------------------
// <copyright file="PageAttributeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest
{
    [Page(MaxTop = 5, PageSize = 2)]
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }
        
        [Page(MaxTop = 2, PageSize = 2)]
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
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public static IEdmModel GetEdmModelByModelBoundAPI(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers")
                .EntityType.Page(5, 2);
            builder.EntityType<Customer>().HasMany(p => p.Orders).Page(2, 2);

            builder.EntitySet<Order>("Orders");
            builder.EntityType<Order>().HasMany(p => p.Customers).Page();
            builder.EntityType<SpecialOrder>().Page(5, null);
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
