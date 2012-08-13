// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.OData.Query
{
    public class FilterQueryOptionTest
    {
        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FilterQueryOption("Name eq 'MSFT'", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(null, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Theory]
        [InlineData("Name eq 'MSFT'")]
        [InlineData("''")]
        public void CanConstructValidFilterQuery(string filterValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption(filterValue, context);

            Assert.Same(context, filter.Context);
            Assert.Equal(filterValue, filter.RawValue);
        }

        [Fact]
        public void GetQueryNodeParsesQuery()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Name eq 'MSFT'", context);
            var node = filter.QueryNode;

            Assert.Equal(QueryNodeKind.BinaryOperator, node.Expression.Kind);
            var binaryNode = node.Expression as BinaryOperatorQueryNode;
            Assert.Equal(BinaryOperatorKind.Equal, binaryNode.OperatorKind);
            Assert.Equal(QueryNodeKind.Constant, binaryNode.Right.Kind);
            Assert.Equal("MSFT", ((ConstantQueryNode)binaryNode.Right).Value);
            Assert.Equal(QueryNodeKind.PropertyAccess, binaryNode.Left.Kind);
            var propertyAccessNode = binaryNode.Left as PropertyAccessQueryNode;
            Assert.Equal("Name", propertyAccessNode.Property.Name);
        }
    }
}
