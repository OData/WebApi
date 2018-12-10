// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest.SkipTokenTest
{
    [Page(PageSize = 2)]
    public class Customer
    {
        public Customer()
        {
            DynamicProperties = new Dictionary<string, object>();
        }
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }

        public Guid Token { get; set; }

        public Enums.Skill Skill { get; set; }

        public DateTimeOffset DateTimeOfBirth { get; set; }

        [Page(PageSize = 2)]
        public List<Order> Orders { get; set; }

        public List<Address> Addresses { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }

    }

    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }
        [Page]
        public List<Customer> Customers { get; set; }
    }

    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }

    [Page(PageSize =2)]
    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    public class SkipTokenEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
