// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

namespace System.Net.Http.Formatting
{
    public class JsonMediaTypeFormatterTests : MediaTypeFormatterTestBase<JsonMediaTypeFormatter>
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
        void CopyConstructor()
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter()
            {
                Indent = true,
#if !NETFX_CORE // MaxDepth and DCJS not supported in client portable library
                MaxDepth = 42,
                UseDataContractJsonSerializer = true
#endif
            };

            TestJsonMediaTypeFormatter derivedFormatter = new TestJsonMediaTypeFormatter(formatter);

#if !NETFX_CORE // MaxDepth and DCJS not supported in client portable library
            Assert.Equal(formatter.MaxDepth, derivedFormatter.MaxDepth);
            Assert.Equal(formatter.UseDataContractJsonSerializer, derivedFormatter.UseDataContractJsonSerializer);
#endif
            Assert.Equal(formatter.Indent, derivedFormatter.Indent);
            Assert.Same(formatter.SerializerSettings, derivedFormatter.SerializerSettings);
            Assert.Same(formatter.SerializerSettings.ContractResolver, derivedFormatter.SerializerSettings.ContractResolver);
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

#if !NETFX_CORE // MaxDepth is not supported in portable libraries
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
#endif

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
        public void FormatterThrowsOnWriteWhenOverridenCreateFails()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ThrowAnExceptionOnCreate = true;

            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);

            // Act & Assert
            Action action = () => formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null).Wait();
            Assert.Throws<InvalidOperationException>(action, "The 'CreateJsonSerializer' method threw an exception when attempting to create a JSON serializer.");

            Assert.Null(formatter.InnerDataContractSerializer);
            Assert.NotNull(formatter.InnerJsonSerializer);
        }

        [Fact]
        public void FormatterThrowsOnWriteWhenOverridenCreateReturnsNull()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ReturnNullOnCreate = true;

            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);

            // Act & Assert
            Action action = () => formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null).Wait();
            Assert.Throws<InvalidOperationException>(action, "The 'CreateJsonSerializer' method did not return a JSON serializer.");

            Assert.Null(formatter.InnerDataContractSerializer);
            Assert.NotNull(formatter.InnerJsonSerializer);
        }

        [Fact]
        public void FormatterThrowsOnReadWhenOverridenCreateFails()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ThrowAnExceptionOnCreate = true;

            byte[] array = Encoding.UTF8.GetBytes("foo");
            MemoryStream memoryStream = new MemoryStream(array);

            HttpContent content = new StringContent("foo");

            // Act & Assert
            Action action = () => formatter.ReadFromStreamAsync(typeof(SampleType), memoryStream, content, null).Wait();
            Assert.Throws<InvalidOperationException>(action, "The 'CreateJsonSerializer' method threw an exception when attempting to create a JSON serializer.");

            Assert.Null(formatter.InnerDataContractSerializer);
            Assert.NotNull(formatter.InnerJsonSerializer);
        }

        [Fact]
        public void FormatterThrowsOnReadWhenOverridenCreateReturnsNull()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ReturnNullOnCreate = true;

            byte[] array = Encoding.UTF8.GetBytes("foo");
            MemoryStream memoryStream = new MemoryStream(array);

            HttpContent content = new StringContent("foo");

            // Act & Assert
            Action action = () => formatter.ReadFromStreamAsync(typeof(SampleType), memoryStream, content, null).Wait();

            Assert.Throws<InvalidOperationException>(action, "The 'CreateJsonSerializer' method did not return a JSON serializer.");

            Assert.Null(formatter.InnerDataContractSerializer);
            Assert.NotNull(formatter.InnerJsonSerializer);
        }

