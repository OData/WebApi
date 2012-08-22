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
    public class UriParserTest
    {
        [Fact]
        [Trait("Description", "Can syntactically parse an OData URL")]
        public void CanBuildSyntacticTree()
        {
            var serviceBaseUri = new Uri("http://server/service/");
            var queryUri = new Uri(serviceBaseUri, "Customers(1)");
            var syntacticTree = SyntacticTree.ParseUri(queryUri, serviceBaseUri);
        }

        [Fact]
        [Trait("Description", "Can semantically parse an OData URL")]
        public void CanBuildSemanticTree()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var serviceBaseUri = new Uri("http://server/service/");
            var queryUri = new Uri(serviceBaseUri, "Customers(1)");
            var semanticTree = SemanticTree.ParseUri(queryUri, serviceBaseUri, model);
        }

        [Fact]
        [Trait("Description", "Can syntactically parse and then seperately bind")]
        public void ParseThenBind()
        {
            var serviceBaseUri = new Uri("http://server/service/");
            var queryUri = new Uri(serviceBaseUri, "Customers(1)");
            var syntacticTree = SyntacticTree.ParseUri(queryUri, serviceBaseUri);
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            MetadataBinder binder = new MetadataBinder(model);
            var semanticTree = binder.BindQuery(syntacticTree);
        }

        [Fact]
        [Trait("Description", "Can parse a simple filter")]
        public void ParseSimpleFilter()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var serviceBaseUri = new Uri("http://server/service/");
            var queryUri = new Uri(serviceBaseUri, "Customers?$filter=Name eq 'Alex'");
            var semanticTree = SemanticTree.ParseUri(queryUri, serviceBaseUri, model);
            var filterNode = semanticTree.Query as FilterQueryNode;
            Assert.NotNull(filterNode);
            var equalityNode = filterNode.Expression as BinaryOperatorQueryNode;
            Assert.NotNull(equalityNode);
            Assert.Equal(BinaryOperatorKind.Equal, equalityNode.OperatorKind);
            var leftNode = equalityNode.Left as PropertyAccessQueryNode;
            Assert.NotNull(leftNode);
            Assert.Equal("Name", leftNode.Property.Name);
            var rightNode = equalityNode.Right as ConstantQueryNode;
            Assert.NotNull(rightNode);
            Assert.Equal("Alex", rightNode.Value);
        }

        [Fact]
        [Trait("Description", "Can parse a simple orderby")]
        public void ParseSimpleOrderBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var serviceBaseUri = new Uri("http://server/service/");
            var queryUri = new Uri(serviceBaseUri, "Customers?$orderby=Name");
            var semanticTree = SemanticTree.ParseUri(queryUri, serviceBaseUri, model);
            var orderByNode = semanticTree.Query as OrderByQueryNode;
            Assert.NotNull(orderByNode);
            Assert.Equal(QueryNodeKind.PropertyAccess, orderByNode.Expression.Kind);
            var propertyAccess = orderByNode.Expression as PropertyAccessQueryNode;
            Assert.NotNull(propertyAccess);
            var customerEntityType = model.FindDeclaredType(typeof(Customer).FullName) as IEdmEntityType;
            var nameProperty = customerEntityType.Properties().Single(p => p.Name == "Name");
            Assert.Equal(nameProperty, propertyAccess.Property);
        }

        [Fact]
        [Trait("Description", "Can parse a simple orderby")]
        public void ParseCompoundOrderBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var serviceBaseUri = new Uri("http://server/service/");
            var queryUri = new Uri(serviceBaseUri, "Customers?$orderby=Name,Website desc");
            var semanticTree = SemanticTree.ParseUri(queryUri, serviceBaseUri, model);

            var thenOrderByNode = semanticTree.Query as OrderByQueryNode;
            Assert.NotNull(thenOrderByNode);
            var orderByNode = thenOrderByNode.Collection as OrderByQueryNode;
            Assert.NotNull(orderByNode);
            Assert.Equal("Name", (orderByNode.Expression as PropertyAccessQueryNode).Property.Name);
            Assert.Equal("Website", (thenOrderByNode.Expression as PropertyAccessQueryNode).Property.Name);
        }
    }
}
