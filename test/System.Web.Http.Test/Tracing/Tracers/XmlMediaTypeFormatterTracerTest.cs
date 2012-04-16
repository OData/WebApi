// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class XmlMediaTypeFormatterTracerTest
    {
        [Fact]
        public void CanReadType_Calls_Inner()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>();
            mockFormatter.Setup(f => f.CanReadType(randomType)).Returns(true).Verifiable();
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, new TestTraceWriter(), request);

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
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>();
            mockFormatter.Setup(f => f.CanWriteType(randomType)).Returns(true).Verifiable();
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, new TestTraceWriter(), request);

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
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>();
            XmlMediaTypeFormatter formatterObject = mockFormatter.Object;

            mockFormatter.Setup(f => f.GetPerRequestFormatterInstance(randomType, request, mediaType)).Returns(formatterObject).Verifiable();
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(formatterObject, new TestTraceWriter(), request);

            // Act
            MediaTypeFormatter valueReturned = tracer.GetPerRequestFormatterInstance(randomType, request, mediaType);

            // Assert
            XmlMediaTypeFormatterTracer tracerReturned = Assert.IsType<XmlMediaTypeFormatterTracer>(valueReturned);
            Assert.Same(formatterObject, tracerReturned.InnerFormatter as XmlMediaTypeFormatter);
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
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>();
            XmlMediaTypeFormatter formatterObject = mockFormatter.Object;

            mockFormatter.Setup(f => f.SetDefaultContentHeaders(randomType, contentHeaders, mediaType)).Verifiable();
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(formatterObject, new TestTraceWriter(), request);

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
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.SupportedMediaTypes.Clear();
            innerFormatter.SupportedMediaTypes.Add(mediaType);
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

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
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.MediaTypeMappings.Clear();
            innerFormatter.MediaTypeMappings.Add(mockMapping.Object);
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MediaTypeMappings, tracer.MediaTypeMappings);
        }

        [Fact]
        public void SupportedEncodings_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<Encoding> mockEncoding = new Mock<Encoding>();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.SupportedEncodings.Clear();
            innerFormatter.SupportedEncodings.Add(mockEncoding.Object);
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.SupportedEncodings, tracer.SupportedEncodings);
        }

        [Fact]
        public void RequiredMemberSelector_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IRequiredMemberSelector> mockSelector = new Mock<IRequiredMemberSelector>();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.RequiredMemberSelector = mockSelector.Object;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.RequiredMemberSelector, tracer.RequiredMemberSelector);
        }

        [Fact]
        public void UseXmlSerializer_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.UseXmlSerializer = !innerFormatter.UseXmlSerializer;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.UseXmlSerializer, tracer.UseXmlSerializer);
        }

        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void Indent_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.Indent = !innerFormatter.Indent;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.Indent, tracer.Indent);
        }

        [Fact]
        public void ReadFromStreamAsync_Traces()
        {
            // Arrange
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).
                Returns(TaskHelpers.FromResult<object>("sampleValue"));
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "ReadFromStreamAsync" }
            };

            // Act
            Task<object> task = tracer.ReadFromStreamAsync(typeof(string), new MemoryStream(), request.Content.Headers, null);
            string result = task.Result as string;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal("sampleValue", result);
        }

        [Fact]
        public void ReadFromStreamAsync_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "ReadFromStreamAsync" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.ReadFromStreamAsync(typeof(string), new MemoryStream(), request.Content.Headers, null));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void ReadFromStreamAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(exception);

            mockFormatter.Setup(
                f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).
                Returns(tcs.Task);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "ReadFromStreamAsync" }
            };

            // Act
            Task<object> task = tracer.ReadFromStreamAsync(typeof(string), new MemoryStream(), request.Content.Headers, null);

            // Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => task.Wait());
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void WriteToStreamAsync_Traces()
        {
            // Arrange
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<TransportContext>())).
                Returns(TaskHelpers.Completed());
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "WriteToStreamAsync" }
            };

            // Act
            Task task = tracer.WriteToStreamAsync(typeof(string), "sampleValue", new MemoryStream(), request.Content.Headers, transportContext: null);
            task.Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void WriteToStreamAsync_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f =>
                f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(),
                                     It.IsAny<HttpContentHeaders>(), It.IsAny<TransportContext>())).
                Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "WriteToStreamAsync" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.WriteToStreamAsync(typeof(string), "sampleValue", new MemoryStream(), request.Content.Headers, transportContext: null));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void WriteToStreamAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<XmlMediaTypeFormatter> mockFormatter = new Mock<XmlMediaTypeFormatter>() { CallBase = true };
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(exception);

            mockFormatter.Setup(
                f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<TransportContext>())).
                Returns(tcs.Task);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "WriteToStreamAsync" }
            };

            // Act
            Task task = tracer.WriteToStreamAsync(typeof(string), "sampleValue", new MemoryStream(), request.Content.Headers, transportContext: null);

            // Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => task.Wait());
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

    }
}
