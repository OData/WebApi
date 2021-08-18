//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
