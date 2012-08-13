// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Xunit;
using Xunit.Extensions;

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
        public void GetQueryNodeParsesQuery()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var orderby = new OrderByQueryOption("Name,Website", context);
            var node = orderby.QueryNode;

            Assert.Equal(QueryNodeKind.PropertyAccess, node.Expression.Kind);
            var websiteNode = node.Expression as PropertyAccessQueryNode;
            Assert.Equal("Website", websiteNode.Property.Name);

            var nameNode = ((OrderByQueryNode)node.Collection).Expression;
            Assert.Equal(QueryNodeKind.PropertyAccess, nameNode.Kind);
            Assert.Equal("Name", ((PropertyAccessQueryNode)nameNode).Property.Name);
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
                new Customer { CustomerId = 1, Name = "Andy"  },
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
                new Customer { CustomerId = 1, Name = "Andy"  },
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
                new Customer { CustomerId = 1, Name = "Andy"  },
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
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.co.uk" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.co.nz" }
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
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.co.uk" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.co.nz" }
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
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.co.uk" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.co.nz" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }
    }
}
