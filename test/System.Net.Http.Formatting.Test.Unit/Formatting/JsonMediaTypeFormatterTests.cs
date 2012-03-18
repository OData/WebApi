using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class JsonMediaTypeFormatterTests
    {
        public static List<Type> JTokenTypes
        {
            get
            {
                return new List<Type>
                {
                    typeof(JToken),
                    typeof(JValue),
                    typeof(JArray),
                    typeof(JObject)
                };
            }
        }

        public static IEnumerable<TestData> ValueAndRefTypeTestDataCollectionExceptULong
        {
            get
            {
                return CommonUnitTestDataSets.ValueAndRefTypeTestDataCollection.Except(new[] { CommonUnitTestDataSets.Ulongs });
            }
        }

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<JsonMediaTypeFormatter, MediaTypeFormatter>(TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        [Trait("Description", "JsonMediaTypeFormatter() constructor sets standard Json media types in SupportedMediaTypes.")]
        public void Constructor()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            foreach (MediaTypeHeaderValue mediaType in HttpUnitTestDataSets.StandardJsonMediaTypes)
            {
                Assert.True(formatter.SupportedMediaTypes.Contains(mediaType), String.Format("SupportedMediaTypes should have included {0}.", mediaType.ToString()));
            }
        }

        [Fact]
        [Trait("Description", "DefaultMediaType property returns application/json.")]
        public void DefaultMediaTypeReturnsApplicationJson()
        {
            MediaTypeHeaderValue mediaType = JsonMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/json", mediaType.MediaType);
        }

        [Fact]
        public void SupportEncoding_ContainDefaultEncodings()
        {
            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            Assert.Equal(2, jsonFormatter.SupportedEncodings.Count);
            Assert.Equal("utf-8", jsonFormatter.SupportedEncodings[0].WebName);
            Assert.Equal("utf-16", jsonFormatter.SupportedEncodings[1].WebName);
        }

        [Fact]
        [Trait("Description", "Indent property handles Get/Set correctly.")]
        public void IndentGetSet()
        {
            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            Assert.False(jsonFormatter.Indent);
            jsonFormatter.Indent = true;
            Assert.True(jsonFormatter.Indent);
        }

        [Fact]
        public void MaxDepth_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new JsonMediaTypeFormatter(),
                c => c.MaxDepth,
                expectedDefaultValue: 1024,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 256);
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanReadType() returns the expected results for all known value and reference types.")]
        public void CanReadTypeReturnsExpectedValues(Type variationType, object testData)
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            bool isSerializable = IsTypeSerializableWithJsonSerializer(variationType, testData);
            bool canSupport = formatter.CanReadTypeProxy(variationType);

            // If we don't agree, we assert only if the DCJ serializer says it cannot support something we think it should
            Assert.False(isSerializable != canSupport && isSerializable, String.Format("CanReadType returned wrong value for '{0}'.", variationType));

            // Ask a 2nd time to probe whether the cached result is treated the same
            canSupport = formatter.CanReadTypeProxy(variationType);
            Assert.False(isSerializable != canSupport && isSerializable, String.Format("2nd CanReadType returned wrong value for '{0}'.", variationType));
        }

        [Fact]
        [Trait("Description", "CanReadType() returns true on JsonValue.")]
        public void CanReadTypeReturnsTrueOnJsonValue()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            foreach (Type type in JTokenTypes)
            {
                Assert.True(formatter.CanReadTypeProxy(type), "formatter should have returned true.");
            }
        }

        [Fact]
        [Trait("Description", "CanWriteType() returns true on JsonValue.")]
        public void CanWriteTypeReturnsTrueOnJsonValue()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            foreach (Type type in JTokenTypes)
            {
                Assert.True(formatter.CanWriteTypeProxy(type), "formatter should have returned false.");
            }
        }

        [Theory]
        [TestDataSet(typeof(JsonMediaTypeFormatterTests), "ValueAndRefTypeTestDataCollectionExceptULong")]
        [Trait("Description", "ReadFromStream() returns all value and reference types serialized via WriteToStream.")]
        public void ReadFromAsyncStreamRoundTripsWriteToStreamAsync(Type variationType, object testData)
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;

            bool canSerialize = IsTypeSerializableWithJsonSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                object readObj = null;
                Assert.Stream.WriteAndRead(
                    stream =>
                    {
                        Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null));
                        contentHeaders.ContentLength = stream.Length;
                    },
                    stream => readObj = Assert.Task.SucceedsWithResult<object>(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null)));
                Assert.Equal(testData, readObj);
            }
        }

        [Fact]
        [Trait("Description", "ReadFromStreamAsync() roundtrips JsonValue.")]
        public void ReadFromStreamAsyncRoundTripsJToken()
        {
            string beforeMessage = "Hello World";
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            JToken before = beforeMessage;
            MemoryStream memStream = new MemoryStream();
            JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(memStream));
            before.WriteTo(jsonWriter);
            jsonWriter.Flush();
            memStream.Position = 0;

            JToken after = Assert.Task.SucceedsWithResult<object>(formatter.ReadFromStreamAsync(typeof(JToken), memStream, null, null)) as JToken;
            Assert.NotNull(after);
            string afterMessage = after.ToObject<string>();

            Assert.Equal(beforeMessage, afterMessage);
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "ReadFromStreamAsync() returns all value and reference types serialized via WriteToStreamAsync.")]
        public void ReadFromStreamAsyncRoundTripsWriteToStreamAsync(Type variationType, object testData)
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;

            bool canSerialize = IsTypeSerializableWithJsonSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
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
        [InlineData(false)]
        [InlineData(0)]
        [InlineData("")]
        public void ReadFromStreamAsync_WhenContentLengthIsZero_ReturnsDefaultTypeValue<T>(T value)
        {
            var formatter = new JsonMediaTypeFormatter();
            var content = new StringContent("");

            var result = formatter.ReadFromStreamAsync(typeof(T), content.ReadAsStreamAsync().Result,
                content.Headers, null);

            result.WaitUntilCompleted();
            Assert.Equal(default(T), (T)result.Result);
        }

        [Fact]
        public void ReadFromStreamAsync_WhenContentLengthIsNull_ReadsDataFromStream()
        {
            var formatter = new JsonMediaTypeFormatter();
            var t = new XmlMediaTypeFormatterTests.SampleType { Number = 42 };
            MemoryStream ms = new MemoryStream();
            formatter.WriteToStreamAsync(t.GetType(), t, ms, null, null).WaitUntilCompleted();
            var content = new StringContent(Encoding.Default.GetString(ms.ToArray()));
            content.Headers.ContentLength = null;

            var result = formatter.ReadFromStreamAsync(typeof(XmlMediaTypeFormatterTests.SampleType), content.ReadAsStreamAsync().Result,
                content.Headers, null);

            result.WaitUntilCompleted();
            var value = Assert.IsType<XmlMediaTypeFormatterTests.SampleType>(result.Result);
            Assert.Equal(42, value.Number);
        }

        [Fact]
        [Trait("Description", "UseDataContractJsonSerializer property works when set to true.")]
        public void UseDataContractJsonSerializer_True()
        {
            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter { UseDataContractJsonSerializer = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(jsonFormatter.WriteToStreamAsync(typeof(XmlMediaTypeFormatterTests.SampleType), new XmlMediaTypeFormatterTests.SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            //Assert.True(serializedString.Contains("DataContractSampleType"),
            //    "SampleType should be serialized with data contract name DataContractSampleType because UseDataContractJsonSerializer is set to true.");
            Assert.False(serializedString.Contains("\r\n"), "Using DCJS should emit data without indentation by default.");
        }

        [Fact]
        [Trait("Description", "UseDataContractJsonSerializer property with Indent throws when set to true.")]
        public void UseDataContractJsonSerializer_True_Indent_Throws()
        {
            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter { UseDataContractJsonSerializer = true, Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Throws<NotSupportedException>(
                () => jsonFormatter.WriteToStreamAsync(typeof(XmlMediaTypeFormatterTests.SampleType),
                    new XmlMediaTypeFormatterTests.SampleType(),
                    memoryStream, contentHeaders, transportContext: null));
        }

        [Fact]
        [Trait("Description", "UseDataContractJsonSerializer property works when set to false.")]
        public void UseDataContractJsonSerializer_False()
        {
            JsonMediaTypeFormatter xmlFormatter = new JsonMediaTypeFormatter { UseDataContractJsonSerializer = false };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(XmlMediaTypeFormatterTests.SampleType), new XmlMediaTypeFormatterTests.SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            //Assert.True(serializedString.Contains("DataContractSampleType"),
            //    "SampleType should be serialized with data contract name DataContractSampleType because UseDataContractJsonSerializer is set to true.");
            Assert.False(serializedString.Contains("\r\n"), "Using JsonSerializer should emit data without indentation by default.");
        }

        [Fact]
        [Trait("Description", "UseDataContractJsonSerializer property with Indent works when set to false.")]
        public void UseDataContractJsonSerializer_False_Indent()
        {
            JsonMediaTypeFormatter xmlFormatter = new JsonMediaTypeFormatter { UseDataContractJsonSerializer = false, Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(XmlMediaTypeFormatterTests.SampleType), new XmlMediaTypeFormatterTests.SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Console.WriteLine(serializedString);
            Assert.True(serializedString.Contains("\r\n"), "Using JsonSerializer with Indent set to true should emit data with indentation.");
        }

        [Fact]
        [Trait("Description", "OnWriteToStreamAsync() throws on null.")]
        public void WriteToStreamAsyncThrowsOnNull()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(null, new object(), Stream.Null, null, null); }, "type");
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(typeof(object), new object(), null, null, null); }, "stream");
        }

        [Fact]
        [Trait("Description", "OnWriteToStreamAsync() roundtrips JsonValue.")]
        public void WriteToStreamAsyncRoundTripsJToken()
        {
            string beforeMessage = "Hello World";
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            JToken before = new JValue(beforeMessage);
            MemoryStream memStream = new MemoryStream();

            Assert.Task.Succeeds(formatter.WriteToStreamAsync(typeof(JToken), before, memStream, null, null));
            memStream.Position = 0;
            JToken after = JToken.Load(new JsonTextReader(new StreamReader(memStream)));
            string afterMessage = after.ToObject<string>();

            Assert.Equal(beforeMessage, afterMessage);
        }

        public static IEnumerable<object[]> ReadAndWriteCorrectCharacterEncoding
        {
            get { return MediaTypeFormatterTests.ReadAndWriteCorrectCharacterEncoding; }
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public Task ReadJsonContentUsingCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            string formattedContent = "\"" + content + "\"";
            string mediaType = string.Format("application/json; charset={0}", encoding);

            // Act & assert
            return MediaTypeFormatterTests.ReadContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public Task WriteJsonContentUsingCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            string formattedContent = "\"" + content + "\"";
            string mediaType = string.Format("application/json; charset={0}", encoding);

            // Act & assert
            return MediaTypeFormatterTests.WriteContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        public class TestJsonMediaTypeFormatter : JsonMediaTypeFormatter
        {
            public bool CanReadTypeProxy(Type type)
            {
                return CanReadType(type);
            }

            public bool CanWriteTypeProxy(Type type)
            {
                return CanWriteType(type);
            }
        }

        private bool IsTypeSerializableWithJsonSerializer(Type type, object obj)
        {
            try
            {
                new DataContractJsonSerializer(type);
                if (obj != null && obj.GetType() != type)
                {
                    new DataContractJsonSerializer(obj.GetType());
                }
            }
            catch
            {
                return false;
            }

            return !Assert.Http.IsKnownUnserializable(type, obj, (t) => typeof(INotJsonSerializable).IsAssignableFrom(t));
        }
    }
}
