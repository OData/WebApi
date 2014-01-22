// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataDeserializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Unsupported);

            Assert.Equal(ODataPayloadKind.Unsupported, deserializer.Object.ODataPayloadKind);
        }

        [Fact]
        public void Read_Throws_NotSupported()
        {
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Entry) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => deserializer.Object.Read(messageReader: null, type: null, readContext: null),
                "'ODataDeserializerProxy' does not support Read.");
        }
    }
}
