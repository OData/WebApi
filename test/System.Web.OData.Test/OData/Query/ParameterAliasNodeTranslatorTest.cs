// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Query
{
    public class ParameterAliasNodeTranslatorTest
    {
        private IEdmModel _model;
        private IEdmEntitySet _customersEntitySet;
        private IEdmEntityType _customerEntityType;
        private SingleValueNode _parameterAliasMappedNode;

        public ParameterAliasNodeTranslatorTest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ParameterAliasCustomer>("Customers");
            builder.EntitySet<ParameterAliasOrder>("Orders");

            builder.EntityType<ParameterAliasCustomer>().Function("CollectionFunctionCall")
                .ReturnsCollection<int>().Parameter<int>("p1");

            builder.EntityType<ParameterAliasCustomer>().Function("EntityCollectionFunctionCall")
                .ReturnsCollectionFromEntitySet<ParameterAliasCustomer>("Customers").Parameter<int>("p1");

            builder.EntityType<ParameterAliasCustomer>().Function("SingleEntityFunctionCall")
                .Returns<ParameterAliasCustomer>().Parameter<int>("p1");

            builder.EntityType<ParameterAliasCustomer>().Function("SingleValueFunctionCall")
                .Returns<int>().Parameter<int>("p1");

            _model = builder.GetEdmModel();
            _customersEntitySet = _model.FindDeclaredEntitySet("Customers");
            _customerEntityType = _customersEntitySet.EntityType();
            _parameterAliasMappedNode = new ConstantNode(123);
        }

        [Fact]
        public void Constructor_Throws_NullParameterAliasNodes()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ParameterAliasNodeTranslator(null),
                "parameterAliasNodes");
        }

        [Fact]
        public void ReturnsNull_ParameterAliasNotFound()
        {
            // Arrange
            var translator = new ParameterAliasNodeTranslator(new Dictionary<string, SingleValueNode>());
            var parameterAliasNode = new ParameterAliasNode("@unknown", null);

            // Act
            QueryNode translatedNode = parameterAliasNode.Accept(translator);

            // Assert
            var constantNode = Assert.IsType<ConstantNode>(translatedNode);
            Assert.Null(constantNode.Value);
        }

        [Fact]
        public void CanTranslate_ParameterAliasNode()
        {
            // Arrange
            var translator = new ParameterAliasNodeTranslator(
                new Dictionary<string, SingleValueNode> { { "@p", _parameterAliasMappedNode } });
            var parameterAliasNode = new ParameterAliasNode("@p", null);

            // Act
            QueryNode translatedNode = parameterAliasNode.Accept(translator);

            // Assert
            Assert.Same(_parameterAliasMappedNode, translatedNode);
        }

        [Fact]
        public void CanTranslate_AllNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Orders/all(order: order/ID eq @p)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(allNode.Body);
            var convertNode = Assert.IsType<ConvertNode>(binaryOperatorNode.Right);
            Assert.Same(_parameterAliasMappedNode, convertNode.Source);
        }

        [Fact]
        public void CanTranslate_AnyNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Orders/any(order: @p ne order/ID)");

            // Assert
            var anyNode = Assert.IsType<AnyNode>(translatedNode);
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(anyNode.Body);
            var convertNode = Assert.IsType<ConvertNode>(binaryOperatorNode.Left);
            Assert.Same(_parameterAliasMappedNode, convertNode.Source);
        }

        [Fact]
        public void CanTranslate_CollectionFunctionCallNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Default.CollectionFunctionCall(p1=@p)/all(r : r ne null)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var collectionFunctionCallNode = Assert.IsType<CollectionFunctionCallNode>(allNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(collectionFunctionCallNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
            namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(collectionFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_CollectionNavigationNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Orders/all(order : order ne null)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var collectionNavigationNode = Assert.IsType<CollectionNavigationNode>(allNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(collectionNavigationNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_CollectionOpenPropertyAccessNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/OpenComplex/CollectionProperty/all(p : p ne null)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var collectionOpenPropertyAccessNode = Assert.IsType<CollectionOpenPropertyAccessNode>(allNode.Source);
            var singleValuePropertyAccessNode = Assert.IsType<SingleValuePropertyAccessNode>(collectionOpenPropertyAccessNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleValuePropertyAccessNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_CollectionPropertyAccessNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Addresses/all(a : a ne null)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var collectionPropertyAccessNode = Assert.IsType<CollectionPropertyAccessNode>(allNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(collectionPropertyAccessNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_CollectionPropertyCastNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Addresses/System.Web.OData.Query.ParameterAliasAddress/all(a : a ne null)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var collectionPropertyCastNode = Assert.IsType<CollectionPropertyCastNode>(allNode.Source);
            var collectionPropertyAccessNode = Assert.IsType<CollectionPropertyAccessNode>(collectionPropertyCastNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(collectionPropertyAccessNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_EntityCollectionCastNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Orders/System.Web.OData.Query.ParameterAliasOrder/all(o : o ne null)");

            // Assert
            var allNode = Assert.IsType<AllNode>(translatedNode);
            var entityCollectionCastNode = Assert.IsType<EntityCollectionCastNode>(allNode.Source);
            var collectionNavigationNode = Assert.IsType<CollectionNavigationNode>(entityCollectionCastNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(collectionNavigationNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_EntityCollectionFunctionCallNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Default.EntityCollectionFunctionCall(p1=@p)/any(r : r eq null)");

            // Assert
            var anyNode = Assert.IsType<AnyNode>(translatedNode);
            var entityCollectionFunctionCallNode = Assert.IsType<EntityCollectionFunctionCallNode>(anyNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(entityCollectionFunctionCallNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
            namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(entityCollectionFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleEntityCastNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/System.Web.OData.Query.ParameterAliasCustomer eq null");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var singleEntityCastNode = Assert.IsType<SingleEntityCastNode>(binaryOperatorNode.Left);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleEntityCastNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleEntityFunctionCallNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/Default.SingleEntityFunctionCall(p1=@p) eq null");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(binaryOperatorNode.Left);
            var singleEntityFunctionCallSourceNode = Assert.IsType<SingleEntityFunctionCallNode>(singleEntityFunctionCallNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallSourceNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
            namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleNavigationNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/SpecialOrder eq null");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var singleNavigationNode = Assert.IsType<SingleNavigationNode>(binaryOperatorNode.Left);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleNavigationNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleValueCastNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/SpecialAddress/System.Web.OData.Query.ParameterAliasAddress eq null");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var singleValueCastNode = Assert.IsType<SingleValueCastNode>(binaryOperatorNode.Left);
            var singleValuePropertyAccessNode = Assert.IsType<SingleValuePropertyAccessNode>(singleValueCastNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleValuePropertyAccessNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleValueFunctionCallNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("null ne Default.SingleEntityFunctionCall(p1=@p)/Default.SingleValueFunctionCall(p1=@p)");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var convertNode = Assert.IsType<ConvertNode>(binaryOperatorNode.Right);
            var singleValueFunctionCallNode = Assert.IsType<SingleValueFunctionCallNode>(convertNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleValueFunctionCallNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
            namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleValueFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleValueOpenPropertyAccessNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/OpenComplex/Unknown ne null");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var singleValueOpenPropertyAccessNode = Assert.IsType<SingleValueOpenPropertyAccessNode>(binaryOperatorNode.Left);
            var singleValuePropertyAccessNode = Assert.IsType<SingleValuePropertyAccessNode>(singleValueOpenPropertyAccessNode.Source);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleValuePropertyAccessNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_SingleValuePropertyAccessNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("Default.SingleEntityFunctionCall(p1=@p)/SpecialAddress ne null");

            // Assert
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(translatedNode);
            var singleValuePropertyAccessNode = Assert.IsType<SingleValuePropertyAccessNode>(binaryOperatorNode.Left);
            var singleEntityFunctionCallNode = Assert.IsType<SingleEntityFunctionCallNode>(singleValuePropertyAccessNode.Source);
            var namedFunctionParameterNode = Assert.IsType<NamedFunctionParameterNode>(singleEntityFunctionCallNode.Parameters.Single());
            Assert.Same(_parameterAliasMappedNode, namedFunctionParameterNode.Value);
        }

        [Fact]
        public void CanTranslate_UnaryOperatorNode()
        {
            // Arrange & Act
            QueryNode translatedNode = TranslateFilterExpression("not(@p eq 123)");

            // Assert
            var unaryOperatorNode = Assert.IsType<UnaryOperatorNode>(translatedNode);
            var binaryOperatorNode = Assert.IsType<BinaryOperatorNode>(unaryOperatorNode.Operand);
            var convertNode = Assert.IsType<ConvertNode>(binaryOperatorNode.Left);
            Assert.Same(_parameterAliasMappedNode, convertNode.Source);
        }

        private QueryNode TranslateFilterExpression(string filter)
        {
            var parser = new ODataQueryOptionParser(_model, _customerEntityType, _customersEntitySet,
                new Dictionary<string, string> { { "$filter", filter } });
            FilterClause filterClause = parser.ParseFilter();
            var translator = new ParameterAliasNodeTranslator(
                new Dictionary<string, SingleValueNode> { { "@p", _parameterAliasMappedNode } });
            QueryNode translatedNode = filterClause.Expression.Accept(translator);
            return translatedNode;
        }

        private class ParameterAliasCustomer
        {
            public int ID { get; set; }
            public ParameterAliasOpenComplexType OpenComplex { get; set; }
            public ParameterAliasOrder SpecialOrder { get; set; }
            public ParameterAliasAddress SpecialAddress { get; set; }
            public IList<ParameterAliasAddress> Addresses { get; set; }
            public IList<ParameterAliasOrder> Orders { get; set; }
        }

        private class ParameterAliasOpenComplexType
        {
            public IDictionary<string, object> DynamicProperties { get; set; }
        }

        private class ParameterAliasOrder
        {
            public int ID { get; set; }
        }

        private class ParameterAliasAddress
        {
            public string Street { get; set; }
        }
    }
}
