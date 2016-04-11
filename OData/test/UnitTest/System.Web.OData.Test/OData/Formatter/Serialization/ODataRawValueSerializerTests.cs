// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

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

        public static TheoryDataSet<object, DateTimeOffset> DateTimeTestData
        {
            get
            {
                DateTime dt = DateTime.UtcNow;
                DateTimeOffset dto = new DateTimeOffset(dt).ToLocalTime();
                return new TheoryDataSet<object, DateTimeOffset>
                {
                    { dt, dto},
                    { new DateTime?(dt), dto}
                };
            }
        }

        [Theory]
        [PropertyData("DateTimeTestData")]
        public void SerializesDateTimeTypes(object value, DateTimeOffset expect)
        {
            // Arrange
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);

            // Act
            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);

            // Assert
            Assert.Equal(expect, DateTimeOffset.Parse(reader.ReadToEnd()));
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

        [Fact]
        public void SerializesReturnedCountValue()
        {
            // Arrange
            var serializer = new ODataRawValueSerializer();
            var mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            var messageWriter = new ODataMessageWriter(mockRequest.Object);
            var request = new HttpRequestMessage();
            request.ODataProperties().Path = new ODataPath(CountSegment.Instance);
            var context = new ODataSerializerContext { Request = request };

            // Act
            serializer.WriteObject(5, null, messageWriter, context);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            // Assert
            Assert.Equal("5", result);
        }
    }
}