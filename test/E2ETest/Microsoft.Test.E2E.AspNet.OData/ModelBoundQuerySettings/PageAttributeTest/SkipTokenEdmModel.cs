// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public IList<OrderDetail> Details { get; set; }

        public int Price { get; set; }
        [Page]
        public List<Customer> Customers { get; set; }
    }

    [Page(PageSize = 2)]
    public class OrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
    }

    public class SpecialOrder : Order
    {
        public string SpecialName { get; set; }
    }

    [Page(PageSize = 2)]
    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }

    [Page(PageSize = 2)]
    public class Date
    {
        [Key]
        public DateTime DateValue { get; set; }
    }

    [Page(PageSize = 2)]
    public class DateOffset
    {
        [Key]
        public DateTimeOffset DateValue { get; set; }
    }

    public class SkipTokenEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<OrderDetail>("Details");
            builder.EntitySet<Date>("Dates");
            builder.EntitySet<DateOffset>("DateOffsets");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
