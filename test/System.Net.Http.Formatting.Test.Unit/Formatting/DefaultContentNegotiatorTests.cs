using System.Linq;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class DefaultContentNegotiatorTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(DefaultContentNegotiator), TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void Negotiate_WhenTypeParameterIsNull_ThrowsException()
        {
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeHeaderValue mediaType;

            Assert.ThrowsArgumentNull(() => selector.Negotiate(null, request, Enumerable.Empty<MediaTypeFormatter>(), out mediaType), "type");
        }

        [Fact]
        public void Negotiate_WhenRequestParameterIsNull_ThrowsException()
        {
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaType;

            Assert.ThrowsArgumentNull(() => selector.Negotiate(typeof(string), null, Enumerable.Empty<MediaTypeFormatter>(), out mediaType), "request");
        }

        [Fact]
        public void Negotiate_WhenFormattersParameterIsNull_ThrowsException()
        {
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeHeaderValue mediaType;

            Assert.ThrowsArgumentNull(() => selector.Negotiate(typeof(string), request, null, out mediaType), "formatters");
        }

        [Fact]
        public void Negotiate_ForEmptyFormatterCollection_ReturnsNull()
        {
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeHeaderValue mediaType;

            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, Enumerable.Empty<MediaTypeFormatter>(), out mediaType);

            Assert.Null(formatter);
            Assert.Null(mediaType);
        }

        [Fact]
        public void Negotiate_ForRequestReturnsFirstMatchingFormatter()
        {
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/myMediaType");

            MediaTypeFormatter formatter1 = new MockMediaTypeFormatter()
            {
                CanWriteTypeCallback = (Type t) => false
            };

            MediaTypeFormatter formatter2 = new MockMediaTypeFormatter()
            {
                CanWriteTypeCallback = (Type t) => true
            };

            formatter2.SupportedMediaTypes.Add(mediaType);

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(
                new MediaTypeFormatter[] 
                {
                    formatter1,
                    formatter2
                });

            HttpContent content = new StringContent("test");
            content.Headers.ContentType = mediaType;
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = content
            };

            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;
            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, collection, out mediaTypeReturned);
            Assert.Same(formatter2, formatter);
            Assert.MediaType.AreEqual(mediaType, mediaTypeReturned, "Expected the formatter's media type to be returned.");
        }

        [Fact]
        public void Negotiate_SelectsJsonAsDefaultFormatter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new StringContent("test")
            };
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;

            // Act
            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, new MediaTypeFormatterCollection(), out mediaTypeReturned);

            // Assert
            Assert.IsType<JsonMediaTypeFormatter>(formatter);
            Assert.Equal(mediaTypeReturned.MediaType, MediaTypeConstants.ApplicationJsonMediaType.MediaType);
        }

        [Fact]
        public void Negotiate_SelectsXmlFormatter_ForXhrRequestThatAcceptsXml()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new StringContent("test")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;

            // Act
            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, new MediaTypeFormatterCollection(), out mediaTypeReturned);

            // Assert
            Assert.Equal("application/xml", mediaTypeReturned.MediaType);
            Assert.IsType<XmlMediaTypeFormatter>(formatter);
        }

        [Fact]
        public void Negotiate_SelectsJsonFormatter_ForXhrRequestThatDoesNotSpecifyAcceptHeaders()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new StringContent("test")
            };
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;

            // Act
            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, new MediaTypeFormatterCollection(), out mediaTypeReturned);

            // Assert
            Assert.Equal("application/json", mediaTypeReturned.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(formatter);
        }

        [Fact]
        public void Negotiate_SelectsJsonFormatter_ForXHRAndJsonValueResponse()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new StringContent("test")
            };
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;

            // Act
            MediaTypeFormatter formatter = selector.Negotiate(typeof(JToken), request, new MediaTypeFormatterCollection(), out mediaTypeReturned);

            Assert.Equal("application/json", mediaTypeReturned.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(formatter);
        }

        [Fact]
        public void Negotiate_SelectsJsonFormatter_ForXHRAndMatchAllAcceptHeader()
        {
            // Accept
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new StringContent("test")
            };
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;

            // Act
            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, new MediaTypeFormatterCollection(), out mediaTypeReturned);

            // Assert
            Assert.Equal("application/json", mediaTypeReturned.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(formatter);
        }

        [Fact]
        public void Negotiate_UsesRequestedFormatterForXHRAndMatchAllPlusOtherAcceptHeader()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new StringContent("test")
            };
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"); // XHR header sent by Firefox 3b5
            DefaultContentNegotiator selector = new DefaultContentNegotiator();
            MediaTypeHeaderValue mediaTypeReturned = null;

            // Act
            MediaTypeFormatter formatter = selector.Negotiate(typeof(string), request, new MediaTypeFormatterCollection(), out mediaTypeReturned);

            // Assert
            Assert.Equal("application/xml", mediaTypeReturned.MediaType);
            Assert.IsType<XmlMediaTypeFormatter>(formatter);
        }
    }
}
