// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Values;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Query
{
    public class EnumNodeLearningTests
    {
        [Fact]
        public void EnumNode_ThrowsNotImplementedException_AccessKind()
        {
            // Arrange
            Mock<IEdmEnumTypeReference> type = new Mock<IEdmEnumTypeReference>();
            Mock<IEdmEnumValue> value = new Mock<IEdmEnumValue>();
            EnumNode enumNode = new EnumNode(type.Object, value.Object);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => enumNode.Kind, "The method or operation is not implemented.");
        }
    }
}
