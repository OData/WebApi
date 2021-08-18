//-----------------------------------------------------------------------------
// <copyright file="CountAttributeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CountAttributeTest
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        [Count(Disabled = true)]
        public List<Order> Orders { get; set; }

        public List<Address> Addresses { get; set; }

        public List<Address2> Addresses2 { get; set; }

        public List<Order> CountableOrders { get; set; } 
    }

    [Count]
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }
    }

    [Count(Disabled = true)]
    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    [Count]
    public class Address2
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class CountAttributeEdmModel
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
                .EntityType.HasMany(p => p.Orders)
                .Count(QueryOptionSetting.Disabled);
            builder.EntitySet<Order>("Orders").EntityType.Count();
            builder.EntityType<SpecialOrder>().Count(QueryOptionSetting.Disabled);
            builder.ComplexType<Address2>().Count();
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
