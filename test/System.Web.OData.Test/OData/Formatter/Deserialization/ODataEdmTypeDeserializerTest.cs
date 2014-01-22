// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataEdmTypeDeserializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            var deserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Unsupported);

            Assert.Equal(ODataPayloadKind.Unsupported, deserializer.Object.ODataPayloadKind);
        }

        [Fact]
        public void Ctor_SetsProperty_DeserializerProvider()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Unsupported, deserializerProvider.Object);

            Assert.Same(deserializerProvider.Object, deserializer.Object.DeserializerProvider);
        }

        [Fact]
        public void ReadInline_Throws_NotSupported()
        {
            var deserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => deserializer.Object.ReadInline(item: null, edmType: null, readContext: null),
                "Type 'ODataEdmTypeDeserializerProxy' does not support ReadInline.");
        }
    }
}
