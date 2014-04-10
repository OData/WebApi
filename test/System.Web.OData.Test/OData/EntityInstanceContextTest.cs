// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData
{
    public class EntityInstanceContextTest
    {
        private ODataSerializerContext _serializerContext = new ODataSerializerContext { Model = EdmCoreModel.Instance };
        private IEdmEntityType _entityType = new EdmEntityType("NS", "Name");
        private object _entityInstance = new object();
        private EntityInstanceContext _context = new EntityInstanceContext();

        [Fact]
        public void EmptyCtor_InitializesProperty_SerializerContext()
        {
            var context = new EntityInstanceContext();
            Assert.NotNull(context.SerializerContext);
        }

        [Fact]
        public void Property_EntityInstance_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EntityInstance, null, allowNull: true, roundTripTestValue: _entityInstance);
        }

        [Fact]
        public void Property_EdmModel_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EdmModel, null, allowNull: true, roundTripTestValue: EdmCoreModel.Instance);
        }

        [Fact]
        public void Property_EntitySet_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.NavigationSource, null, allowNull: true, roundTripTestValue: new Mock<IEdmEntitySet>().Object);
        }

        [Fact]
        public void Property_EntityType_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EntityType, null, allowNull: true, roundTripTestValue: _entityType);
        }

        [Fact]
        public void Property_Request_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.Request, null, allowNull: true, roundTripTestValue: new HttpRequestMessage());
        }

        [Fact]
        public void Property_SerializerContext_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.SerializerContext, _context.SerializerContext, allowNull: true, roundTripTestValue: new ODataSerializerContext());
        }

        [Fact]
        public void Property__RoundTrips()
        {
            Assert.Reflection.BooleanProperty(_context, (c) => c.SkipExpensiveAvailabilityChecks, false);
        }

        [Fact]
        public void Property_Url_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.Url, null, allowNull: true, roundTripTestValue: new UrlHelper(new HttpRequestMessage()));
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfPropertyIsNotFound()
        {
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            Mock<IEdmStructuredObject> edmObject = new Mock<IEdmStructuredObject>();
            edmObject.Setup(o => o.GetEdmType()).Returns(entityType);
            EntityInstanceContext instanceContext = new EntityInstanceContext(_serializerContext, entityType, edmObject.Object);

            Assert.Throws<InvalidOperationException>(
                () => instanceContext.GetPropertyValue("NotPresentProperty"),
                "The EDM instance of type '[NS.Name Nullable=False]' is missing the property 'NotPresentProperty'.");
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfEdmObjectIsNull()
        {
            EntityInstanceContext instanceContext = new EntityInstanceContext();
            Assert.Throws<InvalidOperationException>(
                () => instanceContext.GetPropertyValue("SomeProperty"),
                "The property 'EdmObject' of EntityInstanceContext cannot be null.");
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfEdmObjectGetEdmTypeReturnsNull()
        {
            // Arrange
            object outObject = null;
            Mock<IEdmEntityObject> mock = new Mock<IEdmEntityObject>();
            mock.Setup(o => o.TryGetPropertyValue(It.IsAny<string>(), out outObject)).Returns(false).Verifiable();
            mock.Setup(o => o.GetEdmType()).Returns<IEdmTypeReference>(null).Verifiable();
            EntityInstanceContext context = new EntityInstanceContext();
            context.EdmObject = mock.Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => context.GetPropertyValue("SomeProperty"),
                exceptionMessage: "The EDM type of the object of type 'Castle.Proxies.IEdmEntityObjectProxy' is null. " +
                "The EDM type of an IEdmObject cannot be null.");
            mock.Verify();
        }

        [Fact]
        public void Property_EntityInstance_CanBeBuiltFromIEdmObject()
        {
            // Arrange
            EdmEntityType edmType = new EdmEntityType("NS", "Name");
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            EdmModel model = new EdmModel();
            model.AddElement(edmType);
            model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(typeof(TestEntity)));
            Mock<IEdmEntityObject> edmObject = new Mock<IEdmEntityObject>();
            object propertyValue = 42;
            edmObject.Setup(e => e.TryGetPropertyValue("Property", out propertyValue)).Returns(true);
            edmObject.Setup(e => e.GetEdmType()).Returns(new EdmEntityTypeReference(edmType, isNullable: false));

            EntityInstanceContext entityContext = new EntityInstanceContext { EdmModel = model, EdmObject = edmObject.Object, EntityType = edmType };

            // Act
            object resource = entityContext.EntityInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(42, testEntity.Property);
        }

        [Fact]
        public void Property_EntityInstance_EdmObjectHasCollectionProperty()
        {
            // Arrange
            EdmEntityType edmType = new EdmEntityType("NS", "Name");
            edmType.AddStructuralProperty(
                "CollectionProperty",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false))));
            EdmModel model = new EdmModel();
            model.AddElement(edmType);
            model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(typeof(TestEntity)));
            Mock<IEdmEntityObject> edmObject = new Mock<IEdmEntityObject>();
            object propertyValue = new List<int> { 42 };
            edmObject.Setup(e => e.TryGetPropertyValue("CollectionProperty", out propertyValue)).Returns(true);
            edmObject.Setup(e => e.GetEdmType()).Returns(new EdmEntityTypeReference(edmType, isNullable: false));

            EntityInstanceContext entityContext = new EntityInstanceContext { EdmModel = model, EdmObject = edmObject.Object, EntityType = edmType };

            // Act
            object resource = entityContext.EntityInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(new[] { 42 }, testEntity.CollectionProperty);
        }

        [Fact]
        public void Property_EntityInstance_ThrowsInvalidOp_EntityTypeDoesNotHaveAMapping()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            EdmModel model = new EdmModel();
            IEdmEntityObject instance = new Mock<IEdmEntityObject>().Object;
            EntityInstanceContext entityContext = new EntityInstanceContext { EntityType = entityType, EdmModel = model, EdmObject = instance };

            Assert.Throws<InvalidOperationException>(
                () => entityContext.EntityInstance, "The provided mapping does not contain an entry for the entity type 'NS.Name'.");
        }

        [Fact]
        public void Property_EntityInstance_ReturnsNullWhenEdmObjectIsNull()
        {
            EntityInstanceContext entityContext = new EntityInstanceContext { EdmObject = null };
            Assert.Null(entityContext.EntityInstance);
        }

        [Fact]
        public void Property_EntityInstance_ReturnsEdmStructuredObjectInstance()
        {
            object instance = new object();
            IEdmEntityTypeReference entityType = new Mock<IEdmEntityTypeReference>().Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;
            EntityInstanceContext entityContext =
                new EntityInstanceContext { EdmObject = new TypedEdmEntityObject(instance, entityType, edmModel) };

            Assert.Same(instance, entityContext.EntityInstance);
        }

        private class TestEntity
        {
            public int Property { get; set; }

            public int[] CollectionProperty { get; set; }
        }
    }
}
