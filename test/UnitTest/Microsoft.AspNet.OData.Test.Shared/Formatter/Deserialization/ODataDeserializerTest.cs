// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public class ODataDeserializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            // Arrange
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Unsupported);

            // Act & Assert
            Assert.Equal(ODataPayloadKind.Unsupported, deserializer.Object.ODataPayloadKind);
        }

        [Fact]
        public void Read_Throws_NotSupported()
        {
            // Arrange
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Resource) { CallBase = true };

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(
                () => deserializer.Object.Read(messageReader: null, type: null, readContext: null),
                "'ODataDeserializerProxy' does not support Read.");
        }
    }
}
