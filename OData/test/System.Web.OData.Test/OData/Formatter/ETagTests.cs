// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
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
                    new Customer
                        {
                            ID = 3,
                            FirstName = "Def",
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

            // Act && Assert
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
        public void ApplyTo_NewQueryReturned_GivenQueryable()
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
            ETag etagCustomer = new ETag { EntityType = typeof(Customer), IsIfNoneMatch = true };
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
            ETag etagCustomer = new ETag { EntityType = typeof(Customer), IsIfNoneMatch = true };
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
        public void ApplyTo_SameQueryReturned_GivenQueryableAndETagAny()
        {
            // Arrange
            var any = new ETag { IsAny = true };
            var customers = _customers.AsQueryable();

            // Act
            var queryable = any.ApplyTo(customers);

            // Assert
            Assert.NotNull(queryable);
            Assert.Same(queryable, customers);
        }
    }
}
