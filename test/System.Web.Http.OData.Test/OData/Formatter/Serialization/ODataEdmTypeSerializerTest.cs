// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEdmTypeSerializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported).Object;
            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void Ctor_SetsProperty_SerializerProvider()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported, serializerProvider).Object;

            Assert.Same(serializerProvider, serializer.SerializerProvider);
        }

        [Fact]
        public void WriteObjectInline_Throws_NotSupported()
        {
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => serializer.Object.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: null),
                "ODataEdmTypeSerializerProxy does not support WriteObjectInline.");
        }

        [Fact]
        public void CreateODataValue_Throws_NotSupported()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => serializer.Object.CreateODataValue(graph: null, expectedType: edmType, writeContext: null),
                "ODataEdmTypeSerializerProxy does not support CreateODataValue.");
        }

        [Fact]
        public void CreateProperty_Returns_ODataProperty()
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported);
            serializer
                .Setup(s => s.CreateODataValue(42, edmType, null))
                .Returns(new ODataPrimitiveValue(42));

            // Act
            ODataProperty property = serializer.Object.CreateProperty(graph: 42, expectedType: edmType,
                elementName: "SomePropertyName", writeContext: null);

            // Assert
            Assert.NotNull(property);
            Assert.Equal("SomePropertyName", property.Name);
            Assert.Equal(42, property.Value);
        }
    }
}
