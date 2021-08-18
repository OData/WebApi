//-----------------------------------------------------------------------------
// <copyright file="ETagTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter
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
            ExceptionAssert.Throws<InvalidOperationException>(() => etag["Name"], "The ETag is not well-formed.");
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
            ExceptionAssert.Throws<InvalidOperationException>(() => dynamicETag.Name, "The ETag is not well-formed.");
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
            Assert.Equal(@"Param_0 => (Param_0.FirstName == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty)",
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
                @"Param_0 => Not((Param_0.LastName == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty))",
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
                @"Param_0 => Not(((Param_0.FirstName == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty) "
                + "AndAlso (Param_0.LastName == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.String]).TypedProperty)))",
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

        [Theory]
        [InlineData(1.0, true, new[] { 1, 3 })]
        [InlineData(1.0, false, new[] { 2 })]
        [InlineData(1.1, true, null)]
        [InlineData(1.1, false, new[] { 1, 2, 3 })]
        public void ApplyTo_NewQueryReturned_ForDouble(double value, bool ifMatch, IList<int> expect)
        {
            // Arrange
            var myCustomers = new List<MyETagCustomer>
            {
                new MyETagCustomer
                {
                    ID = 1,
                    DoubleETag = 1.0,
                },
                new MyETagCustomer
                {
                    ID = 2,
                    DoubleETag = 1.1,
                },
                new MyETagCustomer
                {
                    ID = 3,
                    DoubleETag = 1.0,
                },
            };

            IETagHandler handerl = new DefaultODataETagHandler();
            Dictionary<string, object> properties = new Dictionary<string, object> { { "DoubleETag", value } };
            EntityTagHeaderValue etagHeaderValue = handerl.CreateETag(properties);

            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MyETagCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customer = model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "MyEtagCustomer");
            IEdmEntitySet customers = model.FindDeclaredEntitySet("Customers");
            ODataPath odataPath = new ODataPath(new[] { new EntitySetSegment(customers) });
            var request = RequestFactory.CreateFromModel(model);
            request.ODataContext().Path = odataPath;

            ETag etagCustomer = request.GetETag(etagHeaderValue);
            etagCustomer.EntityType = typeof(MyETagCustomer);
            etagCustomer.IsIfNoneMatch = !ifMatch;

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(myCustomers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IList<MyETagCustomer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<MyETagCustomer>>(queryable).ToList();
            if (expect != null)
            {
                Assert.Equal(expect, actualCustomers.Select(c => c.ID));
            }

            MethodCallExpression methodCall = queryable.Expression as MethodCallExpression;
            Assert.NotNull(methodCall);
            Assert.Equal(2, methodCall.Arguments.Count);
            if (ifMatch)
            {
                Assert.Equal(
                    "Param_0 => (Param_0.DoubleETag == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Double]).TypedProperty)",
                    methodCall.Arguments[1].ToString());
            }
            else
            {
                Assert.Equal(
                    "Param_0 => Not((Param_0.DoubleETag == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Double]).TypedProperty))",
                    methodCall.Arguments[1].ToString());
            }
        }

        public class MyETagCustomer
        {
            public int ID { get; set; }

            [ConcurrencyCheck]
            public double DoubleETag { get; set; }
        }

        [Theory]
        [InlineData((sbyte)1, (short)1, true, new int[] {})]
        [InlineData((sbyte)1, (short)1, false, new[] { 1, 2, 3 })]
        [InlineData(SByte.MaxValue, Int16.MaxValue, true, new[] { 2 })]
        [InlineData(SByte.MaxValue, Int16.MaxValue, false, new[] { 1, 3 })]
        [InlineData(SByte.MinValue, Int16.MinValue, true, new[] { 3 })]
        [InlineData(SByte.MinValue, Int16.MinValue, false, new[] { 1, 2 })]
        public void ApplyTo_NewQueryReturned_ForInteger(sbyte byteVal, short shortVal, bool ifMatch, IList<int> expect)
        {
            // Arrange
            var mycustomers = new List<MyETagOrder>
            {
                new MyETagOrder
                {
                    ID = 1,
                    ByteVal = 7,
                    ShortVal = 8
                },
                new MyETagOrder
                {
                    ID = 2,
                    ByteVal = SByte.MaxValue,
                    ShortVal = Int16.MaxValue
                },
                new MyETagOrder
                {
                    ID = 3,
                    ByteVal = SByte.MinValue,
                    ShortVal = Int16.MinValue
                },
            };
            IETagHandler handerl = new DefaultODataETagHandler();
            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                { "ByteVal", byteVal },
                { "ShortVal", shortVal }
            };
            EntityTagHeaderValue etagHeaderValue = handerl.CreateETag(properties);

            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MyETagOrder>("Orders");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet orders = model.FindDeclaredEntitySet("Orders");
            ODataPath odataPath = new ODataPath(new[] {new EntitySetSegment(orders) });
            var request = RequestFactory.CreateFromModel(model);
            request.ODataContext().Path = odataPath;

            ETag etagCustomer = request.GetETag(etagHeaderValue);
            etagCustomer.EntityType = typeof(MyETagOrder);
            etagCustomer.IsIfNoneMatch = !ifMatch;

            // Act
            IQueryable queryable = etagCustomer.ApplyTo(mycustomers.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<MyETagOrder> actualOrders = Assert.IsAssignableFrom<IEnumerable<MyETagOrder>>(queryable);
            Assert.Equal(expect, actualOrders.Select(c => c.ID));
            MethodCallExpression methodCall = queryable.Expression as MethodCallExpression;
            Assert.NotNull(methodCall);
            Assert.Equal(2, methodCall.Arguments.Count);

            if (ifMatch)
            {
                Assert.Equal(
                    "Param_0 => ((Param_0.ByteVal == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.SByte]).TypedProperty) " +
                    "AndAlso (Param_0.ShortVal == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Int16]).TypedProperty))",
                    methodCall.Arguments[1].ToString());
            }
            else
            {
                Assert.Equal(
                    "Param_0 => Not(((Param_0.ByteVal == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.SByte]).TypedProperty) " +
                    "AndAlso (Param_0.ShortVal == value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Int16]).TypedProperty)))",
                    methodCall.Arguments[1].ToString());
            }
        }

        public class MyETagOrder
        {
            public int ID { get; set; }

            [ConcurrencyCheck]
            public sbyte ByteVal { get; set; }

            [ConcurrencyCheck]
            public short ShortVal { get; set; }
        }
    }
}
