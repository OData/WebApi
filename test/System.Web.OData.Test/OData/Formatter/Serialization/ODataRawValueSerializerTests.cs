// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Core;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataRawValueSerializerTests
    {
        [Theory]
        [InlineData(5)]
        [InlineData(5u)]
        [InlineData(5L)]
        [InlineData(5f)]
        [InlineData(5d)]
        [InlineData("test")]
        [InlineData(false)]
        [InlineData('t')]
        public void SerializesPrimitiveTypes(object value)
        {
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);

            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            Assert.Equal(value.ToString(), result, ignoreCase: true);
        }

        [Fact]
        public void SerializesNullablePrimitiveTypes()
        {
            int? value = 5;
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);

            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);

            Assert.Equal(value.ToString(), reader.ReadToEnd());
        }

        [Fact]
        public void SerializesEnumType()
        {
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);
            object value = Color.Red | Color.Blue;

            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            Assert.Equal(value.ToString(), result, ignoreCase: true);
        }
    }
}
