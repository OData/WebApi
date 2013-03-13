// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEdmTypeDeserializerTest
    {
        [Fact]
        public void Ctor_Throws_ArgumentNullForEdmType()
        {
            Assert.ThrowsArgumentNull(
                () =>
                {
                    var deserializer = new Mock<ODataEdmTypeDeserializer>(null, ODataPayloadKind.Unsupported).Object;
                },
                "edmType");
        }

        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            var deserializer = new Mock<ODataEdmTypeDeserializer>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported);

            Assert.Equal(ODataPayloadKind.Unsupported, deserializer.Object.ODataPayloadKind);
        }

        [Fact]
        public void Ctor_SetsProperty_EdmType()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var deserializer = new Mock<ODataEdmTypeDeserializer>(edmType, ODataPayloadKind.Unsupported);

            Assert.Same(edmType, deserializer.Object.EdmType);
        }

        [Fact]
        public void Ctor_SetsProperty_DeserializerProvider()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new Mock<ODataEdmTypeDeserializer>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported, deserializerProvider.Object);

            Assert.Same(deserializerProvider.Object, deserializer.Object.DeserializerProvider);
        }

        [Fact]
        public void ReadInline_Throws_NotSupported()
        {
            var deserializer = new Mock<ODataEdmTypeDeserializer>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => deserializer.Object.ReadInline(item: null, readContext: null),
                "Type 'ODataEdmTypeDeserializerProxy' does not support ReadInline.");
        }
    }
}
