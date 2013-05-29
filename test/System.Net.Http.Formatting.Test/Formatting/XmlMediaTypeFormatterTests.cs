// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.TestCommon;
using Moq;
using Newtonsoft.Json;

namespace System.Net.Http.Formatting
{
    public class XmlMediaTypeFormatterTests : MediaTypeFormatterTestBase<XmlMediaTypeFormatter>
    {
        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get { return HttpTestData.StandardXmlMediaTypes; }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get { return HttpTestData.StandardEncodings; }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get { return ExpectedSupportedEncodings.ElementAt(0).GetBytes("<DataContractSampleType xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/System.Net.Http.Formatting\"><Number>42</Number></DataContractSampleType>"); }
        }

        [Fact]
        public void DefaultMediaType_ReturnsApplicationXml()
        {
            MediaTypeHeaderValue mediaType = XmlMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/xml", mediaType.MediaType);
        }

#if !NETFX_CORE // We don't support MaxDepth in the portable library
        [Fact]
        public void MaxDepthReturnsCorrectValue()
        {
            Assert.Reflection.IntegerProperty(
                new XmlMediaTypeFormatter(),
                f => f.MaxDepth,
                expectedDefaultValue: 256,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 10);
        }

        [Fact]
        public void ReadDeeplyNestedObjectThrows()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter() { MaxDepth = 1 };

            MemoryStream stream = new MemoryStream();
            formatter.WriteToStreamAsync(typeof(SampleType), new SampleType() { Number = 1 }, stream, null, null).Wait();
            stream.Position = 0;
            Task task = formatter.ReadFromStreamAsync(typeof(SampleType), stream, null, null);
            Assert.Throws<SerializationException>(() => task.Wait());
        }
#endif

        [Fact]
        public void Indent_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new XmlMediaTypeFormatter(),
                c => c.Indent,
                expectedDefaultValue: false);
        }

        [Fact]
        public void UseXmlSerializer_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new XmlMediaTypeFormatter(),
                c => c.UseXmlSerializer,
                expectedDefaultValue: false);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(IQueryable<string>))]
        public void UseXmlFormatterWithNull(Type type)
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = false };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(type, null, memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("nil=\"true\""),
                "Null value should be serialized as nil.");
            Assert.True(serializedString.ToLower().Contains("arrayofstring"),
                "It should be serialized out as an array of string.");
        }

        [Fact]
        public void UseXmlSerializer_False()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = false };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("DataContractSampleType"),
                "SampleType should be serialized with data contract name DataContractSampleType because we're using DCS.");
            Assert.False(serializedString.Contains("version=\"1.0\" encoding=\"utf-8\""),
                    "Using DCS should not emit the xml declaration by default.");
            Assert.False(serializedString.Contains("\r\n"), "Using DCS should emit data without indentation by default.");
        }

        [Fact]
        public void UseXmlSerializer_False_Indent()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = false, Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("\r\n"), "Using DCS with indent set to true should emit data with indentation.");
        }

        [Fact]
        public void SetSerializer_ThrowsWithNullType()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(null, xmlSerializer); }, "type");
        }

        [Fact]
        public void SetSerializer_ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(typeof(string), (XmlSerializer)null); }, "serializer");
        }

        [Fact]
        public void SetSerializer1_ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer<string>((XmlSerializer)null); }, "serializer");
        }

        [Fact]
        public void SetSerializer2_ThrowsWithNullType()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            XmlObjectSerializer xmlObjectSerializer = new DataContractSerializer(typeof(string));
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(null, xmlObjectSerializer); }, "type");
        }

        [Fact]
        public void SetSerializer2_ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(typeof(string), (XmlObjectSerializer)null); }, "serializer");
        }

        [Fact]
        public void SetSerializer3_ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer<string>((XmlSerializer)null); }, "serializer");
        }

        [Fact]
        public void RemoveSerializer_ThrowsWithNullType()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.RemoveSerializer(null); }, "type");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        public void ReadFromStreamAsync_RoundTripsWriteToStreamAsyncUsingXmlSerializer(Type variationType, object testData)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;

            bool canSerialize = IsSerializableWithXmlSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                formatter.SetSerializer(variationType, new XmlSerializer(variationType));

                object readObj = null;
                Assert.Stream.WriteAndRead(
                    stream =>
                    {
                        Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, content, transportContext: null));
                        contentHeaders.ContentLength = stream.Length;
                    },
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, content, null)));
                Assert.Equal(testData, readObj);
            }
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        public void ReadFromStream_AsyncRoundTripsWriteToStreamUsingDataContractSerializer(Type variationType, object testData)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;

            bool canSerialize = IsSerializableWithDataContractSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                formatter.SetSerializer(variationType, new DataContractSerializer(variationType));

                object readObj = null;
                Assert.Stream.WriteAndRead(
                    stream =>
                    {
                        Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, content, transportContext: null));
                        contentHeaders.ContentLength = stream.Length;
                    },
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, content, null))
                    );
                Assert.Equal(testData, readObj);
            }
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "ReadAndWriteCorrectCharacterEncoding")]
        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            if (!isDefaultEncoding)
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            
            string formattedContent = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + content + "</string>";
