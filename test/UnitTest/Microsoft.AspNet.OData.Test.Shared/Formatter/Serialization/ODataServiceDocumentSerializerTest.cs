//-----------------------------------------------------------------------------
// <copyright file="ODataServiceDocumentSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataServiceDocumentSerializerTest
    {
        private Type _workspaceType = typeof(ODataServiceDocument);

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(42, _workspaceType, messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_Graph()
        {
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(null, type: _workspaceType, messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_CannotWriteType()
        {
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObject(42, _workspaceType, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "ODataServiceDocumentSerializer cannot write an object of type 'ODataServiceDocument'.");
        }

        [Fact]
        public void ODataServiceDocumentSerializer_Works()
        {
            // Arrange
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter writer = new ODataMessageWriter(message, settings);

            // Act
            serializer.WriteObject(new ODataServiceDocument(), _workspaceType, writer, new ODataSerializerContext());
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata\",\"value\":[]}", result);
        }
    }
}
