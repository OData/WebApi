// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Query
{
    public class OrderByPropertyNodeTest
    {
        [Fact]
        public void Constructor_With_Null_Throws()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                () => new OrderByPropertyNode(null, OrderByDirection.Ascending),
                "property");
        }

        [Fact]
        public void Constructor_Initializes_Correctly()
        {
            // Arrange
            Mock<IEdmProperty> mockProperty = new Mock<IEdmProperty>();

            // Act
            OrderByPropertyNode node = new OrderByPropertyNode(mockProperty.Object, OrderByDirection.Descending);

            // Assert
            Assert.ReferenceEquals(mockProperty.Object, node.Property);
            Assert.Equal(OrderByDirection.Descending, node.Direction);
        }

        [Fact]
        public void CreateCollection_From_OrderByQueryNode_Succeeds()
        {
            // Arrange
            Mock<IEdmTypeReference> mockTypeReference1 = new Mock<IEdmTypeReference>();
            Mock<IEdmTypeReference> mockTypeReference2 = new Mock<IEdmTypeReference>();
            Mock<IEdmProperty> mockProperty1 = new Mock<IEdmProperty>();
            mockProperty1.SetupGet<IEdmTypeReference>(p => p.Type).Returns(mockTypeReference1.Object);
            Mock<IEdmProperty> mockProperty2 = new Mock<IEdmProperty>();
            mockProperty1.SetupGet<IEdmTypeReference>(p => p.Type).Returns(mockTypeReference2.Object);
            PropertyAccessQueryNode propertyAccessQueryNode1 = new PropertyAccessQueryNode()
            {
                Property = mockProperty1.Object,
            };
            PropertyAccessQueryNode propertyAccessQueryNode2 = new PropertyAccessQueryNode()
            {
                Property = mockProperty2.Object,
            };

            OrderByQueryNode queryNode1 = new OrderByQueryNode()
            {
                Direction = OrderByDirection.Descending,
                Collection = null,
                Expression = propertyAccessQueryNode1
            };

            OrderByQueryNode queryNode2 = new OrderByQueryNode()
            {
                Direction = OrderByDirection.Ascending,
                Collection = queryNode1,
                Expression = propertyAccessQueryNode2
            };

            // Act
            ICollection<OrderByPropertyNode> nodes = OrderByPropertyNode.CreateCollection(queryNode2);

            // Assert
            Assert.Equal(2, nodes.Count);
            Assert.ReferenceEquals(mockProperty1.Object, nodes.First().Property);
            Assert.Equal(OrderByDirection.Descending, nodes.First().Direction);

            Assert.ReferenceEquals(mockProperty2.Object, nodes.Last().Property);
            Assert.Equal(OrderByDirection.Ascending, nodes.Last().Direction);
        }
    }
}
