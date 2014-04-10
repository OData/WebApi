// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class BoundActionPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Action()
        {
            Assert.ThrowsArgumentNull(() => new BoundActionPathSegment(action: null), "action");
        }

        [Fact]
        public void Ctor_TakingAction_InitializesActionProperty()
        {
            // Arrange
            Mock<IEdmAction> edmAction = new Mock<IEdmAction>();
            edmAction.Setup(a => a.Namespace).Returns("NS");
            edmAction.Setup(a => a.Name).Returns("SomeAction");

            // Act
            BoundActionPathSegment actionPathSegment = new BoundActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(edmAction.Object, actionPathSegment.Action);
        }

        [Fact]
        public void Ctor_TakingAction_InitializesActionNameProperty()
        {
            // Arrange
            Mock<IEdmAction> edmAction = new Mock<IEdmAction>();
            edmAction.Setup(a => a.Namespace).Returns("NS");
            edmAction.Setup(a => a.Name).Returns("SomeAction");

            // Act
            BoundActionPathSegment actionPathSegment = new BoundActionPathSegment(edmAction.Object);

            // Assert
            Assert.Equal("NS.SomeAction", actionPathSegment.ActionName);
        }

        [Fact]
        public void Ctor_TakingActionName_InitializesActionNameProperty()
        {
            // Arrange
            BoundActionPathSegment actionPathSegment = new BoundActionPathSegment("SomeAction");

            // Act & Assert
            Assert.Null(actionPathSegment.Action);
            Assert.Equal("SomeAction", actionPathSegment.ActionName);
        }

        [Fact]
        public void Property_SegmentKind_IsBoundAction()
        {
            // Arrange
            BoundActionPathSegment segment = new BoundActionPathSegment("SomeAction");

            // Act & Assert
            Assert.Equal(ODataSegmentKinds.Action, segment.SegmentKind);
        }

        [Fact]
        public void GetEdmType_Returns_ActionReturnType()
        {
            // Arrange
            Mock<IEdmEntityType> returnType = new Mock<IEdmEntityType>();
            Mock<IEdmAction> edmAction = new Mock<IEdmAction>();
            edmAction.Setup(a => a.Namespace).Returns("NS");
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.ReturnType).Returns(new EdmEntityTypeReference(returnType.Object, isNullable: false));

            // Act
            BoundActionPathSegment segment = new BoundActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(returnType.Object, segment.GetEdmType(previousEdmType: null));
        }

        [Fact]
        public void GetNavigationSource_Returns_ActionTargetEntitySet()
        {
            // Arrange
            Mock<IEdmEntitySet> targetEntitySet = new Mock<IEdmEntitySet>();
            Mock<IEdmAction> edmAction = new Mock<IEdmAction>();
            edmAction.Setup(a => a.Namespace).Returns("NS");
            edmAction.Setup(a => a.Name).Returns("SomeAction");

            // Act
            BoundActionPathSegment segment = new BoundActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(targetEntitySet.Object, segment.GetNavigationSource(targetEntitySet.Object));
        }

        [Fact]
        public void ToString_ReturnsActionName()
        {
            BoundActionPathSegment segment = new BoundActionPathSegment(actionName: "SomeAction");
            Assert.Equal("SomeAction", segment.ToString());
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfThePathSegmentRefersToSameAction()
        {
            // Arrange
            Mock<IEdmAction> edmAction = new Mock<IEdmAction>();
            edmAction.Setup(a => a.Namespace).Returns("NS");
            edmAction.Setup(a => a.Name).Returns("SomeAction");

            BoundActionPathSegment pathSegmentTemplate = new BoundActionPathSegment(edmAction.Object);
            BoundActionPathSegment pathSegment = new BoundActionPathSegment(edmAction.Object);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act & Assert
            Assert.True(pathSegmentTemplate.TryMatch(pathSegment, values));
            Assert.Empty(values);
        }
    }
}
