// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// A test class for common <see cref="MediaTypeFormatter"/> functionality across multiple implementations.
    /// </summary>
    /// <typeparam name="TFormatter">The type of formatter under test.</typeparam>
    public abstract class MediaTypeFormatterTestBase<TFormatter> where TFormatter : MediaTypeFormatter, new()
    {
        protected MediaTypeFormatterTestBase()
        {
        }

        public abstract IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes { get; }

        public abstract IEnumerable<Encoding> ExpectedSupportedEncodings { get; }

        /// <summary>
        /// Byte representation of an <see cref="SampleType"/> with value 42 using the default encoding
        /// for this media type formatter.
        /// </summary>
        public abstract byte[] ExpectedSampleTypeByteRepresentation { get; }

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<TFormatter, MediaTypeFormatter>(TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void SupportEncoding_DefaultSupportedMediaTypes()
        {
            TFormatter formatter = new TFormatter();
            Assert.True(ExpectedSupportedMediaTypes.SequenceEqual(formatter.SupportedMediaTypes));
        }

        [Fact]
        public void SupportEncoding_DefaultSupportedEncodings()
        {
            TFormatter formatter = new TFormatter();
            Assert.True(ExpectedSupportedEncodings.SequenceEqual(formatter.SupportedEncodings));
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsOnNull()
        {
            TFormatter formatter = new TFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(null, Stream.Null, null, null); }, "type");
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(typeof(object), null, null, null, null); }, "stream");
        }

        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotReadStream()
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            contentHeaders.ContentLength = 0;

            // Act 
            return formatter.ReadFromStreamAsync(typeof(object), mockStream.Object, contentHeaders, mockFormatterLogger).
                ContinueWith(
                    readTask =>
                    {
                        // Assert
                        Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                        mockStream.Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
                        mockStream.Verify(s => s.ReadByte(), Times.Never());
                        mockStream.Verify(s => s.BeginRead(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                    });
        }

        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            contentHeaders.ContentLength = 0;

            // Act 
            return formatter.ReadFromStreamAsync(typeof(object), mockStream.Object, contentHeaders, mockFormatterLogger).
                ContinueWith(
                    readTask =>
                    {
                        // Assert
                        Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                        mockStream.Verify(s => s.Close(), Times.Never());
                    });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(0)]
        [InlineData("")]
        public void ReadFromStreamAsync_WhenContentLengthIsZero_ReturnsDefaultTypeValue<T>(T value)
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            HttpContent content = new StringContent("");

            // Act
            var result = formatter.ReadFromStreamAsync(typeof(T), content.ReadAsStreamAsync().Result,
                content.Headers, null);
            result.WaitUntilCompleted();

            // Assert
            Assert.Equal(default(T), (T)result.Result);
        }

        [Fact]
        public Task ReadFromStreamAsync_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            contentHeaders.ContentLength = memStream.Length;

            // Act
            return formatter.ReadFromStreamAsync(typeof(SampleType), memStream, contentHeaders, null).ContinueWith(
                readTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.True(memStream.CanRead);

                    var value = Assert.IsType<SampleType>(readTask.Result);
                    Assert.Equal(42, value.Number);
                });
        }

        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsNull_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            contentHeaders.ContentLength = null;

            // Act
            return formatter.ReadFromStreamAsync(typeof(SampleType), memStream, contentHeaders, null).ContinueWith(
                readTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.True(memStream.CanRead);

                    var value = Assert.IsType<SampleType>(readTask.Result);
                    Assert.Equal(42, value.Number);
                });
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsOnNull()
        {
            TFormatter formatter = new TFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(null, new object(), Stream.Null, null, null); }, "type");
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(typeof(object), new object(), null, null, null); }, "stream");
        }

        [Fact]
        public Task WriteToStreamAsync_WhenObjectIsNull_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.CanWrite).Returns(true);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

            // Act 
            return formatter.WriteToStreamAsync(typeof(object), null, mockStream.Object, contentHeaders, null).ContinueWith(
                writeTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    mockStream.Verify(s => s.Close(), Times.Never());
                    mockStream.Verify(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                });
        }

        [Fact]
        public Task WriteToStreamAsync_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = new TFormatter();
            SampleType sampleType = new SampleType { Number = 42 };
            MemoryStream memStream = new MemoryStream();
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            return formatter.WriteToStreamAsync(typeof(SampleType), sampleType, memStream, contentHeaders, null).ContinueWith(
                writeTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    Assert.True(memStream.CanRead);

                    byte[] actualSampleTypeByteRepresentation = memStream.ToArray();
                    Assert.Equal(ExpectedSampleTypeByteRepresentation, actualSampleTypeByteRepresentation);
                });
        }

        public abstract Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding);

        public abstract Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding);

        public Task ReadFromStreamAsync_UsesCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            byte[] data = enc.GetBytes(formattedContent);
            MemoryStream memStream = new MemoryStream(data);

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = data.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            return formatter.ReadFromStreamAsync(typeof(string), memStream, headers, mockFormatterLogger).ContinueWith(
                (readTask) =>
                {
                    string result = readTask.Result as string;

                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.Equal(content, result);
                });
        }

        public Task WriteToStreamAsync_UsesCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            byte[] preamble = enc.GetPreamble();
            byte[] data = enc.GetBytes(formattedContent);
            byte[] expectedData = new byte[preamble.Length + data.Length];
            Buffer.BlockCopy(preamble, 0, expectedData, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, expectedData, preamble.Length, data.Length);

            MemoryStream memStream = new MemoryStream();

            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            contentHeaders.Clear();
            contentHeaders.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            contentHeaders.ContentLength = expectedData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            return formatter.WriteToStreamAsync(typeof(string), content, memStream, contentHeaders, null).ContinueWith(
                (writeTask) =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    byte[] actualData = memStream.ToArray();
                    Assert.Equal(expectedData, actualData);
                });
        }

        public static Task ReadContentUsingCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            byte[] data = enc.GetBytes(formattedContent);
            MemoryStream memStream = new MemoryStream(data);

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = data.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            return formatter.ReadFromStreamAsync(typeof(string), memStream, headers, mockFormatterLogger).ContinueWith(
                (readTask) =>
                {
                    string result = readTask.Result as string;

                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.Equal(content, result);
                });
        }

        public static Task WriteContentUsingCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            byte[] preamble = enc.GetPreamble();
            byte[] data = enc.GetBytes(formattedContent);
            byte[] expectedData = new byte[preamble.Length + data.Length];
            Buffer.BlockCopy(preamble, 0, expectedData, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, expectedData, preamble.Length, data.Length);

            MemoryStream memStream = new MemoryStream();

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = expectedData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            return formatter.WriteToStreamAsync(typeof(string), content, memStream, headers, null).ContinueWith(
                (writeTask) =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    byte[] actualData = memStream.ToArray();

                    Assert.Equal(expectedData, actualData);
                });
        }

        [DataContract(Name = "DataContractSampleType")]
        public class SampleType
        {
            [DataMember]
            public int Number { get; set; }
        }
    }
}
