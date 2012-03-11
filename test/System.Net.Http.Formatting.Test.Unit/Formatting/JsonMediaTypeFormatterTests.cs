using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class JsonMediaTypeFormatterTests
    {
        public static List<Type> JsonValueTypes
        {
            get
            {
                return new List<Type>
                {
                    typeof(JsonValue),
                    typeof(JsonPrimitive),
                    typeof(JsonArray),
                    typeof(JsonObject)
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
        [Trait("Description", "CharacterEncoding property handles Get/Set correctly.")]
        public void CharacterEncodingGetSet()
        {
            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            Assert.IsType<UTF8Encoding>(jsonFormatter.CharacterEncoding);
            jsonFormatter.CharacterEncoding = Encoding.Unicode;
            Assert.Same(Encoding.Unicode, jsonFormatter.CharacterEncoding);
            jsonFormatter.CharacterEncoding = Encoding.UTF8;
            Assert.Same(Encoding.UTF8, jsonFormatter.CharacterEncoding);
        }

        [Fact]
        [Trait("Description", "CharacterEncoding property throws on invalid arguments")]
        public void CharacterEncodingSetThrows()
        {
            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { jsonFormatter.CharacterEncoding = null; }, "value");
            Assert.ThrowsArgument(() => { jsonFormatter.CharacterEncoding = Encoding.UTF32; }, "value");
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
            foreach (Type type in JsonValueTypes)
            {
                Assert.True(formatter.CanReadTypeProxy(type), "formatter should have returned true.");
            }
        }

        [Fact]
        [Trait("Description", "CanWriteType() returns true on JsonValue.")]
        public void CanWriteTypeReturnsTrueOnJsonValue()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            foreach (Type type in JsonValueTypes)
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
                    stream => Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null)),
                    stream => readObj = Assert.Task.SucceedsWithResult<object>(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null)));
                Assert.Equal(testData, readObj);
            }
        }

        [Fact]
        [Trait("Description", "ReadFromStreamAsync() roundtrips JsonValue.")]
        public void ReadFromStreamAsyncRoundTripsJsonValue()
        {
            string beforeMessage = "Hello World";
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            JsonValue before = beforeMessage;
            MemoryStream memStream = new MemoryStream();
            before.Save(memStream);
            memStream.Position = 0;

            JsonValue after = Assert.Task.SucceedsWithResult<object>(formatter.ReadFromStreamAsync(typeof(JsonValue), memStream, null, null)) as JsonValue;
            Assert.NotNull(after);
            string afterMessage = after.ReadAs<string>();

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
                    stream => Assert.Task.Succeeds(formatter.WriteToStreamAsync(variationType, testData, stream, contentHeaders, transportContext: null)),
                    stream => readObj = Assert.Task.SucceedsWithResult(formatter.ReadFromStreamAsync(variationType, stream, contentHeaders, null)));
                Assert.Equal(testData, readObj);
            }

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
        public void WriteToStreamAsyncRoundTripsJsonValue()
        {
            string beforeMessage = "Hello World";
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            JsonValue before = new JsonPrimitive(beforeMessage);
            MemoryStream memStream = new MemoryStream();

            Assert.Task.Succeeds(formatter.WriteToStreamAsync(typeof(JsonValue), before, memStream, null, null));
            memStream.Position = 0;
            JsonValue after = JsonValue.Load(memStream);
            string afterMessage = after.ReadAs<string>();

            Assert.Equal(beforeMessage, afterMessage);
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
