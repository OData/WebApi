// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
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
                            FirstName = "Foo",
                            LastName = "Bar",
                        },
                    new Customer
                        {
                            ID = 2,
                            FirstName = "Abc",
                            LastName = "Xyz",
                        },
                    new Customer
                        {
                            ID = 3,
                            FirstName = "Def",
                            LastName = "Xyz",
                        },
                };
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryable()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer>();
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
            MethodCallExpression methodCall = queryable.Expression as MethodCallExpression;
            Assert.NotNull(methodCall);
            Assert.Equal(2, methodCall.Arguments.Count);
            Assert.Equal(@"Param_0 => (Param_0.FirstName == value(System.Web.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty)",
                methodCall.Arguments[1].ToString());
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_GivenQueryableAndIsIfNoneMatch()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer> { IsIfNoneMatch = true };
            dynamic etag = etagCustomer;
            etag.LastName = "Xyz";

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                new[] { 1 },
                actualCustomers.Select(customer => customer.ID));
            MethodCallExpression methodCall = queryable.Expression as MethodCallExpression;
            Assert.NotNull(methodCall);
            Assert.Equal(2, methodCall.Arguments.Count);
            Assert.Equal(
                @"Param_0 => Not((Param_0.LastName == value(System.Web.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty))",
                methodCall.Arguments[1].ToString());
        }

        [Fact]
        public void ApplyTo_NewQueryReturned_IsIfNoneMatchWithMultipleConcurrencyProperties()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer> { IsIfNoneMatch = true };
            dynamic etag = etagCustomer;
            etag.FirstName = "Def";
            etag.LastName = "Xyz";

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(_customers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                new[] { 1, 2 },
                actualCustomers.Select(customer => customer.ID));
            MethodCallExpression methodCall = queryable.Expression as MethodCallExpression;
            Assert.NotNull(methodCall);
            Assert.Equal(2, methodCall.Arguments.Count);
            Assert.Equal(
                @"Param_0 => Not(((Param_0.FirstName == value(System.Web.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty) "
                + "AndAlso (Param_0.LastName == value(System.Web.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty)))",
                methodCall.Arguments[1].ToString());
        }

        [Fact]
        public void ApplyToNonGenericThrows_IfPassedAnIQueryableOfTheWrongType()
        {
            // Arrange
            ETag<Customer> etagCustomer = new ETag<Customer> { IsIfNoneMatch = true };
            IQueryable query = Enumerable.Empty<int>().AsQueryable();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => etagCustomer.ApplyTo(query),
                "Cannot apply ETag of 'System.Web.OData.Formatter.Serialization.Models.Customer' to IQueryable of " +
                "'System.Int32'.\r\nParameter name: query");
        }
    }
}
