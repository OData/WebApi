// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEntrySerializerTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(
                () =>
                {
                    var serializer = new Mock<ODataEntrySerializer>(null, ODataPayloadKind.Unsupported).Object;
                },
                "edmType");
        }

        [Fact]
        public void Ctor_SetsProperty_EdmType()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEntrySerializer>(edmType, ODataPayloadKind.Unsupported).Object;

            Assert.Same(edmType, serializer.EdmType);
        }

        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEntrySerializer>(edmType, ODataPayloadKind.Unsupported).Object;

            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void Ctor_SetsProperty_SerializerProvider()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            var serializer = new Mock<ODataEntrySerializer>(edmType, ODataPayloadKind.Unsupported, serializerProvider).Object;

            Assert.Same(serializerProvider, serializer.SerializerProvider);
        }

        [Fact]
        public void WriteObjectInline_Throws_NotSupported()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEntrySerializer>(edmType, ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => serializer.Object.WriteObjectInline(graph: null, writer: null, writeContext: null),
                "ODataEntrySerializerProxy does not support WriteObjectInline.");
        }

        [Fact]
        public void CreateProperty_Throws_NotSupported()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEntrySerializer>(edmType, ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => serializer.Object.CreateProperty(graph: null, elementName: "element", writeContext: null),
                "ODataEntrySerializerProxy does not support CreateProperty.");
        }
    }
}
