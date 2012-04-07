// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Mocks;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class HttpClientExtensionsTest
    {
        private readonly MediaTypeFormatter _formatter = new MockMediaTypeFormatter { CallBase = true };
        private readonly HttpClient _client;

        public HttpClientExtensionsTest()
        {
            Mock<TestableHttpMessageHandler> handlerMock = new Mock<TestableHttpMessageHandler> { CallBase = true };
            handlerMock
                .Setup(h => h.SendAsyncPublic(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns((HttpRequestMessage request, CancellationToken _) => TaskHelpers.FromResult(new HttpResponseMessage() { RequestMessage = request }));

            _client = new HttpClient(handlerMock.Object);
        }

        [Fact]
        public void PostAsJsonAsync_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsJsonAsync("http://www.example.com", new object()), "client");
        }

        [Fact]
        public void PostAsJsonAsync_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsJsonAsync(null, new object()),
                "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        [Fact]
        public void PostAsJsonAsync_UsesJsonMediaTypeFormatter()
        {
            var result = _client.PostAsJsonAsync("http://example.com", new object());

            var response = result.Result;
            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<JsonMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PostAsXmlAsync_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsXmlAsync("http://www.example.com", new object()), "client");
        }

        [Fact]
        public void PostAsXmlAsync_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsXmlAsync(null, new object()),
                "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        [Fact]
        public void PostAsXmlAsync_UsesXmlMediaTypeFormatter()
        {
            var result = _client.PostAsXmlAsync("http://example.com", new object());

            var response = result.Result;
            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<XmlMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PostAsync_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsync("http://www.example.com", new object(), new JsonMediaTypeFormatter(), "text/json"), "client");
        }

        [Fact]
        public void PostAsync_WhenRequestUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsync(null, new object(), new JsonMediaTypeFormatter(), "text/json"),
                "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        [Fact]
        public void PostAsync_WhenRequestUriIsSet_CreatesRequestWithAppropriateUri()
        {
            _client.BaseAddress = new Uri("http://example.com/");

            var result = _client.PostAsync("myapi/", new object(), _formatter);

            var request = result.Result.RequestMessage;
            Assert.Equal("http://example.com/myapi/", request.RequestUri.ToString());
        }

        [Fact]
        public void PostAsync_WhenAuthoritativeMediaTypeIsSet_CreatesRequestWithAppropriateContentType()
        {
            var result = _client.PostAsync("http://example.com/myapi/", new object(), _formatter, "foo/bar; charset=utf-16");

            var request = result.Result.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-16", request.Content.Headers.ContentType.CharSet);
        }

        [Fact]
        public void PostAsync_WhenFormatterIsSet_CreatesRequestWithObjectContentAndCorrectFormatter()
        {
            var result = _client.PostAsync("http://example.com/myapi/", new object(), _formatter, "foo/bar; charset=utf-16");

            var request = result.Result.RequestMessage;
            var content = Assert.IsType<ObjectContent<object>>(request.Content);
            Assert.Same(_formatter, content.Formatter);
        }

        [Fact]
        public void PostAsync_IssuesPostRequest()
        {
            var result = _client.PostAsync("http://example.com/myapi/", new object(), _formatter);

            var request = result.Result.RequestMessage;
            Assert.Same(HttpMethod.Post, request.Method);
        }

        [Fact]
        public void PostAsync_WhenMediaTypeFormatterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _client.PostAsync("http;//example.com", new object(), formatter: null), "formatter");
        }

        [Fact]
        public void PutAsJsonAsync_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsJsonAsync("http://www.example.com", new object()), "client");
        }

        [Fact]
        public void PutAsJsonAsync_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsJsonAsync(null, new object()),
                "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        [Fact]
        public void PutAsJsonAsync_UsesJsonMediaTypeFormatter()
        {
            var result = _client.PutAsJsonAsync("http://example.com", new object());

            var response = result.Result;
            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<JsonMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PutAsXmlAsync_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsXmlAsync("http://www.example.com", new object()), "client");
        }

        [Fact]
        public void PutAsXmlAsync_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsXmlAsync(null, new object()),
                "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        [Fact]
        public void PutAsXmlAsync_UsesXmlMediaTypeFormatter()
        {
            var result = _client.PutAsXmlAsync("http://example.com", new object());

            var response = result.Result;
            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<XmlMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PutAsync_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsync("http://www.example.com", new object(), new JsonMediaTypeFormatter(), "text/json"), "client");
        }

        [Fact]
        public void PutAsync_WhenRequestUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsync(null, new object(), new JsonMediaTypeFormatter(), "text/json"),
                "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        [Fact]
        public void PutAsync_WhenRequestUriIsSet_CreatesRequestWithAppropriateUri()
        {
            _client.BaseAddress = new Uri("http://example.com/");

            var result = _client.PutAsync("myapi/", new object(), _formatter);

            var request = result.Result.RequestMessage;
            Assert.Equal("http://example.com/myapi/", request.RequestUri.ToString());
        }

        [Fact]
        public void PutAsync_WhenAuthoritativeMediaTypeIsSet_CreatesRequestWithAppropriateContentType()
        {
            var result = _client.PutAsync("http://example.com/myapi/", new object(), _formatter, "foo/bar; charset=utf-16");

            var request = result.Result.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-16", request.Content.Headers.ContentType.CharSet);
        }

        [Fact]
        public void PutAsync_WhenFormatterIsSet_CreatesRequestWithObjectContentAndCorrectFormatter()
        {
            var result = _client.PutAsync("http://example.com/myapi/", new object(), _formatter, "foo/bar; charset=utf-16");

            var request = result.Result.RequestMessage;
            var content = Assert.IsType<ObjectContent<object>>(request.Content);
            Assert.Same(_formatter, content.Formatter);
        }

        [Fact]
        public void PutAsync_IssuesPutRequest()
        {
            var result = _client.PutAsync("http://example.com/myapi/", new object(), _formatter);

            var request = result.Result.RequestMessage;
            Assert.Same(HttpMethod.Put, request.Method);
        }

        [Fact]
        public void PutAsync_WhenMediaTypeFormatterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _client.PutAsync("http;//example.com", new object(), formatter: null), "formatter");
        }
    }
}
