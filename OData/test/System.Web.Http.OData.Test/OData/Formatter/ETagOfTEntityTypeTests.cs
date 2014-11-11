// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ETagOfTEntityTypeTests
    {
        private readonly IList<Customer> _customers;

        public ETagOfTEntityTypeTests()
        {
            _customers = new List<Customer>
            {
                new Customer
                {
                    ID = 1,
                    FirstName = "TestFName",
                    LastName = "TestLName",
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
        public void ApplyTo_NewQueryReturned_GivenQueryable_SingleProperty()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer>();
            dynamic etag = etagCustomer;
            etag.FirstName = "TestFName";

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(new[] { 1 }, actualCustomers.Select(customer => customer.ID));
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryable_MutipleProperties()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer>();
            dynamic etag = etagCustomer;
            etag.FirstName = "TestFName";
            etag.LastName = "TestLName";

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(new[] { 1 }, actualCustomers.Select(customer => customer.ID));
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryable_NoMatch()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer>();
            dynamic etag = etagCustomer;
            etag.FirstName = "TestFName";
            etag.LastName = "testLName";

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(new int[0], actualCustomers.Select(customer => customer.ID));
        }
    }
}
