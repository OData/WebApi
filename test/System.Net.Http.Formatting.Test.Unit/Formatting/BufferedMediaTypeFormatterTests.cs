// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;
using Xunit.Extensions;
using System.Threading.Tasks;

namespace System.Net.Http.Formatting
{
    public class BufferedMediaTypeFormatterTests : MediaTypeFormatterTestBase<MockBufferedMediaTypeFormatter>
    {
        private const string ExpectedSupportedMediaType = "text/test";
        private const string TestData = "Hello World Hello World Hello World Hello World Hello World Hello World";

        public static IEnumerable<object[]> ReadAndWriteCorrectCharacterEncoding
        {
            get { return HttpTestData.ReadAndWriteCorrectCharacterEncoding; }
        }

        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get { return new List<MediaTypeHeaderValue> { new MediaTypeHeaderValue(ExpectedSupportedMediaType) }; }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get { return HttpTestData.StandardEncodings; }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get { return ExpectedSupportedEncodings.ElementAt(0).GetBytes("System.Net.Http.Formatting.MediaTypeFormatterTestBase`1+SampleType[System.Net.Http.Formatting.MockBufferedMediaTypeFormatter]"); }
        }

        [Fact]
        public void BufferSize_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new MockBufferedMediaTypeFormatter(),
                c => c.BufferSize,
                expectedDefaultValue: 16 * 1024,
                minLegalValue: 0,
                illegalLowerValue: -1,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 1024);
        }

        [Fact]
        public void WriteToStreamAsync_WhenTypeParameterIsNull_ThrowsException()
        {
            BufferedMediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(
                () => formatter.WriteToStreamAsync(null, new object(), new MemoryStream(), null, null), "type");
        }

        [Fact]
        public void WriteToStreamAsync_WhenStreamParameterIsNull_ThrowsException()
        {
            BufferedMediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(
                () => formatter.WriteToStreamAsync(typeof(object), new object(), null, null, null), "stream");
        }

        [Fact]
        public void ReadFromStreamAsync_WhenTypeParamterIsNull_ThrowsException()
        {
            BufferedMediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => formatter.ReadFromStreamAsync(null, new MemoryStream(), null, null), "type");
        }

        [Fact]
        public void ReadFromStreamAsync_WhenStreamParamterIsNull_ThrowsException()
        {
            BufferedMediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => formatter.ReadFromStreamAsync(typeof(object), null, null, null), "stream");
        }

        [Fact]
        public void BufferedWrite()
        {
            // Arrange. Specifically use the base class with async signatures. 
            MediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            MemoryStream output = new MemoryStream();

            // Act. Call the async signature.
            formatter.WriteToStreamAsync(TestData.GetType(), TestData, output, null, null).Wait();

            // Assert
            byte[] expectedBytes = ExpectedSupportedEncodings.ElementAt(0).GetBytes(TestData);
            byte[] actualBytes = output.ToArray();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Fact]
        public void BufferedRead()
        {
            // Arrange. Specifically use the base class with async signatures. 
            MediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            byte[] expectedBytes = ExpectedSupportedEncodings.ElementAt(0).GetBytes(TestData);
            MemoryStream input = new MemoryStream(expectedBytes);

            // Act. Call the async signature.
            object result = formatter.ReadFromStreamAsync(TestData.GetType(), input, null, null).Result;

            // Assert
            Assert.Equal(TestData, result);
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            MediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            string mediaType = string.Format("{0}; charset={1}", ExpectedSupportedMediaType, encoding);

            // Act & assert
            return ReadFromStreamAsync_UsesCorrectCharacterEncodingHelper(formatter, content, content, mediaType, encoding, isDefaultEncoding);
        }

        [Theory]
        [PropertyData("ReadAndWriteCorrectCharacterEncoding")]
        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            MediaTypeFormatter formatter = new MockBufferedMediaTypeFormatter();
            string mediaType = string.Format("{0}; charset={1}", ExpectedSupportedMediaType, encoding);

            // Act & assert
            return WriteToStreamAsync_UsesCorrectCharacterEncodingHelper(formatter, content, content, mediaType, encoding, isDefaultEncoding);
        }
    }

    public class MockBufferedMediaTypeFormatter : BufferedMediaTypeFormatter
    {
        private const string SupportedMediaType = "text/test";        

        public MockBufferedMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(SupportedMediaType));

            // Set default supported character encodings
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override object ReadFromStream(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            object result = null;

            Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            using (StreamReader sReader = new StreamReader(stream, effectiveEncoding))
            {
                if (type == typeof(BufferedMediaTypeFormatterTests.SampleType))
                {
                    return new BufferedMediaTypeFormatterTests.SampleType { Number = 42 };
                }
                else
                {
                    result = sReader.ReadToEnd();
                }
            }
            return result;
        }

        public override void WriteToStream(Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
        {
            Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            using (StreamWriter sWriter = new StreamWriter(stream, effectiveEncoding))
            {
                if (value != null)
                {
                    sWriter.Write(value.ToString());
                }
                else
                {
                    sWriter.Write("null!");
                }
            }
        }
    }
}
