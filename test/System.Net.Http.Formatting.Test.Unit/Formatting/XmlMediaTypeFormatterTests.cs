using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class XmlMediaTypeFormatterTests
    {
        [Fact]
        [Trait("Description", "XmlMediaTypeFormatter is public, concrete, and unsealed.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(XmlMediaTypeFormatter), TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "StandardXmlMediaTypes")]
        [Trait("Description", "XmlMediaTypeFormatter() constructor sets standard Xml media types in SupportedMediaTypes.")]
        public void Constructor(MediaTypeHeaderValue mediaType)
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.True(formatter.SupportedMediaTypes.Contains(mediaType), String.Format("SupportedMediaTypes should have included {0}.", mediaType.ToString()));
        }

        [Fact]
        [Trait("Description", "DefaultMediaType property returns application/xml.")]
        public void DefaultMediaTypeReturnsApplicationXml()
        {
            MediaTypeHeaderValue mediaType = XmlMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/xml", mediaType.MediaType);
        }

        [Fact]
        public void SupportEncoding_ContainDefaultEncodings()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter();
            Assert.Equal(2, xmlFormatter.SupportedEncodings.Count);
            Assert.Equal("utf-8", xmlFormatter.SupportedEncodings[0].WebName);
            Assert.Equal("utf-16", xmlFormatter.SupportedEncodings[1].WebName);
        }

        [Fact]
        [Trait("Description", "Indent property handles Get/Set correctly.")]
        public void IndentGetSet()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter();
            Assert.False(xmlFormatter.Indent);
            xmlFormatter.Indent = true;
            Assert.True(xmlFormatter.Indent);
        }

        [Fact]
        [Trait("Description", "UseXmlSerializer property should be false by default.")]
        public void UseXmlSerializer_Default()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter();
            Assert.False(xmlFormatter.UseXmlSerializer, "The UseXmlSerializer property should be false by default.");
        }

        [Fact]
        [Trait("Description", "UseXmlSerializer property works when set to false.")]
        public void UseXmlSerializer_False()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = false };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("DataContractSampleType"),
                "SampleType should be serialized with data contract name DataContractSampleType because we're using DCS.");
            Assert.False(serializedString.Contains("version=\"1.0\" encoding=\"utf-8\""),
                    "Using DCS should not emit the xml declaration by default.");
            Assert.False(serializedString.Contains("\r\n"), "Using DCS should emit data without indentation by default.");
        }

        [Fact]
        [Trait("Description", "UseXmlSerializer property with Indent works when set to false.")]
        public void UseXmlSerializer_False_Indent()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = false, Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("\r\n"), "Using DCS with indent set to true should emit data with indentation.");
        }

        [Fact]
        [Trait("Description", "UseXmlSerializer property works when set to true.")]
        public void UseXmlSerializer_True()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.False(serializedString.Contains("DataContractSampleType"),
                "SampleType should not be serialized with data contract name DataContractSampleType because UseXmlSerializer is set to true.");
            Assert.False(serializedString.Contains("version=\"1.0\" encoding=\"utf-8\""),
              "Using XmlSerializer should not emit the xml declaration by default.");
            Assert.False(serializedString.Contains("\r\n"), "Using default XmlSerializer should emit data without indentation.");
        }

        [Fact]
        [Trait("Description", "UseXmlSerializer property with Indent works when set to true.")]
        public void UseXmlSerializer_True_Indent()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseXmlSerializer = true, Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("\r\n"), "Using default XmlSerializer with Indent set to true should emit data with indentation.");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanReadType() returns the same result as the XmlSerializer constructor.")]
        public void CanReadTypeReturnsSameResultAsXmlSerializerConstructor(Type variationType, object testData)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter() { UseXmlSerializer = true };

            bool isSerializable = IsSerializableWithXmlSerializer(variationType, testData);
            bool canSupport = formatter.CanReadTypeCaller(variationType);
            if (isSerializable != canSupport)
            {
                Assert.Equal(isSerializable, canSupport);
            }

            // Ask a 2nd time to probe whether the cached result is treated the same
            canSupport = formatter.CanReadTypeCaller(variationType);
            Assert.Equal(isSerializable, canSupport);

        }

        [Fact]
        [Trait("Description", "SetSerializer(Type, XmlSerializer) throws with null type.")]
        public void SetSerializerThrowsWithNullType()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(null, xmlSerializer); }, "type");
        }

        [Fact]
        [Trait("Description", "SetSerializer(Type, XmlSerializer) throws with null serializer.")]
        public void SetSerializerThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(typeof(string), (XmlSerializer)null); }, "serializer");
        }

        [Fact]
        [Trait("Description", "SetSerializer<T>(XmlSerializer) throws with null serializer.")]
        public void SetSerializer1ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer<string>((XmlSerializer)null); }, "serializer");
        }

        [Fact]
        [Trait("Description", "SetSerializer(Type, XmlObjectSerializer) throws with null type.")]
        public void SetSerializer2ThrowsWithNullType()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            XmlObjectSerializer xmlObjectSerializer = new DataContractSerializer(typeof(string));
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(null, xmlObjectSerializer); }, "type");
        }

        [Fact]
        [Trait("Description", "SetSerializer(Type, XmlObjectSerializer) throws with null serializer.")]
        public void SetSerializer2ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(typeof(string), (XmlObjectSerializer)null); }, "serializer");
        }

        [Fact]
        [Trait("Description", "SetSerializer<T>(XmlObjectSerializer) throws with null serializer.")]
        public void SetSerializer3ThrowsWithNullSerializer()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer<string>((XmlSerializer)null); }, "serializer");
        }

        [Fact]
        [Trait("Description", "RemoveSerializer throws with null type.")]
        public void RemoveSerializerThrowsWithNullType()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.RemoveSerializer(null); }, "type");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "ReadFromStreamAsync() returns all value and reference types serialized via WriteToStreamAsync using XmlSerializer.")]
        public void ReadFromStreamAsyncRoundTripsWriteToStreamAsyncUsingXmlSerializer(Type variationType, object testData)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;

            bool canSerialize = IsSerializableWithXmlSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                formatter.SetSerializer(variationType, new XmlSerializer(variationType));

                object readObj = null;
                Assert.Stream.WriteAndRead(
                    stream =>
                    {
                        Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null));
                        contentHeaders.ContentLength = stream.Length;
                    },
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null)));
                Assert.Equal(testData, readObj);
            }
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "ReadFromStream() returns all value and reference types serialized via WriteToStream using DataContractSerializer.")]
        public void ReadFromStreamAsyncRoundTripsWriteToStreamUsingDataContractSerializer(Type variationType, object testData)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;

            bool canSerialize = IsSerializableWithDataContractSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                formatter.SetSerializer(variationType, new DataContractSerializer(variationType));

                object readObj = null;
                Assert.Stream.WriteAndRead(
                    stream =>
                    {
                        Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null));
                        contentHeaders.ContentLength = stream.Length;
                    },
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null))
                    );
                Assert.Equal(testData, readObj);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(0)]
        [InlineData("")]
        public void ReadFromStreamAsync_WhenContentLengthIsZero_ReturnsDefaultTypeValue<T>(T value)
        {
            var formatter = new XmlMediaTypeFormatter();
            var content = new StringContent("");

            var result = formatter.ReadFromStreamAsync(typeof(T), content.ReadAsStreamAsync().Result,
                content.Headers, null);

            result.WaitUntilCompleted();
            Assert.Equal(default(T), (T)result.Result);
        }

        [Fact]
        public void ReadFromStreamAsync_WhenContentLengthIsNull_ReadsDataFromStream()
        {
            var formatter = new XmlMediaTypeFormatter();
            SampleType t = new SampleType { Number = 42 };
            MemoryStream ms = new MemoryStream();
            formatter.WriteToStreamAsync(t.GetType(), t, ms, null, null).WaitUntilCompleted();
            var content = new StringContent(Encoding.Default.GetString(ms.ToArray()));
            content.Headers.ContentLength = null;

            var result = formatter.ReadFromStreamAsync(typeof(SampleType), content.ReadAsStreamAsync().Result,
                content.Headers, null);

            result.WaitUntilCompleted();
            var value = Assert.IsType<SampleType>(result.Result);
            Assert.Equal(42, value.Number);
        }

        public static IEnumerable<object[]> ReadAndWriteCorrectCharacterEncoding
        {
            get { return MediaTypeFormatterTests.ReadAndWriteCorrectCharacterEncoding; }
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public Task ReadXmlContentUsingCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            if (!isDefaultEncoding)
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            string formattedContent = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + content + "</string>";
            string mediaType = string.Format("application/xml; charset={0}", encoding);

            // Act & assert
            return MediaTypeFormatterTests.ReadContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public Task WriteXmlContentUsingCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
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
            return MediaTypeFormatterTests.WriteContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
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

        [DataContract(Name = "DataContractSampleType")]
        public class SampleType
        {
            [DataMember]
            public int Number { get; set; }
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