#if NETFX_CORE
            // We need to supply the xml declaration when compiled in portable library for non utf-8 content
            if (String.Equals("utf-16", encoding, StringComparison.OrdinalIgnoreCase))
            {
                formattedContent = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" + formattedContent;
            }
#endif
            string mediaType = string.Format("application/xml; charset={0}", encoding);

            // Act & assert
            return ReadFromStreamAsync_UsesCorrectCharacterEncodingHelper(formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Fact]
        public void ReadFromStreamAsync_UsesGetDeserializerAndCreateXmlReader()
        {
            Type type = typeof(string);
            string xml = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">x</string>";
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var serializer = new Mock<XmlObjectSerializer>() { CallBase = true };
            var reader = new Mock<XmlReader>() { CallBase = true };
            var formatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            formatter.Setup(f => f.GetDeserializer(type, null)).Returns(serializer.Object);
            formatter.Setup(f => f.CreateXmlReader(stream, null)).Returns(reader.Object);

            formatter.Object.ReadFromStreamAsync(type, stream, content: null, formatterLogger: null).Wait();

            serializer.Verify(s => s.ReadObject(reader.Object));
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsException_WhenGetDeserializerReturnsNull()
        {
            Type type = typeof(string);
            string xml = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">x</string>";
            var formatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            formatter.Setup(f => f.GetDeserializer(type, null)).Returns(null);

            Assert.Throws<InvalidOperationException>(
                () => formatter.Object.ReadFromStreamAsync(type, new MemoryStream(Encoding.UTF8.GetBytes(xml)), content: null, formatterLogger: null).Wait(),
                "The object returned by GetDeserializer must not be a null value.");
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsException_WhenGetDeserializerReturnsInvalidType()
        {
            Type type = typeof(string);
            string xml = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">x</string>";
            var formatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            formatter.Setup(f => f.GetDeserializer(type, null)).Returns(new JsonSerializer());

            Assert.Throws<InvalidOperationException>(
                () => formatter.Object.ReadFromStreamAsync(type, new MemoryStream(Encoding.UTF8.GetBytes(xml)), content: null, formatterLogger: null).Wait(),
                "The object of type 'JsonSerializer' returned by GetDeserializer must be an instance of either XmlObjectSerializer or XmlSerializer.");
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "ReadAndWriteCorrectCharacterEncoding")]
        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            if (!isDefaultEncoding)
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            string formattedContent = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + content +
                                      "</string>";
            string mediaType = string.Format("application/xml; charset={0}", encoding);

            // Act & assert
            return WriteToStreamAsync_UsesCorrectCharacterEncodingHelper(formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Fact]
        public void WriteToStreamAsync_UsesGetSerializerAndCreateXmlWriter()
        {
            Type type = typeof(string);
            object value = "x";
            Stream stream = new MemoryStream();
            var serializer = new Mock<XmlObjectSerializer>() { CallBase = true };
            var writer = new Mock<XmlWriter>() { CallBase = true };
            var formatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            formatter.Setup(f => f.GetSerializer(type, value, null)).Returns(serializer.Object);
            formatter.Setup(f => f.CreateXmlWriter(stream, null)).Returns(writer.Object);

            formatter.Object.WriteToStreamAsync(type, value, stream, content: null, transportContext: null).Wait();

            serializer.Verify(s => s.WriteObject(writer.Object, value));
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsException_WhenGetSerializerReturnsNull()
        {
            Type type = typeof(string);
            object value = "x";
            var formatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            formatter.Setup(f => f.GetSerializer(type, value, null)).Returns(null);

            Assert.Throws<InvalidOperationException>(
                () => formatter.Object.WriteToStreamAsync(type, value, new MemoryStream(), content: null, transportContext: null).Wait(),
                "The object returned by GetSerializer must not be a null value.");
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsException_WhenGetSerializerReturnsInvalidType()
        {
            Type type = typeof(string);
            object value = "x";
            var formatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            formatter.Setup(f => f.GetSerializer(type, value, null)).Returns(new JsonSerializer());

            Assert.Throws<InvalidOperationException>(
                () => formatter.Object.WriteToStreamAsync(type, value, new MemoryStream(), content: null, transportContext: null).Wait(),
                "The object of type 'JsonSerializer' returned by GetSerializer must be an instance of either XmlObjectSerializer or XmlSerializer.");
        }

#if !NETFX_CORE // Different behavior in portable libraries due to no DataContract validation
        [Fact]
        public void CanReadType_ReturnsFalse_ForInvalidDataContracts()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();

            Assert.False(formatter.CanReadType(typeof(InvalidDataContract)));
        }

        [Fact]
        public void CanWriteType_ReturnsFalse_ForInvalidDataContracts()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();

            Assert.False(formatter.CanWriteType(typeof(InvalidDataContract)));
        }
#else
        [Fact]
        public void CanReadType_InPortableLibrary_ReturnsFalse_ForInvalidDataContracts()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();

            // The formatter is unable to positively identify non readable types, so true is always returned
            Assert.True(formatter.CanReadType(typeof(InvalidDataContract)));
        }

        [Fact]
        public void CanWriteType_InPortableLibrary_ReturnsTrue_ForInvalidDataContracts()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();

            // The formatter is unable to positively identify non readable types, so true is always returned
            Assert.True(formatter.CanWriteType(typeof(InvalidDataContract)));
        }
#endif

        public class InvalidDataContract
        {
            // removing the default ctor makes this invalid
            public InvalidDataContract(string s)
            {
            }
        }

        public class TestXmlMediaTypeFormatter : XmlMediaTypeFormatter
        {
            public bool CanReadTypeCaller(Type type)
            {
                return CanReadType(type);
            }

            public bool CanWriteTypeCaller(Type type)
            {
                return CanWriteType(type);
            }
        }

        private bool IsSerializableWithXmlSerializer(Type type, object obj)
        {
            if (Assert.Http.IsKnownUnserializable(type, obj))
            {
                return false;
            }

            try
            {
                new XmlSerializer(type);
                if (obj != null && obj.GetType() != type)
                {
                    new XmlSerializer(obj.GetType());
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool IsSerializableWithDataContractSerializer(Type type, object obj)
        {
            if (Assert.Http.IsKnownUnserializable(type, obj))
            {
                return false;
            }

            try
            {
                new DataContractSerializer(type);
                if (obj != null && obj.GetType() != type)
                {
                    new DataContractSerializer(obj.GetType());
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
