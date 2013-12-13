// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Expressions;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Routing
{
    public class ActionPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Action()
        {
            Assert.ThrowsArgumentNull(() => new ActionPathSegment(action: null), "action");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_ActionName()
        {
            Assert.ThrowsArgumentNull(() => new ActionPathSegment(actionName: null), "actionName");
        }

        [Fact]
        public void Ctor_TakingAction_InitializesPropertyAction()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.Container).Returns(container);

            // Act
            ActionPathSegment actionPathSegment = new ActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(edmAction.Object, actionPathSegment.Action);
        }

        [Fact]
        public void Ctor_TakingAction_InitializesPropertyActionName()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.Container).Returns(container);

            // Act
            ActionPathSegment actionPathSegment = new ActionPathSegment(edmAction.Object);

            // Assert
            Assert.Equal("NS.Container.SomeAction", actionPathSegment.ActionName);
        }

        [Fact]
        public void Ctor_TakingActionName_InitializesPropertyActionName()
        {
            ActionPathSegment actionPathSegment = new ActionPathSegment("SomeAction");
            Assert.Equal("SomeAction", actionPathSegment.ActionName);
        }

        [Fact]
        public void Property_SegmentKind_IsAction()
        {
            ActionPathSegment segment = new ActionPathSegment("SomeAction");
            Assert.Equal(ODataSegmentKinds.Action, segment.SegmentKind);
        }

        [Fact]
        public void GetEdmType_Returns_ActionReturnType()
        {
            // Arrange
            Mock<IEdmEntityType> returnType = new Mock<IEdmEntityType>();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.Container).Returns(container);
            edmAction.Setup(a => a.Action.ReturnType).Returns(new EdmEntityTypeReference(returnType.Object, isNullable: false));

            // Act
            ActionPathSegment segment = new ActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(returnType.Object, segment.GetEdmType(previousEdmType: null));
        }

        [Fact]
        public void GetEntitySet_Returns_ActionTargetEntitySet()
        {
            // Arrange
            Mock<IEdmEntitySet> targetEntitySet = new Mock<IEdmEntitySet>();
            Mock<IEdmEntityType> returnType = new Mock<IEdmEntityType>();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.Container).Returns(container);
            edmAction.Setup(a => a.Action.ReturnType).Returns(new EdmEntityTypeReference(returnType.Object, isNullable: false));
            edmAction.Setup(a => a.EntitySet).Returns(new EdmEntitySetReferenceExpression(targetEntitySet.Object));

            // Act
            ActionPathSegment segment = new ActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(targetEntitySet.Object, segment.GetEntitySet(previousEntitySet: null));
        }

        [Fact]
        public void ToString_ReturnsActionName()
        {
            ActionPathSegment segment = new ActionPathSegment(actionName: "SomeAction");
            Assert.Equal("SomeAction", segment.ToString());
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfThePathSegmentRefersToSameAction()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.Container).Returns(container);

            ActionPathSegment pathSegmentTemplate = new ActionPathSegment(edmAction.Object);
            ActionPathSegment pathSegment = new ActionPathSegment(edmAction.Object);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act & Assert
            Assert.True(pathSegmentTemplate.TryMatch(pathSegment, values));
            Assert.Empty(values);
        }
    }
}
