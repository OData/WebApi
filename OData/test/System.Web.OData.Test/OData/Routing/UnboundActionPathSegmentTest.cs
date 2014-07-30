// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class UnboundActionPathSegmentTest
    {
        private IEdmModel _model;
        private IEdmEntityContainer _container;

        public UnboundActionPathSegmentTest()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<MyCustomer>().HasKey(c => c.Id).Property(c => c.Name);
            builder.EntitySet<MyCustomer>("Customers");
            ActionConfiguration action = builder.Action("CreateCustomer");
            action.ReturnsFromEntitySet<MyCustomer>("Customers");
            builder.Action("MyAction").Returns<string>();
            builder.Action("ActionWithoutReturn");
            _model = builder.GetEdmModel();
            _container = _model.EntityContainer;
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Action()
        {
            Assert.ThrowsArgumentNull(() => new UnboundActionPathSegment(action: null), "action");
        }

        [Fact]
        public void Ctor_TakingAction_InitializesActionProperty()
        {
            // Arrange
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");

            // Act
            UnboundActionPathSegment actionPathSegment = new UnboundActionPathSegment(edmAction.Object);

            // Assert
            Assert.Same(edmAction.Object, actionPathSegment.Action);
        }

        [Fact]
        public void Ctor_TakingAction_InitializesActionNameProperty()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("CreateCustomer").SingleOrDefault() as IEdmActionImport;

            // Act
            UnboundActionPathSegment actionPathSegment = new UnboundActionPathSegment(action);

            // Assert
            Assert.Equal("CreateCustomer", actionPathSegment.ActionName);
        }

        [Fact]
        public void Ctor_TakingActionName_InitializesActionNameProperty()
        {
            // Arrange
            UnboundActionPathSegment actionPathSegment = new UnboundActionPathSegment("SomeAction");

            // Act & Assert
            Assert.Null(actionPathSegment.Action);
            Assert.Equal("SomeAction", actionPathSegment.ActionName);
        }

        [Fact]
        public void Property_SegmentKind_IsUnboundAction()
        {
            // Arrange
            UnboundActionPathSegment segment = new UnboundActionPathSegment("SomeAction");

            // Act & Assert
            Assert.Equal(ODataSegmentKinds.UnboundAction, segment.SegmentKind);
        }

        [Fact]
        public void GetEdmType_ThrowArgumentException_IfArgumentNotNull()
        {
            // Arrange
            var segment = new UnboundActionPathSegment("CreateCustomer");
            Mock<IEdmType> edmType = new Mock<IEdmType>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => segment.GetEdmType(edmType.Object));
        }

        [Fact]
        public void GetEdmType_ReturnsNotNull_UnboundActionEntityType()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("CreateCustomer").SingleOrDefault() as IEdmActionImport;

            // Act
            UnboundActionPathSegment segment = new UnboundActionPathSegment(action);
            var result = segment.GetEdmType(previousEdmType: null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("System.Web.OData.Routing.MyCustomer", result.FullTypeName());
        }

        [Fact]
        public void GetEdmType_ReturnsNotNull_UnboundActionPrimitiveType()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("MyAction").SingleOrDefault() as IEdmActionImport;

            // Act
            UnboundActionPathSegment segment = new UnboundActionPathSegment(action);
            var result = segment.GetEdmType(previousEdmType: null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edm.String", result.FullTypeName());
        }

        [Fact]
        public void GetEdmType_ReturnsNull_UnboundAction()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("ActionWithoutReturn").SingleOrDefault() as IEdmActionImport;

            // Act
            UnboundActionPathSegment segment = new UnboundActionPathSegment(action);
            var result = segment.GetEdmType(previousEdmType: null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetNavigationSource_ReturnsNotNull_UnboundActionEntityset()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("CreateCustomer").SingleOrDefault() as IEdmActionImport;

            // Act
            UnboundActionPathSegment segment = new UnboundActionPathSegment(action);
            var result = segment.GetNavigationSource(previousNavigationSource: null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("System.Web.OData.Routing.MyCustomer", result.EntityType().FullName());
        }

        [Fact]
        public void GetNavigationSource_ReturnsNull_UnboundActionPrimitveReturnType()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("MyAction").SingleOrDefault() as IEdmActionImport;
            UnboundActionPathSegment segment = new UnboundActionPathSegment(action);

            // Act
            var result = segment.GetNavigationSource(previousNavigationSource: null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToString_ReturnsActionName()
        {
            // Arrange
            IEdmActionImport action = _container.FindOperationImports("CreateCustomer").SingleOrDefault() as IEdmActionImport;
            UnboundActionPathSegment segment = new UnboundActionPathSegment(action);

            // Act
            var result = segment.ToString();

            // Assert
            Assert.Equal("CreateCustomer", result);
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfThePathSegmentRefersToSameAction()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            Mock<IEdmActionImport> edmAction = new Mock<IEdmActionImport>();
            edmAction.Setup(a => a.Name).Returns("SomeAction");
            edmAction.Setup(a => a.Container).Returns(container);

            UnboundActionPathSegment pathSegmentTemplate = new UnboundActionPathSegment(edmAction.Object);
            UnboundActionPathSegment pathSegment = new UnboundActionPathSegment(edmAction.Object);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act
            var match = pathSegmentTemplate.TryMatch(pathSegment, values);

            // Assert
            Assert.True(match);
            Assert.Empty(values);
        }

        internal class MyCustomer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
