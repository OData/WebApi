//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkSerializerTest.cs" company=".NET Foundation">
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
    public class ODataEntityReferenceLinkSerializerTest
    {
        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink), messageWriter: null,
                    writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
        {
            // Arrange
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: "not uri", type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "ODataEntityReferenceLinkSerializer cannot write an object of type 'System.String'.");
        }

        public static TheoryDataSet<object> SerializationTestData
        {
            get
            {
                Uri uri = new Uri("http://sampleuri/");
                return new TheoryDataSet<object>
                {
                    uri,
                    new ODataEntityReferenceLink { Url = uri }
                };
            }
        }

        [Theory]
        [MemberData(nameof(SerializationTestData))]
        public void ODataEntityReferenceLinkSerializer_Serializes_Uri(object link)
        {
            // Arrange
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };

            settings.SetContentType(ODataFormat.Json);
            ODataMessageWriter writer = new ODataMessageWriter(message, settings);

            // Act
            serializer.WriteObject(link, typeof(ODataEntityReferenceLink), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#$ref\",\"@odata.id\":\"http://sampleuri/\"}", result);
        }
    }
}
