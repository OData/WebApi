// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter
{
    public class EdmStructuredObjectTest
    {
        [Fact]
        public void Ctor_InitializesProperty_Instance()
        {
            object instance = new object();
            IEdmStructuredTypeReference edmType = new Mock<IEdmStructuredTypeReference>().Object;

            EdmStructuredObject edmObject = new EdmStructuredObject(instance, edmType);

            Assert.Same(instance, edmObject.Instance);
        }

        [Fact]
        public void Ctor_InitializesProperty_EdmType()
        {
            IEdmStructuredTypeReference edmType = new Mock<IEdmStructuredTypeReference>().Object;

            EdmStructuredObject edmObject = new EdmStructuredObject(42, edmType);

            Assert.Same(edmType, edmObject.EdmType);
        }

        [Fact]
        public void GetEdmType_Returns_InitializedEdmType()
        {
            IEdmStructuredTypeReference edmType = new Mock<IEdmStructuredTypeReference>().Object;
            EdmStructuredObject edmObject = new EdmStructuredObject(42, edmType);

            IEdmTypeReference result = edmObject.GetEdmType();

            Assert.Same(edmType, edmObject.EdmType);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfInstanceIsNull()
        {
            IEdmStructuredTypeReference edmType = new Mock<IEdmStructuredTypeReference>().Object;
            EdmStructuredObject edmObject = new EdmStructuredObject(instance: null, edmType: edmType);
            object value;

            Assert.False(edmObject.TryGetValue("property", out value));
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_IfPropertyIsPresent()
        {
            TestEntity instance = new TestEntity { Property = new object() };
            IEdmStructuredTypeReference edmType = new Mock<IEdmStructuredTypeReference>().Object;
            EdmStructuredObject edmObject = new EdmStructuredObject(instance, edmType);
            object value;

            Assert.True(edmObject.TryGetValue("Property", out value));
            Assert.Same(instance.Property, value);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfPropertyIsNotPresent()
        {
            TestEntity instance = new TestEntity { Property = new object() };
            IEdmStructuredTypeReference edmType = new Mock<IEdmStructuredTypeReference>().Object;
            EdmStructuredObject edmObject = new EdmStructuredObject(instance, edmType);
            object value;

            Assert.False(edmObject.TryGetValue("NotPresentProperty", out value));
            Assert.Null(value);
        }

        public class TestEntity
        {
            public object Property { get; set; }
        }
    }
}
