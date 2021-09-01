//-----------------------------------------------------------------------------
// <copyright file="ResourceContextTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class ResourceContextTest
    {
        private ODataSerializerContext _serializerContext = new ODataSerializerContext { Model = EdmCoreModel.Instance };
        private IEdmEntityType _entityType = new EdmEntityType("NS", "Name");
        private object _entityInstance = new object();
        private ResourceContext _context = new ResourceContext();

        [Fact]
        public void EmptyCtor_InitializesProperty_SerializerContext()
        {
            // Arrange
            var context = new ResourceContext();

            // Act & Assert
            Assert.NotNull(context.SerializerContext);
        }

        [Fact]
        public void Property_ResourceInstance_RoundTrips()
        {
            ReflectionAssert.Property(_context, (c) => c.ResourceInstance, null, allowNull: true, roundTripTestValue: _entityInstance);
        }

        [Fact]
        public void Property_EdmModel_RoundTrips()
        {
            ReflectionAssert.Property(_context, (c) => c.EdmModel, null, allowNull: true, roundTripTestValue: EdmCoreModel.Instance);
        }

        [Fact]
        public void Property_NavigationSource_RoundTrips()
        {
            ReflectionAssert.Property(_context, (c) => c.NavigationSource, null, allowNull: true, roundTripTestValue: new Mock<IEdmEntitySet>().Object);
        }

        [Fact]
        public void Property_StructuredType_RoundTrips()
        {
            ReflectionAssert.Property(_context, (c) => c.StructuredType, null, allowNull: true, roundTripTestValue: _entityType);
        }

        [Fact]
        public void Property_Request_RoundTrips()
        {
            ReflectionAssert.Property(_context, (c) => c.Request, null, allowNull: true, roundTripTestValue: RequestFactory.Create());
        }

        [Fact]
        public void Property_SerializerContext_RoundTrips()
        {
            ReflectionAssert.Property(_context, (c) => c.SerializerContext, _context.SerializerContext, allowNull: true, roundTripTestValue: new ODataSerializerContext());
        }

        [Fact]
        public void Property_SkipExpensiveAvailabilityChecks_RoundTrips()
        {
            ReflectionAssert.BooleanProperty(_context, (c) => c.SkipExpensiveAvailabilityChecks, false);
        }

        [Fact]
        public void Property_Url_RoundTrips()
        {
#if NETFX // So far, Asp.NET core version doesn't have Url property.
            ReflectionAssert.Property(_context, (c) => c.Url, null, allowNull: true, roundTripTestValue: new UrlHelper(new HttpRequestMessage()));
#endif
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfPropertyIsNotFound()
        {
            // Arrange
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            Mock<IEdmStructuredObject> edmObject = new Mock<IEdmStructuredObject>();
            edmObject.Setup(o => o.GetEdmType()).Returns(entityType);
            ResourceContext instanceContext = new ResourceContext(_serializerContext, entityType, edmObject.Object);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => instanceContext.GetPropertyValue("NotPresentProperty"),
                "The EDM instance of type '[NS.Name Nullable=False]' is missing the property 'NotPresentProperty'.");
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfEdmObjectIsNull()
        {
            // Arrange
            ResourceContext instanceContext = new ResourceContext();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => instanceContext.GetPropertyValue("SomeProperty"),
                "The property 'EdmObject' of ResourceContext cannot be null.");
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfEdmObjectGetEdmTypeReturnsNull()
        {
            // Arrange
            object outObject;
            Mock<IEdmEntityObject> mock = new Mock<IEdmEntityObject>();
            mock.Setup(o => o.TryGetPropertyValue(It.IsAny<string>(), out outObject)).Returns(false).Verifiable();
            mock.Setup(o => o.GetEdmType()).Returns<IEdmTypeReference>(null).Verifiable();
            ResourceContext context = new ResourceContext();
            context.EdmObject = mock.Object;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => context.GetPropertyValue("SomeProperty"),
                "The EDM type of an IEdmObject cannot be null.", partialMatch: true);
            mock.Verify();
        }

        [Fact]
        public void Property_ResourceInstance_CanBeBuiltFromIEdmObject_ForEntity()
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

            ResourceContext entityContext = new ResourceContext { EdmModel = model, EdmObject = edmObject.Object, StructuredType = edmType };

            // Act
            object resource = entityContext.ResourceInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(42, testEntity.Property);
        }

        [Fact]
        public void Property_ResourceInstance_CanBeBuiltFromIEdmObject_ForComplex()
        {
            // Arrange
            EdmComplexType edmType = new EdmComplexType("NS", "Name");
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            EdmModel model = new EdmModel();
            model.AddElement(edmType);
            model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(typeof(TestEntity)));
            Mock<IEdmComplexObject> edmObject = new Mock<IEdmComplexObject>();
            object propertyValue = 42;
            edmObject.Setup(e => e.TryGetPropertyValue("Property", out propertyValue)).Returns(true);
            edmObject.Setup(e => e.GetEdmType()).Returns(new EdmComplexTypeReference(edmType, isNullable: false));

            ResourceContext entityContext = new ResourceContext { EdmModel = model, EdmObject = edmObject.Object, StructuredType = edmType };

            // Act
            object resource = entityContext.ResourceInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(42, testEntity.Property);
        }

        [Fact]
        public void Property_ResourceInstance_EdmObjectHasCollectionProperty_ForEntityCollection()
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

            ResourceContext entityContext = new ResourceContext { EdmModel = model, EdmObject = edmObject.Object, StructuredType = edmType };

            // Act
            object resource = entityContext.ResourceInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(new[] { 42 }, testEntity.CollectionProperty);
        }

        [Fact]
        public void Property_ResourceInstance_EdmObjectHasCollectionProperty_ForComplexCollection()
        {
            // Arrange
            EdmComplexType edmType = new EdmComplexType("NS", "Name");
            edmType.AddStructuralProperty(
                "CollectionProperty",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false))));
            EdmModel model = new EdmModel();
            model.AddElement(edmType);
            model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(typeof(TestEntity)));
            Mock<IEdmComplexObject> edmObject = new Mock<IEdmComplexObject>();
            object propertyValue = new List<int> { 42 };
            edmObject.Setup(e => e.TryGetPropertyValue("CollectionProperty", out propertyValue)).Returns(true);
            edmObject.Setup(e => e.GetEdmType()).Returns(new EdmComplexTypeReference(edmType, isNullable: false));

            ResourceContext entityContext = new ResourceContext { EdmModel = model, EdmObject = edmObject.Object, StructuredType = edmType };

            // Act
            object resource = entityContext.ResourceInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(new[] { 42 }, testEntity.CollectionProperty);
        }

        [Fact]
        public void Property_ResourceInstance_HandlesModelClrNameDifferences()
        {
            // Arrange
            const string clrPropertyName = "Property";
            const string modelPropertyName = "DifferentProperty";

            EdmComplexType edmType = new EdmComplexType("NS", "Name");
            EdmStructuralProperty edmProperty = edmType.AddStructuralProperty(modelPropertyName, EdmPrimitiveTypeKind.Int32);
            EdmModel model = new EdmModel();
            model.AddElement(edmType);
            model.SetAnnotationValue(edmType, new ClrTypeAnnotation(typeof(TestEntity)));
            model.SetAnnotationValue(edmProperty, new ClrPropertyInfoAnnotation(typeof(TestEntity).GetProperty(clrPropertyName)));
            Mock<IEdmComplexObject> edmObject = new Mock<IEdmComplexObject>();
            object propertyValue = 42;
            edmObject.Setup(e => e.TryGetPropertyValue(modelPropertyName, out propertyValue)).Returns(true);
            edmObject.Setup(e => e.GetEdmType()).Returns(new EdmComplexTypeReference(edmType, isNullable: false));

            ResourceContext entityContext = new ResourceContext { EdmModel = model, EdmObject = edmObject.Object, StructuredType = edmType };

            // Act
            object resource = entityContext.ResourceInstance;

            // Assert
            TestEntity testEntity = Assert.IsType<TestEntity>(resource);
            Assert.Equal(42, testEntity.Property);
        }

        [Fact]
        public void Property_ResourceInstance_ThrowsInvalidOp_ResourceTypeDoesNotHaveAMapping()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            EdmModel model = new EdmModel();
            IEdmEntityObject instance = new Mock<IEdmEntityObject>().Object;
            ResourceContext entityContext = new ResourceContext { StructuredType = entityType, EdmModel = model, EdmObject = instance };

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => entityContext.ResourceInstance, "The provided mapping does not contain a resource for the resource type 'NS.Name'.");
        }

        [Fact]
        public void Property_ResourceInstance_CanBeBuiltWithSelectExpandWrapperProperties()
        {
            // Arrange
            EdmComplexType edmType = new EdmComplexType("NS", "Name");
            var edmTypeRef = new EdmComplexTypeReference(edmType, isNullable: false);
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            edmType.AddStructuralProperty("SubEntity1", edmTypeRef);
            edmType.AddStructuralProperty("SubEntity2", edmTypeRef);
            EdmModel model = new EdmModel();
            model.AddElement(edmType);
            model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(typeof(TestSubEntity)));
            Mock<IEdmComplexObject> edmObject = new Mock<IEdmComplexObject>();
            object propertyValue = 42;
            object selectExpandWrapper = new SelectExpandWrapper<TestEntity>(null);
            object subEntity2 = new TestSubEntity
            {
                Property = 33
            };
            edmObject.Setup(e => e.TryGetPropertyValue("Property", out propertyValue)).Returns(true);
            edmObject.Setup(e => e.TryGetPropertyValue("SubEntity1", out selectExpandWrapper)).Returns(true);
            edmObject.Setup(e => e.TryGetPropertyValue("SubEntity2", out subEntity2)).Returns(true);
            edmObject.Setup(e => e.GetEdmType()).Returns(edmTypeRef);

            ResourceContext entityContext = new ResourceContext { EdmModel = model, EdmObject = edmObject.Object, StructuredType = edmType };

            // Act
            object resource = entityContext.ResourceInstance;

            // Assert
            TestSubEntity testEntity = Assert.IsType<TestSubEntity>(resource);
            Assert.Equal(42, testEntity.Property);
            Assert.Null(testEntity.SubEntity1);
            Assert.NotNull(testEntity.SubEntity2);
            Assert.Equal(33, testEntity.SubEntity2.Property);
        }

        [Fact]
        public void Property_ResourceInstance_ReturnsNullWhenEdmObjectIsNull()
        {
            // Arrange
            ResourceContext entityContext = new ResourceContext { EdmObject = null };

            // Act & Assert
            Assert.Null(entityContext.ResourceInstance);
        }

        [Fact]
        public void Property_ResourceInstance_ReturnsEdmStructuredObjectInstance()
        {
            // Arrange
            object instance = new object();
            IEdmEntityTypeReference entityType = new Mock<IEdmEntityTypeReference>().Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;
            ResourceContext entityContext =
                new ResourceContext { EdmObject = new TypedEdmEntityObject(instance, entityType, edmModel) };

            // Act & Assert
            Assert.Same(instance, entityContext.ResourceInstance);
        }

        /// <summary>
        /// A simple class with a property and collection property.
        /// </summary>
        private class TestEntity
        {
            public int Property { get; set; }

            public int[] CollectionProperty { get; set; }
        }

        private class TestSubEntity
        {
            public int Property { get; set; }

            public TestSubEntity SubEntity1 { get; set; }

            public TestSubEntity SubEntity2 { get; set; }
        }

        /// <summary>
        /// An instance of IEdmEntityObject with no EdmType.
        /// </summary>
        private class NullEdmType : IEdmEntityObject
        {
            public IEdmTypeReference GetEdmType()
            {
                return null;
            }

            public bool TryGetPropertyValue(string propertyName, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}
