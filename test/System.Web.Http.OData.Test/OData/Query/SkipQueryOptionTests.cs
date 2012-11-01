// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class SkipQueryOptionTests
    {
        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SkipQueryOption("1", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new SkipQueryOption(null, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new SkipQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Theory]
        [InlineData("2")]
        [InlineData("100")]
        [InlineData("0")]
        [InlineData("-1")]
        public void CanConstructValidFilterQuery(string skipValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var skip = new SkipQueryOption(skipValue, context);

            Assert.Same(context, skip.Context);
            Assert.Equal(skipValue, skip.RawValue);
        }

        [Theory]
        [InlineData("NotANumber")]
        [InlineData("''")]
        [InlineData(" ")]
        public void ApplyInValidSkipQueryThrows(string skipValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var skip = new SkipQueryOption(skipValue, context);

            Assert.Throws<ODataException>(() =>
                skip.ApplyTo(ODataQueryOptionTest.Customers));
        }

        [Theory]
        [InlineData("-1", -1)]
        [InlineData("0", 0)]
        [InlineData("100", 100)]
        public void Value_Returns_ParsedSkipValue(string skipValue, int expectedValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var skip = new SkipQueryOption(skipValue, context);

            Assert.Equal(expectedValue, skip.Value);
        }

        [Theory]
        [InlineData("NotANumber")]
        [InlineData("''")]
        [InlineData(" ")]
        public void Value_ThrowsODataException_ForInvalidValues(string skipValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var skip = new SkipQueryOption(skipValue, context);

            Assert.Throws<ODataException>(() => skip.Value);
        }

        [Fact]
        public void CanApplySkip()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var skipOption = new SkipQueryOption("1", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = skipOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results.Length);
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
        }

        [Fact]
        public void CanApplySkipOrderby()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var orderbyOption = new OrderByQueryOption("Name", context);
            var skipOption = new SkipQueryOption("1", context);

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            IQueryable queryable = orderbyOption.ApplyTo(customers);
            queryable = skipOption.ApplyTo(queryable);
            var results = ((IQueryable<Customer>)queryable).ToArray();
            Assert.Equal(2, results.Length);
            Assert.Equal(3, results[0].CustomerId);
            Assert.Equal(1, results[1].CustomerId);
        }
    }
}
