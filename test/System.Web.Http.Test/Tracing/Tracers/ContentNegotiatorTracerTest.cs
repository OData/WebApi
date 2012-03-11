using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class ContentNegotiatorTracerTest
    {
        [Fact]
        public void Negotiate_Calls_Inner_Negotiate()
        {
            // Arrange
            MediaTypeHeaderValue innerSelectedMediaType = null;
            MediaTypeHeaderValue outerSelectedMediaType = null;
            bool negotiateCalled = false;
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out innerSelectedMediaType)).Callback(
                                () => {negotiateCalled = true;});

            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, new TestTraceWriter());

            // Act
            ((IContentNegotiator) tracer).Negotiate(typeof (int), request, new MediaTypeFormatter[0], out outerSelectedMediaType); 

            // Assert
            Assert.True(negotiateCalled);
        }

        [Fact]
        public void Negotiate_Returns_Inner_MediaType()
        {
            // Arrange
            MediaTypeHeaderValue expectedMediaType = new MediaTypeHeaderValue("application/xml");
            MediaTypeHeaderValue innerSelectedMediaType = expectedMediaType;
            MediaTypeHeaderValue outerSelectedMediaType = null;
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out innerSelectedMediaType));

            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, new TestTraceWriter());

            // Act
            ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out outerSelectedMediaType);

            // Assert
            Assert.Same(expectedMediaType, outerSelectedMediaType);
        }

        [Fact]
        public void Negotiate_Returns_Wrapped_Inner_XmlFormatter()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Returns(
                                expectedFormatter);
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, new TestTraceWriter());

            // Act
            var actualFormatter = ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType);

            // Assert
            Assert.IsType<XmlMediaTypeFormatterTracer>(actualFormatter);
        }

        [Fact]
        public void Negotiate_Returns_Wrapped_Inner_JsonFormatter()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            MediaTypeFormatter expectedFormatter = new JsonMediaTypeFormatter();
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Returns(
                                expectedFormatter);
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, new TestTraceWriter());

            // Act
            var actualFormatter = ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType);

            // Assert
            Assert.IsType<JsonMediaTypeFormatterTracer>(actualFormatter);
        }

        [Fact]
        public void Negotiate_Returns_Wrapped_Inner_FormUrlEncodedFormatter()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            MediaTypeFormatter expectedFormatter = new FormUrlEncodedMediaTypeFormatter();
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Returns(
                                expectedFormatter);
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, new TestTraceWriter());

            // Act
            var actualFormatter = ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType);

            // Assert
            Assert.IsType<FormUrlEncodedMediaTypeFormatterTracer>(actualFormatter);
        }

        [Fact]
        public void Negotiate_Returns_Null_Inner_Formatter()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Returns(
                                (MediaTypeFormatter) null);
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, new TestTraceWriter());

            // Act
            var actualFormatter = ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType);

            // Assert
            Assert.Null(actualFormatter);
        }

        [Fact]
        public void Negotiate_Traces_BeginEnd()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Returns(
                                expectedFormatter);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Negotiate_Throws_When_Inner_Throws()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException expectedException = new InvalidOperationException("test");
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Throws(expectedException);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, traceWriter);

            // Act & Assert
            InvalidOperationException actualException = Assert.Throws<InvalidOperationException>(() => ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType));

            // Assert
            Assert.Same(expectedException, actualException);
        }

        [Fact]
        public void Negotiate_Traces_BeginEnd_When_Inner_Throws()
        {
            // Arrange
            MediaTypeHeaderValue mediaType = null;
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException expectedException = new InvalidOperationException("test");
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>();
            mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Throws(expectedException);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(mockNegotiator.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ((IContentNegotiator)tracer).Negotiate(typeof(int), request, new MediaTypeFormatter[0], out mediaType));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(expectedException, traceWriter.Traces[1].Exception);
        }
    }
}
