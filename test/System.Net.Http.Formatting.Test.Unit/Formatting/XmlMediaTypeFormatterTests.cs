using System.IO;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
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
        [Trait("Description", "CharacterEncoding property handles Get/Set correctly.")]
        public void CharacterEncodingGetSet()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter();
            Assert.IsType<UTF8Encoding>(xmlFormatter.CharacterEncoding);
            xmlFormatter.CharacterEncoding = Encoding.Unicode;
            Assert.Same(Encoding.Unicode, xmlFormatter.CharacterEncoding);
            xmlFormatter.CharacterEncoding = Encoding.UTF8;
            Assert.Same(Encoding.UTF8, xmlFormatter.CharacterEncoding);
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
        [Trait("Description", "CharacterEncoding property throws on invalid arguments")]
        public void CharacterEncodingSetThrows()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { xmlFormatter.CharacterEncoding = null; }, "value");
            Assert.ThrowsArgument(() => { xmlFormatter.CharacterEncoding = Encoding.UTF32; }, "value");
        }

        [Fact]
        [Trait("Description", "UseDataContractSerializer property should be false by default.")]
        public void UseDataContractSerializer_Default()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter();
            Assert.False(xmlFormatter.UseDataContractSerializer, "The UseDataContractSerializer property should be false by default.");
        }

        [Fact]
        [Trait("Description", "UseDataContractSerializer property works when set to true.")]
        public void UseDataContractSerializer_True()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseDataContractSerializer = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("DataContractSampleType"),
                "SampleType should be serialized with data contract name DataContractSampleType because UseDataContractSerializer is set to true.");
            Assert.False(serializedString.Contains("version=\"1.0\" encoding=\"utf-8\""),
                    "Using DCS should not emit the xml declaration by default.");
            Assert.False(serializedString.Contains("\r\n"), "Using DCS should emit data without indentation by default.");
        }

        [Fact]
        [Trait("Description", "UseDataContractSerializer property with Indent works when set to true.")]
        public void UseDataContractSerializer_True_Indent()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseDataContractSerializer = true, Indent = true};
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("\r\n"), "Using DCS with indent set to true should emit data with indentation.");
        }

        [Fact]
        [Trait("Description", "UseDataContractSerializer property works when set to false.")]
        public void UseDataContractSerializer_False()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseDataContractSerializer = false };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.False(serializedString.Contains("DataContractSampleType"),
                "SampleType should not be serialized with data contract name DataContractSampleType because UseDataContractSerializer is set to false.");
            Assert.False(serializedString.Contains("version=\"1.0\" encoding=\"utf-8\""),
              "Using XmlSerializer should not emit the xml declaration by default.");
            Assert.False(serializedString.Contains("\r\n"), "Using default XmlSerializer should emit data without indentation.");
        }

        [Fact]
        [Trait("Description", "UseDataContractSerializer property with Indent works when set to false.")]
        public void UseDataContractSerializer_False_Indent()
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlMediaTypeFormatter { UseDataContractSerializer = false, Indent = true};
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
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();

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

        [Theory]
        [TestDataSet(typeof(JsonMediaTypeFormatterTests), "JsonValueTypes")]
        [Trait("Description", "CanReadType() returns false on JsonValue.")]
        public void CanReadTypeReturnsFalseOnJsonValue(Type type)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();
            Assert.False(formatter.CanReadTypeCaller(type), "formatter should have returned false.");
        }

        [Theory]
        [TestDataSet(typeof(JsonMediaTypeFormatterTests), "JsonValueTypes")]
        [Trait("Description", "CanWriteType() returns false on JsonValue.")]
        public void CanWriteTypeReturnsFalseOnJsonValue(Type type)
        {
            TestXmlMediaTypeFormatter formatter = new TestXmlMediaTypeFormatter();
            Assert.False(formatter.CanWriteTypeCaller(type), "formatter should have returned false.");
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
                    stream => Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null)),
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null))
                    );
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
                    stream => Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null)),
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null))
                    );
                Assert.Equal(testData, readObj);
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
