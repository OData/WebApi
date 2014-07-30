// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataErrorSerializerTest
    {
        [Fact]
        public void WriteObject_SupportsHttpError()
        {
            var serializer = new ODataErrorSerializer();
            var error = new HttpError("bad stuff");
            Mock<IODataResponseMessage> mockResponseMessage = new Mock<IODataResponseMessage>();
            mockResponseMessage.Setup(response => response.GetStream()).Returns(new MemoryStream());

            Assert.DoesNotThrow(() => serializer.WriteObject(error, typeof(ODataError), new ODataMessageWriter(mockResponseMessage.Object), 
                new ODataSerializerContext()));
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_Graph()
        {
            ODataErrorSerializer serializer = new ODataErrorSerializer();

            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataError),messageWriter: null, writeContext: null),
                "graph");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataErrorSerializer serializer = new ODataErrorSerializer();

            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: 42, type: typeof(ODataError), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_ErrorTypeMustBeODataErrorOrHttpError()
        {
            ODataErrorSerializer serializer = new ODataErrorSerializer();
            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(42, typeof(ODataError), ODataTestUtil.GetMockODataMessageWriter(), new ODataSerializerContext()),
                "The type 'System.Int32' is not supported by the ODataErrorSerializer. The type must be ODataError or HttpError.");
        }

        [Fact]
        public void ODataErrorSerializer_Works()
        {
            // Arrange
            ODataErrorSerializer serializer = new ODataErrorSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataError error = new ODataError { Message = "Error!!!" };

            // Act
            serializer.WriteObject(error, typeof(ODataError), new ODataMessageWriter(message), new ODataSerializerContext());

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("Error!!!", element.Descendants().Single(e => e.Name.LocalName == "message").Value);
        }
    }
}
