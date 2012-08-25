// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;

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

        [Fact(Skip="Enable once Uri Parser sets parameters of Any/All bound to CollectionProperty correctly")]
        public void CanConstructValidAnyQueryOverPrimitiveCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Aliases/any(a: a eq 'alias')", context);
            var node = filter.QueryNode;
            var anyNode = node.Expression as AnyQueryNode;
            var aParameter = anyNode.Parameters.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.ParameterType.Definition as IEdmPrimitiveType;

            Assert.NotNull(aParameter);
            Assert.NotNull(aParameterType);
            Assert.Equal("String", aParameter.Name);            
        }

        [Fact(Skip = "Enable once Uri Parser sets parameters of Any/All bound to CollectionProperty correctly")]
        public void CanConstructValidAnyQueryOverComplexCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);
            var node = filter.QueryNode;
            var anyNode = node.Expression as AnyQueryNode;
            var aParameter = anyNode.Parameters.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.ParameterType.Definition as IEdmComplexType;

            Assert.NotNull(aParameter);
            Assert.NotNull(aParameterType);
            Assert.Equal("Address", aParameter.Name); 
        }
    }
}
