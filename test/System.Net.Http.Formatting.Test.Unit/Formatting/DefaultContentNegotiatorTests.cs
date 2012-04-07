// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class DefaultContentNegotiatorTests
    {
        private readonly DefaultContentNegotiator _negotiator = new DefaultContentNegotiator();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(DefaultContentNegotiator), TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void Negotiate_WhenTypeParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _negotiator.Negotiate(null, _request, Enumerable.Empty<MediaTypeFormatter>()), "type");
        }

        [Fact]
        public void Negotiate_WhenRequestParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _negotiator.Negotiate(typeof(string), null, Enumerable.Empty<MediaTypeFormatter>()), "request");
        }

        [Fact]
        public void Negotiate_WhenFormattersParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _negotiator.Negotiate(typeof(string), _request, null), "formatters");
        }

        [Fact]
        public void Negotiate_ForEmptyFormatterCollection_ReturnsNull()
        {
            var result = _negotiator.Negotiate(typeof(string), _request, Enumerable.Empty<MediaTypeFormatter>());

            Assert.Null(result);
        }

        [Fact]
        public void MediaTypeMappingTakesPrecedenceOverAcceptHeader()
        {
            // Prepare the request message
            _request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            _request.Headers.Add("Browser", "IE");
            _request.Headers.Add("Cookie", "ABC");

            // Prepare the formatters
            List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
            formatters.Add(new JsonMediaTypeFormatter());
            formatters.Add(new XmlMediaTypeFormatter());
            PlainTextFormatter frmtr = new PlainTextFormatter();
            frmtr.SupportedMediaTypes.Clear();
            frmtr.MediaTypeMappings.Clear();
            frmtr.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
            frmtr.MediaTypeMappings.Add(new MyMediaTypeMapping(new MediaTypeHeaderValue(("application/xml"))));
            formatters.Add(frmtr);

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, formatters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/xml", result.MediaType.MediaType);
            Assert.IsType<PlainTextFormatter>(result.Formatter);
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

            _request.Content = new StringContent("test", Encoding.Default, mediaType.MediaType);

            var result = _negotiator.Negotiate(typeof(string), _request, collection);
            Assert.Same(formatter2, result.Formatter);
            Assert.MediaType.AreEqual(mediaType, result.MediaType, "Expected the formatter's media type to be returned.");
        }

        [Fact]
        public void Negotiate_SelectsJsonAsDefaultFormatter()
        {
            // Arrange
            _request.Content = new StringContent("test");

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, new MediaTypeFormatterCollection());

            // Assert
            Assert.IsType<JsonMediaTypeFormatter>(result.Formatter);
            Assert.Equal(MediaTypeConstants.ApplicationJsonMediaType.MediaType, result.MediaType.MediaType);
        }

        [Fact]
        public void Negotiate_SelectsXmlFormatter_ForXhrRequestThatAcceptsXml()
        {
            // Arrange
            _request.Content = new StringContent("test");
            _request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            _request.Headers.Add("x-requested-with", "XMLHttpRequest");

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, new MediaTypeFormatterCollection());

            // Assert
            Assert.Equal("application/xml", result.MediaType.MediaType);
            Assert.IsType<XmlMediaTypeFormatter>(result.Formatter);
        }

        [Fact]
        public void Negotiate_SelectsJsonFormatter_ForXhrRequestThatDoesNotSpecifyAcceptHeaders()
        {
            // Arrange
            _request.Content = new StringContent("test");
            _request.Headers.Add("x-requested-with", "XMLHttpRequest");

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, new MediaTypeFormatterCollection());

            // Assert
            Assert.Equal("application/json", result.MediaType.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(result.Formatter);
        }

        [Fact]
        public void Negotiate_RespectsFormatterOrdering_ForXhrRequestThatDoesNotSpecifyAcceptHeaders()
        {
            // Arrange
            _request.Content = new StringContent("test");
            _request.Headers.Add("x-requested-with", "XMLHttpRequest");

            MediaTypeFormatterCollection formatters = new MediaTypeFormatterCollection(new MediaTypeFormatter[]
            {
                new XmlMediaTypeFormatter(),
                new JsonMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter()
            });

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, formatters);

            // Assert
            Assert.Equal("application/json", result.MediaType.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(result.Formatter);
        }

        [Fact]
        public void Negotiate_SelectsJsonFormatter_ForXHRAndJsonValueResponse()
        {
            // Arrange
            _request.Content = new StringContent("test");
            _request.Headers.Add("x-requested-with", "XMLHttpRequest");

            // Act
            var result = _negotiator.Negotiate(typeof(JToken), _request, new MediaTypeFormatterCollection());

            Assert.Equal("application/json", result.MediaType.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(result.Formatter);
        }

        [Fact]
        public void Negotiate_SelectsJsonFormatter_ForXHRAndMatchAllAcceptHeader()
        {
            // Accept
            _request.Content = new StringContent("test");
            _request.Headers.Add("x-requested-with", "XMLHttpRequest");
            _request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, new MediaTypeFormatterCollection());

            // Assert
            Assert.Equal("application/json", result.MediaType.MediaType);
            Assert.IsType<JsonMediaTypeFormatter>(result.Formatter);
        }

        [Fact]
        public void Negotiate_UsesRequestedFormatterForXHRAndMatchAllPlusOtherAcceptHeader()
        {
            // Arrange
            _request.Content = new StringContent("test");
            _request.Headers.Add("x-requested-with", "XMLHttpRequest");
            _request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"); // XHR header sent by Firefox 3b5

            // Act
            var result = _negotiator.Negotiate(typeof(string), _request, new MediaTypeFormatterCollection());

            // Assert
            Assert.Equal("application/xml", result.MediaType.MediaType);
            Assert.IsType<XmlMediaTypeFormatter>(result.Formatter);
        }

        private class PlainTextFormatter : MediaTypeFormatter
        {
            public override bool CanReadType(Type type)
            {
                return true;
            }

            public override bool CanWriteType(Type type)
            {
                return true;
            }
        }

        private class MyMediaTypeMapping : MediaTypeMapping
        {
            public MyMediaTypeMapping(MediaTypeHeaderValue mediaType)
                : base(mediaType)
            {
            }

            public override double TryMatchMediaType(HttpRequestMessage request)
            {
                if (request.Headers.Contains("Cookie"))
                {
                    return 1.0;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
