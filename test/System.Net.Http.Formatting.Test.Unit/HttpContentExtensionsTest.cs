using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class HttpContentExtensionsTest
    {
        private static readonly IEnumerable<MediaTypeFormatter> _emptyFormatterList = Enumerable.Empty<MediaTypeFormatter>();
        private readonly Mock<MediaTypeFormatter> _formatterMock = new Mock<MediaTypeFormatter>();
        private readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue("foo/bar");

        [Fact]
        public void ReadAsAsync_WhenContentParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync(null, typeof(string), _emptyFormatterList), "content");
        }

        [Fact]
        public void ReadAsAsync_WhenTypeParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync(new StringContent(""), null, _emptyFormatterList), "type");
        }

        [Fact]
        public void ReadAsAsync_WhenFormattersParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync(new StringContent(""), typeof(string), null), "formatters");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenContentParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync<string>(null, _emptyFormatterList), "content");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenFormattersParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync<string>(new StringContent(""), null), "formatters");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenNoMatchingFormatterFound_Throws()
        {
            var content = new StringContent("{}");
            content.Headers.ContentType = _mediaType;
            content.Headers.ContentType.CharSet = "utf-16";
            var formatters = new MediaTypeFormatter[] { new JsonMediaTypeFormatter() };

            Assert.Throws<InvalidOperationException>(() => content.ReadAsAsync<List<string>>(formatters),
                "No MediaTypeFormatter is available to read an object of type 'List`1' from content with media type 'foo/bar'.");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenNoMatchingFormatterFoundForContentWithNoMediaType_Throws()
        {
            var content = new StringContent("{}");
            content.Headers.ContentType = null;
            var formatters = new MediaTypeFormatter[] { new JsonMediaTypeFormatter() };

            Assert.Throws<InvalidOperationException>(() => content.ReadAsAsync<List<string>>(formatters),
                "No MediaTypeFormatter is available to read an object of type 'List`1' from content with media type ''undefined''.");
        }

        [Fact]
        public void ReadAsAsyncOfT_ReadsFromContent_ThenInvokesFormattersReadFromStreamMethod()
        {
            Stream contentStream = null;
            string value = "42";
            var contentMock = new Mock<TestableHttpContent> { CallBase = true };
            contentMock.Setup(c => c.SerializeToStreamAsyncPublic(It.IsAny<Stream>(), It.IsAny<TransportContext>()))
                .Returns(TaskHelpers.Completed)
                .Callback((Stream s, TransportContext _) => contentStream = s)
                .Verifiable();
            HttpContent content = contentMock.Object;
            content.Headers.ContentType = _mediaType;
            _formatterMock
                .Setup(f => f.ReadFromStreamAsync(typeof(string), It.IsAny<Stream>(), It.IsAny<HttpContentHeaders>(), It.IsAny<IFormatterLogger>()))
                .Returns(TaskHelpers.FromResult<object>(value));
            _formatterMock.Setup(f => f.CanReadType(typeof(string))).Returns(true);
            _formatterMock.Object.SupportedMediaTypes.Add(_mediaType);
            var formatters = new[] { _formatterMock.Object };

            var result = content.ReadAsAsync<string>(formatters);

            var resultValue = result.Result;
            Assert.Same(value, resultValue);
            contentMock.Verify();
            _formatterMock.Verify(f => f.ReadFromStreamAsync(typeof(string), contentStream, content.Headers, null), Times.Once());
        }

        public abstract class TestableHttpContent : HttpContent
        {
            protected override Task<Stream> CreateContentReadStreamAsync()
            {
                return CreateContentReadStreamAsyncPublic();
            }

            public virtual Task<Stream> CreateContentReadStreamAsyncPublic()
            {
                return base.CreateContentReadStreamAsync();
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return SerializeToStreamAsyncPublic(stream, context);
            }

            public abstract Task SerializeToStreamAsyncPublic(Stream stream, TransportContext context);

            protected override bool TryComputeLength(out long length)
            {
                return TryComputeLengthPublic(out length);
            }

            public abstract bool TryComputeLengthPublic(out long length);
        }
    }
}
