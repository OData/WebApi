// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataMetadataSerializerTest
    {
        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(42, typeof(IEdmModel), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void ODataMetadataSerializer_Works()
        {
            // Arrange
            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
            IEdmModel model = new EdmModel();

            // Act
            serializer.WriteObject("42", typeof(IEdmModel), new ODataMessageWriter(message, settings, model), new ODataSerializerContext());

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("Edmx", element.Name.LocalName);
        }
    }
}
