// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class TypedEdmEntityObjectTest
    {
        [Fact]
        public void Ctor_InitializesProperty_Instance()
        {
            // Arrange
            object instance = new object();
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;

            // Act
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance, edmType, edmModel);

            // Assert
            Assert.Same(instance, edmObject.Instance);
        }

        [Fact]
        public void GetEdmType_Returns_InitializedEdmType()
        {
            // Arrange
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(42, edmType, edmModel);

            // Act
            IEdmTypeReference result = edmObject.GetEdmType();

            // Assert
            Assert.Same(edmType, result);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfInstanceIsNull()
        {
            // Arrange
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance: null, edmType: edmType, edmModel: edmModel);
            object value;

            // Act & Assert
            Assert.False(edmObject.TryGetPropertyValue("property", out value));
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_IfPropertyIsPresent()
        {
            // Arrange
            TestEntity instance = new TestEntity { Property = new object() };
            Mock<IEdmEntityTypeReference> mockEdmType = new Mock<IEdmEntityTypeReference>();
            mockEdmType.Setup(t => t.Definition).Returns(new Mock<IEdmStructuredType>().Object);
            IEdmEntityTypeReference edmType = mockEdmType.Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance, edmType, edmModel);
            object value;

            // Act & Assert
            Assert.True(edmObject.TryGetPropertyValue("Property", out value));
            Assert.Same(instance.Property, value);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfPropertyIsNotPresent()
        {
            // Arrange
            TestEntity instance = new TestEntity { Property = new object() };
            Mock<IEdmEntityTypeReference> mockEdmType = new Mock<IEdmEntityTypeReference>();
            mockEdmType.Setup(t => t.Definition).Returns(new Mock<IEdmStructuredType>().Object);
            IEdmEntityTypeReference edmType = mockEdmType.Object;
            IEdmModel edmModel = new Mock<IEdmModel>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance, edmType, edmModel);
            object value;

            // Act & Assert
            Assert.False(edmObject.TryGetPropertyValue("NotPresentProperty", out value));
            Assert.Null(value);
        }

        [Fact]
        public void GetPropertyGetter_Caches_PropertyGetter()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> mockEdmTypeReference1 = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmStructuredTypeReference> mockEdmTypeReference2 = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmStructuredType> mockEdmType = new Mock<IEdmStructuredType>();
            Mock<IEdmProperty> mockProperty = new Mock<IEdmProperty>();
            mockEdmType.Setup(t => t.FindProperty("Property")).Returns(mockProperty.Object);
            mockEdmTypeReference1.Setup(t => t.Definition).Returns(mockEdmType.Object);
            mockEdmTypeReference2.Setup(t => t.Definition).Returns(mockEdmType.Object);
            IEdmModel model = new Mock<EdmModel>().Object;
            Func<object, object> getter1 = TypedEdmStructuredObject.GetOrCreatePropertyGetter(
                typeof(TestEntity),
                "Property",
                mockEdmTypeReference1.Object,
                model);
            Func<object, object> getter2 = TypedEdmStructuredObject.GetOrCreatePropertyGetter(
                typeof(TestEntity),
                "Property",
                mockEdmTypeReference2.Object,
                model);

            // Act & Assert
            Assert.Same(getter1, getter2);
        }

        public class TestEntity
        {
            public object Property { get; set; }
        }
    }
}
