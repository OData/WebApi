// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataSerializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported).Object;

            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void WriteObject_Throws_NotSupported()
        {
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported) { CallBase = true }.Object;

            Assert.Throws<NotSupportedException>(
                () => serializer.WriteObject(graph: null, type: typeof(int),messageWriter: null, writeContext: null),
                "ODataSerializerProxy does not support WriteObject.");
        }
    }
}
