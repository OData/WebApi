// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization;
using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataEntityReferenceLinksSerializerTest
    {
        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks), messageWriter: null,
                    writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: "not uri", type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "ODataEntityReferenceLinksSerializer cannot write an object of type 'System.String'.");
        }

        public static TheoryDataSet<object> SerializationTestData
        {
            get
            {
                Uri uri1 = new Uri("http://uri1");
                Uri uri2 = new Uri("http://uri2");
                return new TheoryDataSet<object>
                {
                    new Uri[] { uri1, uri2 },

                    new ODataEntityReferenceLinks 
                    { 
                        Links = new ODataEntityReferenceLink[] 
                        { 
                            new ODataEntityReferenceLink{ Url = uri1 }, 
                            new ODataEntityReferenceLink{ Url = uri2 }
                        }
                    }
                };
            }
        }

        [Theory]
        [PropertyData("SerializationTestData")]
        public void ODataEntityReferenceLinkSerializer_Serializes_UrisAndEntityReferenceLinks(object uris)
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
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
            serializer.WriteObject(uris, typeof(ODataEntityReferenceLinks), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#Collection($ref)\"," +
                "\"value\":[{\"@odata.id\":\"http://uri1/\"},{\"@odata.id\":\"http://uri2/\"}]}",
                result);
        }
    }
}
