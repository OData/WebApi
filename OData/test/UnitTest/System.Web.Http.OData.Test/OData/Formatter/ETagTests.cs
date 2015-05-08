﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ETagTests
    {
        private readonly IList<Customer> _customers;

        public ETagTests()
        {
            _customers = new List<Customer>
            {
                new Customer
                {
                    ID = 1,
                    FirstName = "Foo",
                    LastName = "Bar",
                },
                new Customer
                {
                    ID = 2,
                    FirstName = "Abc",
                    LastName = "Xyz",
                },
            };
        }

        [Fact]
        public void GetValue_Returns_SetValue()
        {
            // Arrange
            ETag etag = new ETag();

            // Act & Assert
            etag["Name"] = "Name1";
            Assert.Equal("Name1", etag["Name"]);
        }

        [Fact]
        public void DynamicGetValue_Returns_DynamicSetValue()
        {
            // Arrange
            dynamic etag = new ETag();

            // Act & Assert
            etag.Name = "Name1";
            Assert.Equal("Name1", etag.Name);
        }

        [Fact]
        public void GetValue_ThrowsInvalidOperation_IfNotWellFormed()
        {
            // Arrange
            ETag etag = new ETag();
            etag["Name"] = "Name1";
            etag.IsWellFormed = false;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => etag["Name"], "The ETag is not well-formed.");
        }

        [Fact]
        public void DynamicGetValue_ThrowsInvalidOperation_IfNotWellFormed()
        {
            // Arrange
            ETag etag = new ETag();
            etag["Name"] = "Name1";
            etag.IsWellFormed = false;
            dynamic dynamicETag = etag;

            // Act && Assert
            Assert.Throws<InvalidOperationException>(() => dynamicETag.Name, "The ETag is not well-formed.");
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryable_SingleProperty()
        {
            // Arrange
            ETag etagCustomer = new ETag { EntityType = typeof(Customer) };
            dynamic etag = etagCustomer;
            etag.FirstName = "Foo";

            // Act     
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                new[] { 1 },
                actualCustomers.Select(customer => customer.ID));
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryable_MutipleProperties()
        {
            // Arrange
            ETag etagCustomer = new ETag { EntityType = typeof(Customer) };
            dynamic etag = etagCustomer;
            etag.FirstName = "Abc";
            etag.LastName = "Xyz";

            // Act     
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                new[] { 2 },
                actualCustomers.Select(customer => customer.ID));
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryable_NoMatch()
        {
            // Arrange
            ETag etagCustomer = new ETag { EntityType = typeof(Customer) };
            dynamic etag = etagCustomer;
            etag.FirstName = "foo";

            // Act     
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);

            Assert.Equal(new int[0], actualCustomers.Select(customer => customer.ID));
        }
    }
}
