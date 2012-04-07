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
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class MediaTypeFormatterTracerTest
    {
        [Fact]
        public void OnReadFromStreamAsync_Traces()
        {
            // Arrange
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).
                Returns(TaskHelpers.FromResult<object>("sampleValue"));
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
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
        public void OnReadFromStreamAsync_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
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
        public void OnReadFromStreamAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(exception);

            mockFormatter.Setup(
                f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>())).
                Returns(tcs.Task);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
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
        public void OnWriteToStreamAsync_Traces()
        {
            // Arrange
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<TransportContext>())).
                Returns(TaskHelpers.Completed());
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
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
        public void OnWriteToStreamAsync_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f =>
                f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(),
                                     It.IsAny<HttpContentHeaders>(), It.IsAny<TransportContext>())).
                Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
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
        public void OnWriteToStreamAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(exception);

            mockFormatter.Setup(
                f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<TransportContext>())).
                Returns(tcs.Task);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
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

        [Fact]
        public void GetPerRequestFormatterInstance_Returns_Tracing_MediaTypeFormatter()
        {
            // Arrange
            Mock<MediaTypeFormatter> mockReturnFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f =>
                f.GetPerRequestFormatterInstance(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                                                 It.IsAny<MediaTypeHeaderValue>())).Returns(mockReturnFormatter.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);

            // Act
            MediaTypeFormatter actualFormatter = tracer.GetPerRequestFormatterInstance(typeof(string), request, new MediaTypeHeaderValue("application/json"));

            // Assert
            Assert.IsAssignableFrom<IFormatterTracer>(actualFormatter);
        }

        [Theory]
        [InlineDataAttribute(typeof(XmlMediaTypeFormatter))]
        [InlineDataAttribute(typeof(JsonMediaTypeFormatter))]
        [InlineDataAttribute(typeof(FormUrlEncodedMediaTypeFormatter))]
        public void CreateTracer_Returns_Tracing_Formatter(Type formatterType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatter = (MediaTypeFormatter)Activator.CreateInstance(formatterType);

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            Assert.IsAssignableFrom<IFormatterTracer>(tracingFormatter);
            Assert.IsAssignableFrom(formatterType, tracingFormatter);
        }

        [Fact]
        public void CreateTracer_Returns_Tracing_BufferedFormatter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true }.Object;

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            Assert.IsAssignableFrom<IFormatterTracer>(tracingFormatter);
            Assert.IsAssignableFrom<BufferedMediaTypeFormatter>(tracingFormatter);
        }

        [Theory]
        [InlineDataAttribute(typeof(XmlMediaTypeFormatter))]
        [InlineDataAttribute(typeof(JsonMediaTypeFormatter))]
        [InlineDataAttribute(typeof(FormUrlEncodedMediaTypeFormatter))]
        public void CreateTracer_Loads_SupportedEncodings_From_InnerFormatter(Type formatterType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatter = (MediaTypeFormatter)Activator.CreateInstance(formatterType);
            Mock<Encoding> encoding = new Mock<Encoding>();
            formatter.SupportedEncodings.Clear();
            formatter.SupportedEncodings.Add(encoding.Object);

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            Assert.Equal(formatter.SupportedEncodings, tracingFormatter.SupportedEncodings);
        }

        [Theory]
        [InlineDataAttribute(typeof(XmlMediaTypeFormatter))]
        [InlineDataAttribute(typeof(JsonMediaTypeFormatter))]
        [InlineDataAttribute(typeof(FormUrlEncodedMediaTypeFormatter))]
        public void CreateTracer_Loads_SupportedMediaTypes_From_InnerFormatter(Type formatterType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatter = (MediaTypeFormatter)Activator.CreateInstance(formatterType);

            MediaTypeHeaderValue mediaTypeHeaderValue = new MediaTypeHeaderValue("application/dummy");
            formatter.SupportedMediaTypes.Clear();
            formatter.SupportedMediaTypes.Add(mediaTypeHeaderValue);

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            Assert.Equal(formatter.SupportedMediaTypes, tracingFormatter.SupportedMediaTypes);
        }

        [Theory]
        [InlineDataAttribute(typeof(XmlMediaTypeFormatter))]
        [InlineDataAttribute(typeof(JsonMediaTypeFormatter))]
        [InlineDataAttribute(typeof(FormUrlEncodedMediaTypeFormatter))]
        public void CreateTracer_Loads_MediaTypeMappings_From_InnerFormatter(Type formatterType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatter = (MediaTypeFormatter)Activator.CreateInstance(formatterType);

            Mock<MediaTypeMapping> mediaTypeMapping = new Mock<MediaTypeMapping>(new MediaTypeHeaderValue("application/dummy"));
            formatter.MediaTypeMappings.Clear();
            formatter.MediaTypeMappings.Add(mediaTypeMapping.Object);

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            Assert.Equal(formatter.MediaTypeMappings, tracingFormatter.MediaTypeMappings);
        }

        [Theory]
        [InlineDataAttribute(typeof(XmlMediaTypeFormatter))]
        [InlineDataAttribute(typeof(JsonMediaTypeFormatter))]
        [InlineDataAttribute(typeof(FormUrlEncodedMediaTypeFormatter))]
        public void CreateTracer_Loads_RequiredMemberSelector_From_InnerFormatter(Type formatterType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatter = (MediaTypeFormatter)Activator.CreateInstance(formatterType);

            Mock<IRequiredMemberSelector> requiredMemberSelector = new Mock<IRequiredMemberSelector>();
            formatter.RequiredMemberSelector = requiredMemberSelector.Object;

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            Assert.Equal(formatter.RequiredMemberSelector, tracingFormatter.RequiredMemberSelector);
        }
    }
}
