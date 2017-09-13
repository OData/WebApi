// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest
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
