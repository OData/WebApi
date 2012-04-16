// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class BufferedMediaTypeFormatterTracerTest
    {
        [Fact]
        public void CanReadType_Calls_Inner()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>();
            mockFormatter.Setup(f => f.CanReadType(randomType)).Returns(true).Verifiable();
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, new TestTraceWriter(), request);

            // Act
            bool valueReturned = tracer.CanReadType(randomType);

            // Assert
            Assert.True(valueReturned);
            mockFormatter.Verify();
        }

        [Fact]
        public void CanWriteType_Calls_Inner()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>();
            mockFormatter.Setup(f => f.CanWriteType(randomType)).Returns(true).Verifiable();
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, new TestTraceWriter(), request);

            // Act
            bool valueReturned = tracer.CanWriteType(randomType);

            // Assert
            Assert.True(valueReturned);
            mockFormatter.Verify();
        }

        [Fact]
        public void GetPerRequestFormatterInstance_Calls_Inner_And_Wraps_Tracer_Around_It()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("plain/text");
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>();
            BufferedMediaTypeFormatter formatterObject = mockFormatter.Object;

            mockFormatter.Setup(f => f.GetPerRequestFormatterInstance(randomType, request, mediaType)).Returns(formatterObject).Verifiable();
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(formatterObject, new TestTraceWriter(), request);

            // Act
            MediaTypeFormatter valueReturned = tracer.GetPerRequestFormatterInstance(randomType, request, mediaType);

            // Assert
            BufferedMediaTypeFormatterTracer tracerReturned = Assert.IsType<BufferedMediaTypeFormatterTracer>(valueReturned);
            Assert.Same(formatterObject, tracerReturned.InnerFormatter as BufferedMediaTypeFormatter);
            mockFormatter.Verify();
        }

        [Fact]
        public void SetDefaultContentHeaders_Calls_Inner()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            HttpContentHeaders contentHeaders = new StringContent("").Headers;
            string mediaType = "plain/text";
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>();
            BufferedMediaTypeFormatter formatterObject = mockFormatter.Object;

            mockFormatter.Setup(f => f.SetDefaultContentHeaders(randomType, contentHeaders, mediaType)).Verifiable();
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(formatterObject, new TestTraceWriter(), request);

            // Act
            tracer.SetDefaultContentHeaders(randomType, contentHeaders, mediaType);

            // Assert
            mockFormatter.Verify();
        }

        [Fact]
        public void SupportedMediaTypes_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/fake");
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            BufferedMediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.SupportedMediaTypes.Clear();
            innerFormatter.SupportedMediaTypes.Add(mediaType);
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.SupportedMediaTypes, tracer.SupportedMediaTypes);
        }

        [Fact]
        public void MediaTypeMappings_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/fake");
            Mock<MediaTypeMapping> mockMapping = new Mock<MediaTypeMapping>(mediaType);
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            BufferedMediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.MediaTypeMappings.Clear();
            innerFormatter.MediaTypeMappings.Add(mockMapping.Object);
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MediaTypeMappings, tracer.MediaTypeMappings);
        }

        [Fact]
        public void SupportedEncodings_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<Encoding> mockEncoding = new Mock<Encoding>();
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            BufferedMediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.SupportedEncodings.Clear();
            innerFormatter.SupportedEncodings.Add(mockEncoding.Object);
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.SupportedEncodings, tracer.SupportedEncodings);
        }

        [Fact]
        public void RequiredMemberSelector_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IRequiredMemberSelector> mockSelector = new Mock<IRequiredMemberSelector>();
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            BufferedMediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.RequiredMemberSelector = mockSelector.Object;
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.RequiredMemberSelector, tracer.RequiredMemberSelector);
        }

        [Fact]
        public void BufferSize_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            BufferedMediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.BufferSize = innerFormatter.BufferSize + 1;
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.BufferSize, tracer.BufferSize);
        }

        [Fact]
        public void ReadFromStream_Traces()
        {
            // Arrange
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStream(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).
                Returns("sampleValue");
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "ReadFromStream" }
            };

            // Act
            string valueReturned = tracer.ReadFromStream(typeof(string), new MemoryStream(), request.Content.Headers, null) as string;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal("sampleValue", valueReturned);
        }

        [Fact]
        public void ReadFromStream_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStream(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "ReadFromStream" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.ReadFromStream(typeof(string), new MemoryStream(), request.Content.Headers, null));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void WriteToStream_Traces()
        {
            // Arrange
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.WriteToStream(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>()));
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "WriteToStream" }
            };

            // Act
            tracer.WriteToStream(typeof(string), "sampleValue", new MemoryStream(), request.Content.Headers);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void WriteToStream_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f =>
                f.WriteToStream(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(),
                                     It.IsAny<HttpContentHeaders>())).
                Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "WriteToStream" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.WriteToStream(typeof(string), "sampleValue", new MemoryStream(), request.Content.Headers));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }
    }
}
