// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    // Abstract base test class for MediaTypeFormatter tracers.
    // This tests core functionality common to all MediaTypeFormatters.
    // It does not test read/write paths because BufferedMediaTypeFormatter
    // has sealed those methods, and they cannot be overridden by mocks.
    // Refer to ReadWriteMediaTypeFormatterTracerTestBase for read/write tests.
    internal abstract class MediaTypeFormatterTracerTestBase<TFormatter, TTracer> 
        where TFormatter: MediaTypeFormatter
        where TTracer : MediaTypeFormatter
    {
        public abstract TTracer CreateTracer(TFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter);

        [Fact]
        public void CanReadType_Calls_Inner()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };

            mockFormatter.Setup(f => f.CanReadType(randomType)).Returns(true).Verifiable();
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

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
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            mockFormatter.Setup(f => f.CanWriteType(randomType)).Returns(true).Verifiable();
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

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
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            MediaTypeFormatter formatterObject = mockFormatter.Object;

            mockFormatter.Setup(f => f.GetPerRequestFormatterInstance(randomType, request, mediaType)).Returns(formatterObject).Verifiable();
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

            // Act
            MediaTypeFormatter valueReturned = tracer.GetPerRequestFormatterInstance(randomType, request, mediaType);

            // Assert
            IFormatterTracer tracerReturned = Assert.IsAssignableFrom<IFormatterTracer>(valueReturned);
            Assert.Same(formatterObject, tracerReturned.InnerFormatter);
            mockFormatter.Verify();
        }

        [Fact]
        public void SetDefaultContentHeaders_Calls_Inner()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Type randomType = typeof(string);
            HttpContentHeaders contentHeaders = new StringContent("").Headers;
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("plain/text");
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            MediaTypeFormatter formatterObject = mockFormatter.Object;

            mockFormatter.Setup(f => f.SetDefaultContentHeaders(randomType, contentHeaders, mediaType)).Verifiable();
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

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
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            MediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.SupportedMediaTypes.Clear();
            innerFormatter.SupportedMediaTypes.Add(mediaType);
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

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
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            MediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.MediaTypeMappings.Clear();
            innerFormatter.MediaTypeMappings.Add(mockMapping.Object);
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

            // Act & Assert
            Assert.Equal(innerFormatter.MediaTypeMappings, tracer.MediaTypeMappings);
        }

        [Fact]
        public void SupportedEncodings_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<Encoding> mockEncoding = new Mock<Encoding>();
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            MediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.SupportedEncodings.Clear();
            innerFormatter.SupportedEncodings.Add(mockEncoding.Object);
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

            // Act & Assert
            Assert.Equal(innerFormatter.SupportedEncodings, tracer.SupportedEncodings);
        }

        [Fact]
        public void RequiredMemberSelector_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<IRequiredMemberSelector> mockSelector = new Mock<IRequiredMemberSelector>();
            Mock<TFormatter> mockFormatter = new Mock<TFormatter>() { CallBase = true };
            MediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.RequiredMemberSelector = mockSelector.Object;
            MediaTypeFormatter tracer = CreateTracer(mockFormatter.Object, request, new TestTraceWriter());

            // Act & Assert
            Assert.Equal(innerFormatter.RequiredMemberSelector, tracer.RequiredMemberSelector);
        }
    }
}