#if !NETFX_CORE
        [Fact]
        public void DataContractFormatterThrowsOnWriteWhenOverridenCreateFails()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ThrowAnExceptionOnCreate = true;
            formatter.UseDataContractJsonSerializer = true;

            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);

            // Act & Assert
            Action action = () => formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null).Wait();
            Assert.Throws<InvalidOperationException>(action, "The 'DataContractJsonSerializer' serializer cannot serialize the type 'SampleType'.");

            Assert.NotNull(formatter.InnerDataContractSerializer);
            Assert.Null(formatter.InnerJsonSerializer);
        }

        [Fact]
        public void DataContractFormatterThrowsOnWriteWhenOverridenCreateReturnsNull()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ReturnNullOnCreate = true;
            formatter.UseDataContractJsonSerializer = true;

            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);

            // Act & Assert
            Action action = () => formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null).Wait();
            Assert.Throws<InvalidOperationException>(action, "The 'DataContractJsonSerializer' serializer cannot serialize the type 'SampleType'.");

            Assert.NotNull(formatter.InnerDataContractSerializer);
            Assert.Null(formatter.InnerJsonSerializer);
        }

        [Fact]
        public void DataContractFormatterThrowsOnReadWhenOverridenCreateFails()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ThrowAnExceptionOnCreate = true;
            formatter.UseDataContractJsonSerializer = true;

            byte[] array = Encoding.UTF8.GetBytes("foo");
            MemoryStream memoryStream = new MemoryStream(array);

            HttpContent content = new StringContent("foo");

            // Act & Assert
            Action action = () => formatter.ReadFromStreamAsync(typeof(SampleType), memoryStream, content, null).Wait();
            Assert.Throws<InvalidOperationException>(action, "The 'DataContractJsonSerializer' serializer cannot serialize the type 'SampleType'.");

            Assert.NotNull(formatter.InnerDataContractSerializer);
            Assert.Null(formatter.InnerJsonSerializer);
        }

        [Fact]
        public void DataContractFormatterThrowsOnReadWhenOverridenCreateReturnsNull()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            formatter.ReturnNullOnCreate = true;
            formatter.UseDataContractJsonSerializer = true;

            byte[] array = Encoding.UTF8.GetBytes("foo");
            MemoryStream memoryStream = new MemoryStream(array);

            HttpContent content = new StringContent("foo");

            // Act & Assert
            Action action = () => formatter.ReadFromStreamAsync(typeof(SampleType), memoryStream, content, null).Wait();

            Assert.Throws<InvalidOperationException>(action, "The 'DataContractJsonSerializer' serializer cannot serialize the type 'SampleType'.");

            Assert.NotNull(formatter.InnerDataContractSerializer);
            Assert.Null(formatter.InnerJsonSerializer);
        }
#endif

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
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;

            bool canSerialize = IsTypeSerializableWithJsonSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
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

        [Fact]
        public void UseDataContractJsonSerializer_False()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter
            {
#if !NETFX_CORE // No JsonSerializer in portable libraries
                UseDataContractJsonSerializer = false
#endif
            };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Task.Succeeds(formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            //Assert.True(serializedString.Contains("DataContractSampleType"),
            //    "SampleType should be serialized with data contract name DataContractSampleType because UseDataContractJsonSerializer is set to true.");
            Assert.False(serializedString.Contains("\r\n"), "Using JsonSerializer should emit data without indentation by default.");
        }

        [Fact]
        public void UseDataContractJsonSerializer_False_Indent()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter
            {
#if !NETFX_CORE // No JsonSerializer in portable libraries
                UseDataContractJsonSerializer = false,
#endif
                Indent = true
            };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Task.Succeeds(formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("\r\n"), "Using JsonSerializer with Indent set to true should emit data with indentation.");
        }

        [Theory]
        [InlineData(typeof(IQueryable<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        public void UseJsonFormatterWithNull(Type type)
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter
            {
#if !NETFX_CORE // No JsonSerializer in portable libraries
                UseDataContractJsonSerializer = false
#endif
            };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Task.Succeeds(formatter.WriteToStreamAsync(type, null, memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("null"), "Using Json formatter to serialize null should emit 'null'.");
        }

        [Fact]
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
        [TestDataSet(typeof(HttpTestData), "ReadAndWriteCorrectCharacterEncoding")]
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
        [TestDataSet(typeof(HttpTestData), "ReadAndWriteCorrectCharacterEncoding")]
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
            public TestJsonMediaTypeFormatter()
            {
            }

            public TestJsonMediaTypeFormatter(TestJsonMediaTypeFormatter formatter)
                : base(formatter)
            {
            }

            public bool ThrowAnExceptionOnCreate { get; set; }
            public bool ReturnNullOnCreate { get; set; }
            public JsonSerializer InnerJsonSerializer { get; private set; }
            public DataContractJsonSerializer InnerDataContractSerializer { get; private set; }

            public bool CanReadTypeProxy(Type type)
            {
                return CanReadType(type);
            }

            public bool CanWriteTypeProxy(Type type)
            {
                return CanWriteType(type);
            }

            public override JsonSerializer CreateJsonSerializer()
            {
                InnerJsonSerializer = base.CreateJsonSerializer();

                if (ReturnNullOnCreate)
                {
                    return null;
                }

                if (ThrowAnExceptionOnCreate)
                {
                    throw new Exception("Throwing exception directly, since it needs to get caught by a catch all");
                }

                return InnerJsonSerializer;
            }

#if !NETFX_CORE
            public override DataContractJsonSerializer CreateDataContractSerializer(Type type)
            {
                InnerDataContractSerializer = base.CreateDataContractSerializer(type);

                if (ReturnNullOnCreate)
                {
                    return null;
                }

                if (ThrowAnExceptionOnCreate)
                {
                    throw new Exception("Throwing exception directly, since it needs to get caught by a catch all");
                }

                return InnerDataContractSerializer;
            }
#endif
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
