// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class JsonMediaTypeFormatterTests : MediaTypeFormatterTestBase<JsonMediaTypeFormatter>
    {
        public static IEnumerable<object[]> ReadAndWriteCorrectCharacterEncoding
        {
            get { return HttpTestData.ReadAndWriteCorrectCharacterEncoding; }
        }

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

        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get { return HttpTestData.StandardJsonMediaTypes; }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get { return HttpTestData.StandardEncodings; }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get { return ExpectedSupportedEncodings.ElementAt(0).GetBytes("{\"Number\":42}"); }
        }

        [Fact]
        public void DefaultMediaType_ReturnsApplicationJson()
        {
            MediaTypeHeaderValue mediaType = JsonMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/json", mediaType.MediaType);
        }

        [Fact]
        public void Indent_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new XmlMediaTypeFormatter(),
                c => c.Indent,
                expectedDefaultValue: false);
        }

        [Fact]
        public void MaxDepth_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new JsonMediaTypeFormatter(),
                c => c.MaxDepth,
                expectedDefaultValue: 256,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 256);
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        public void CanReadType_ReturnsExpectedValues(Type variationType, object testData)
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
        public void CanReadType_ReturnsTrueOnJtoken()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            foreach (Type type in JTokenTypes)
            {
                Assert.True(formatter.CanReadTypeProxy(type), "formatter should have returned true.");
            }
        }

        [Fact]
        public void CanWriteType_ReturnsTrueOnJtoken()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            foreach (Type type in JTokenTypes)
            {
                Assert.True(formatter.CanWriteTypeProxy(type), "formatter should have returned false.");
            }
        }

        [Fact]
        public void ReadFromStreamAsync_RoundTripsJToken()
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
        [TestDataSet(typeof(JsonMediaTypeFormatterTests), "ValueAndRefTypeTestDataCollectionExceptULong")]
        public void ReadFromStreamAsync_RoundTripsWriteToStreamAsync(Type variationType, object testData)
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

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

        [Fact]
        [Trait("Description", "UseDataContractJsonSerializer property works when set to false.")]
        public void UseDataContractJsonSerializer_False()
        {
            JsonMediaTypeFormatter xmlFormatter = new JsonMediaTypeFormatter { UseDataContractJsonSerializer = false };
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
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
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(typeof(XmlMediaTypeFormatterTests.SampleType), new XmlMediaTypeFormatterTests.SampleType(), memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Console.WriteLine(serializedString);
            Assert.True(serializedString.Contains("\r\n"), "Using JsonSerializer with Indent set to true should emit data with indentation.");
        }

        [Theory]
        [InlineData(typeof(IQueryable<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        public void UseJsonFormatterWithNull(Type type)
        {
            JsonMediaTypeFormatter xmlFormatter = new JsonMediaTypeFormatter { UseDataContractJsonSerializer = false};
            MemoryStream memoryStream = new MemoryStream();
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            Assert.Task.Succeeds(xmlFormatter.WriteToStreamAsync(type, null, memoryStream, contentHeaders, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("null"), "Using Json formatter to serialize null should emit 'null'.");
        }

        [Fact]
        [Trait("Description", "OnWriteToStreamAsync() roundtrips JsonValue.")]
        public void WriteToStreamAsync_RoundTripsJToken()
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

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            string formattedContent = "\"" + content + "\"";
            string mediaType = string.Format("application/json; charset={0}", encoding);

            // Act & assert
            return ReadContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            string formattedContent = "\"" + content + "\"";
            string mediaType = string.Format("application/json; charset={0}", encoding);

            // Act & assert
            return WriteContentUsingCorrectCharacterEncodingHelper(
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
