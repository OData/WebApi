// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class TypedEdmEntityObjectTest
    {
        [Fact]
        public void Ctor_InitializesProperty_Instance()
        {
            object instance = new object();
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;

            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance, edmType);

            Assert.Same(instance, edmObject.Instance);
        }

        [Fact]
        public void GetEdmType_Returns_InitializedEdmType()
        {
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(42, edmType);

            IEdmTypeReference result = edmObject.GetEdmType();

            Assert.Same(edmType, result);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfInstanceIsNull()
        {
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance: null, edmType: edmType);
            object value;

            Assert.False(edmObject.TryGetPropertyValue("property", out value));
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_IfPropertyIsPresent()
        {
            TestEntity instance = new TestEntity { Property = new object() };
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance, edmType);
            object value;

            Assert.True(edmObject.TryGetPropertyValue("Property", out value));
            Assert.Same(instance.Property, value);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfPropertyIsNotPresent()
        {
            TestEntity instance = new TestEntity { Property = new object() };
            IEdmEntityTypeReference edmType = new Mock<IEdmEntityTypeReference>().Object;
            TypedEdmEntityObject edmObject = new TypedEdmEntityObject(instance, edmType);
            object value;

            Assert.False(edmObject.TryGetPropertyValue("NotPresentProperty", out value));
            Assert.Null(value);
        }

        [Fact]
        public void GetPropertyGetter_Caches_PropertyGetter()
        {
            Func<object, object> getter1 = TypedEdmStructuredObject.GetOrCreatePropertyGetter(typeof(TestEntity), "Property");
            Func<object, object> getter2 = TypedEdmStructuredObject.GetOrCreatePropertyGetter(typeof(TestEntity), "Property");

            Assert.Same(getter1, getter2);
        }

        public class TestEntity
        {
            public object Property { get; set; }
        }
    }
}
