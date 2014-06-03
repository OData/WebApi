// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
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
            Assert.ThrowsArgumentNull(() => new BoundActionPathSegment(action: null, model: null), "action");
        }

        [Fact]
        public void Ctor_TakingAction_InitializesActionProperty()
        {
            // Arrange
            Mock<IEdmAction> edmAction = new Mock<IEdmAction>();
            edmAction.Setup(a => a.Namespace).Returns("NS");
            edmAction.Setup(a => a.Name).Returns("SomeAction");

            // Act
            BoundActionPathSegment actionPathSegment = new BoundActionPathSegment(edmAction.Object, null);

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
            BoundActionPathSegment actionPathSegment = new BoundActionPathSegment(edmAction.Object, null);

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
            BoundActionPathSegment segment = new BoundActionPathSegment(edmAction.Object, null);

            // Assert
            Assert.Same(returnType.Object, segment.GetEdmType(previousEdmType: null));
        }

        [Fact]
        public void GetNavigationSource_Returns_ActionTargetEntitySet_EntitySetPathExpression()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmAction action = model.SchemaElements.OfType<IEdmAction>().First(c => c.Name == "GetMyOrders1");
            IEdmEntitySet previouseEntitySet = model.EntityContainer.FindEntitySet("MyCustomers");
            IEdmEntitySet expectedEntitySet = model.EntityContainer.FindEntitySet("MyOrders");

            // Act
            BoundActionPathSegment segment = new BoundActionPathSegment(action, model);

            // Assert
            Assert.Same(expectedEntitySet, segment.GetNavigationSource(previouseEntitySet));
        }

        [Fact]
        public void GetNavigationSource_Returns_ActionTargetEntitySet_Annotation()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmAction action = model.SchemaElements.OfType<IEdmAction>().First(c => c.Name == "GetMyOrders2");
            IEdmEntitySet previouseEntitySet = model.EntityContainer.FindEntitySet("MyCustomers");
            IEdmEntitySet expectedEntitySet = model.EntityContainer.FindEntitySet("MyOrders");

            // Act
            BoundActionPathSegment segment = new BoundActionPathSegment(action, model);

            // Assert
            Assert.Same(expectedEntitySet, segment.GetNavigationSource(previouseEntitySet));
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

            BoundActionPathSegment pathSegmentTemplate = new BoundActionPathSegment(edmAction.Object, null);
            BoundActionPathSegment pathSegment = new BoundActionPathSegment(edmAction.Object, null);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act & Assert
            Assert.True(pathSegmentTemplate.TryMatch(pathSegment, values));
            Assert.Empty(values);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("MyCustomers");
            builder.EntitySet<Order>("MyOrders");

            builder.EntityType<Customer>()
                .Action("GetMyOrders1")
                .ReturnsEntityViaEntitySetPath<Order>("bindingParameter/Orders");

            builder.EntityType<Customer>().Action("GetMyOrders2").ReturnsFromEntitySet<Order>("MyOrders");
            return builder.GetEdmModel();
        }
    }
}
