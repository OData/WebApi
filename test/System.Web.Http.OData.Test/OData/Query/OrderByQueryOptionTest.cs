// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class OrderByQueryOptionTest
    {
        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OrderByQueryOption("Name", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new OrderByQueryOption(null, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new OrderByQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("''")]
        public void CanConstructValidFilterQuery(string orderbyValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var orderby = new OrderByQueryOption(orderbyValue, context);

            Assert.Same(context, orderby.Context);
            Assert.Equal(orderbyValue, orderby.RawValue);
        }

        [Fact]
        public void PropertyNodes_Getter_Parses_Query()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var orderby = new OrderByQueryOption("Name,Website", context);

            // Act
            ICollection<OrderByPropertyNode> nodes = orderby.PropertyNodes;

            // Assert
            Assert.NotNull(nodes);
            Assert.Equal(2, nodes.Count);
            Assert.Equal("Name", nodes.First().Property.Name);
            Assert.Equal("Website", nodes.Last().Property.Name);
        }

        [Theory]
        [InlineData("BadPropertyName")]
        [InlineData("''")]
        [InlineData(" ")]
        [InlineData("customerid")]
        [InlineData("CustomerId,CustomerId")]
        [InlineData("CustomerId,Name,CustomerId")]
        public void ApplyInValidOrderbyQueryThrows(string orderbyValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var orderby = new OrderByQueryOption(orderbyValue, context);

            Assert.Throws<ODataException>(() =>
                orderby.ApplyTo(ODataQueryOptionTest.Customers));
        }

        [Fact]
        [Trait("Description", "Can apply an orderby")]
        public void CanApplyOrderBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply an orderby")]
        public void CanApplyOrderByAsc()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name asc", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply an orderby descending")]
        public void CanApplyOrderByDescending()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name desc", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply a compound orderby")]
        public void CanApplyOrderByThenBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name,Website", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.net" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.com" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }
        [Fact]
        [Trait("Description", "Can apply a OrderByDescending followed by ThenBy")]
        public void CanApplyOrderByDescThenBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name desc,Website", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.net" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.com" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(3, results[0].CustomerId);
            Assert.Equal(1, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply a OrderByDescending followed by ThenBy")]
        public void CanApplyOrderByDescThenByDesc()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name desc,Website desc", new ODataQueryContext(model, typeof(Customer), "Customers"));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.net" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.com" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        public void ApplyToEnums_ReturnsCorrectQueryable()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumModel>("EnumModels");
            var model = builder.GetEdmModel();

            var context = new ODataQueryContext(model, typeof(EnumModel), "EnumModels");
            var orderbyOption = new OrderByQueryOption("Flag", context);
            IEnumerable<EnumModel> enumModels = FilterQueryOptionTest.EnumModelTestData;

            // Act
            IQueryable queryable = orderbyOption.ApplyTo(enumModels.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<EnumModel> actualCustomers = Assert.IsAssignableFrom<IEnumerable<EnumModel>>(queryable);
            Assert.Equal(
                new int[] { 2, 1, 3 },
                actualCustomers.Select(enumModel => enumModel.Id));
        }
    }
}
