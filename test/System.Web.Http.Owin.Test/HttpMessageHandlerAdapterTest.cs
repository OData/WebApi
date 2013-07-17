// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class HttpMessageHandlerAdapterTest
    {
        [Fact]
        public void Invoke_ThrowsOnNullRequest()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);
            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(context => context.Response).Returns(new OwinResponse());

            Assert.Throws<InvalidOperationException>(
                () => adapter.Invoke(mockContext.Object).Wait(),
                "The OWIN context's Request property must not be null.");
        }

        [Fact]
        public void Invoke_ThrowsOnNullResponse()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);
            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(context => context.Request).Returns(new OwinRequest());

            Assert.Throws<InvalidOperationException>(
                () => adapter.Invoke(mockContext.Object).Wait(),
                "The OWIN context's Response property must not be null.");
        }

        [Fact]
        public void Invoke_BuildsAppropriateRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public void Invoke_BuildsUriWithQueryStringIfPresent()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            environment["owin.RequestQueryString"] = "id=45";
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers?id=45", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public void Invoke_BuildsUriWithHostAndPort()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost:12345", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost:12345/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Theory]
        [InlineData("a b")]
        // reserved characters
        [InlineData("!*'();:@&=$,[]")]
        // common unreserved characters
        [InlineData(@"-_.~+""<>^`{|}")]
        // random unicode characters
        [InlineData("激光這")]
        [InlineData("%24")]
        [InlineData("?#")]
        public void Invoke_CreatesUri_ThatGeneratesCorrectlyDecodedStrings(string decodedId)
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers/" + decodedId);
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);
            var route = new HttpRoute("api/customers/{id}");

            adapter.Invoke(new OwinContext(environment)).Wait();
            IHttpRouteData routeData = route.GetRouteData("/vroot", handler.Request);

            Assert.NotNull(routeData);
            Assert.Equal(decodedId, routeData.Values["id"]);
        }

        [Fact]
        public void Invoke_AddsRequestHeadersToRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var requestHeaders = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            requestHeaders["Accept"] = new string[] { "application/json", "application/xml" };
            requestHeaders["Content-Length"] = new string[] { "45" };
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(2, request.Headers.Count());
            Assert.Equal(new string[] { "application/json", "application/xml" }, request.Headers.Accept.Select(mediaType => mediaType.ToString()).ToArray());
            Assert.Equal("localhost", request.Headers.Host);
            Assert.Single(request.Content.Headers);
            Assert.Equal(45, request.Content.Headers.ContentLength);
        }

        [Fact]
        public void Invoke_SetsRequestBodyOnRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var expectedBody = "This is the request body.";
            environment["owin.RequestBody"] = new MemoryStream(Encoding.UTF8.GetBytes(expectedBody));
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(expectedBody, request.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Invoke_RespectsInputBufferingSetting(bool bufferInput)
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: bufferInput, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var expectedBody = "This is the request body.";
            var requestBodyMock = new Mock<MemoryStream>(Encoding.UTF8.GetBytes(expectedBody));
            requestBodyMock.CallBase = true;
            requestBodyMock.Setup(s => s.CanSeek).Returns(false);
            MemoryStream requestBody = requestBodyMock.Object;
            environment["owin.RequestBody"] = requestBody;
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            if (bufferInput)
            {
                Assert.False(requestBody.CanRead);
                // Assert that the OWIN environment still has a request body that can be read
                var owinRequestBody = environment["owin.RequestBody"] as Stream;
                byte[] bodyBytes = new byte[25];
                int charsRead = owinRequestBody.Read(bodyBytes, 0, 25);
                Assert.Equal(expectedBody, Encoding.UTF8.GetString(bodyBytes));
            }
            else
            {
                Assert.True(requestBody.CanRead);
            }
            // Assert that Web API gets the right body
            var request = handler.Request;
            Assert.Equal(expectedBody, request.Content.ReadAsStringAsync().Result);

        }

        [Fact]
        public void Invoke_SetsOwinEnvironment()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Same(environment, request.GetOwinEnvironment());
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Invoke_SetsRequestIsLocalProperty(bool? isLocal, bool expectedRequestLocal)
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            if (isLocal.HasValue)
            {
                environment["server.IsLocal"] = isLocal.Value;
            }
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(expectedRequestLocal, request.IsLocal());
        }

        [Fact]
        public void Invoke_SetsClientCertificate()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var clientCert = new Mock<X509Certificate2>().Object;
            environment["ssl.ClientCertificate"] = clientCert;
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var request = handler.Request;
            Assert.Equal(clientCert, request.GetClientCertificate());
        }

        [Fact]
        public void Invoke_CallsMessageHandler_WithEnvironmentCancellationToken()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var cancellationToken = new CancellationToken();
            environment["owin.CallCancelled"] = cancellationToken;
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            Assert.Equal(cancellationToken, handler.CancellationToken);
        }

        [Fact]
        public void Invoke_CallsMessageHandler_WithEnvironmentUser()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var user = new Mock<IPrincipal>().Object;
            environment["server.User"] = user;
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            Assert.Equal(user, handler.User);
        }

        [Fact]
        public void Invoke_Throws_IfMessageHandlerReturnsNull()
        {
            HttpResponseMessage response = null;
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            Assert.Throws<InvalidOperationException>(
                () => adapter.Invoke(new OwinContext(environment)).Wait(),
                "The message handler did not return a response message.");
        }

        [Fact]
        public void Invoke_DoesNotCallNext_IfMessageHandlerDoesNotReturn404()
        {
            var mockNext = new Mock<OwinMiddleware>(MockBehavior.Strict, null);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(mockNext.Object, handler, bufferPolicySelector);

            Assert.DoesNotThrow(
                () => adapter.Invoke(new OwinContext(environment)).Wait());
        }

        [Fact]
        public void Invoke_DoesNotCallNext_IfMessageHandlerDoesNotAddNoRouteMatchedProperty()
        {
            var mockNext = new Mock<OwinMiddleware>(MockBehavior.Strict, null);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = false };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: mockNext.Object, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            Assert.DoesNotThrow(
                () => adapter.Invoke(new OwinContext(environment)).Wait());
        }

        [Fact]
        public void Invoke_CallsNext_IfMessageHandlerReturns404WithNoRouteMatched()
        {
            var nextMock = new Mock<OwinMiddleware>(null);
            nextMock.Setup(middleware => middleware.Invoke(It.IsAny<OwinContext>())).Returns(TaskHelpers.Completed()).Verifiable();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: nextMock.Object, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            nextMock.Verify();
        }

        [Fact]
        public void Invoke_SetsResponseStatusCodeAndReasonPhrase()
        {
            var expectedReasonPhrase = "OH NO!";
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = expectedReasonPhrase };
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            Assert.Equal(503, environment["owin.ResponseStatusCode"]);
            Assert.Equal(expectedReasonPhrase, environment["owin.ResponseReasonPhrase"]);
        }

        [Fact]
        public void Invoke_SetsResponseHeaders()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Location = new Uri("http://www.location.com/");
            response.Content = new StringContent(@"{""x"":""y""}", Encoding.UTF8, "application/json");
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.Equal(3, responseHeaders.Count);
            Assert.Equal("http://www.location.com/", Assert.Single(responseHeaders["Location"]));
            Assert.Equal("9", Assert.Single(responseHeaders["Content-Length"]));
            Assert.Equal("application/json; charset=utf-8", Assert.Single(responseHeaders["Content-Type"]));
        }

        [Fact]
        public void Invoke_SetsResponseBody()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expectedBody = @"{""x"":""y""}";
            response.Content = new StringContent(expectedBody, Encoding.UTF8, "application/json");
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var responseStream = new MemoryStream();
            environment["owin.ResponseBody"] = responseStream;
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            responseStream.Seek(0, SeekOrigin.Begin);
            byte[] bodyBytes = new byte[9];
            int charsRead = responseStream.Read(bodyBytes, 0, 9);
            // Assert that we can read 9 characters and no more characters after that
            Assert.Equal(9, charsRead);
            Assert.Equal(-1, responseStream.ReadByte());
            Assert.Equal(expectedBody, Encoding.UTF8.GetString(bodyBytes));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Invoke_RespectsOutputBufferingSetting(bool bufferOutput)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: bufferOutput);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            if (bufferOutput)
            {
                Assert.True(responseHeaders.ContainsKey("Content-Length"));
            }
            else
            {
                Assert.False(responseHeaders.ContainsKey("Content-Length"));
            }
        }

        [Fact]
        public void Invoke_AddsZeroContentLengthHeader_WhenThereIsNoContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.Equal("0", responseHeaders["Content-Length"][0]);
        }

        [Fact]
        public void Invoke_AddsTransferEncodingChunkedHeaderOverContentLength()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Hello world")));
            response.Headers.TransferEncodingChunked = true;
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.Equal("chunked", responseHeaders["Transfer-Encoding"][0]);
            Assert.False(responseHeaders.ContainsKey("Content-Length"));
        }

        [Fact]
        public void Invoke_AddsTransferEncodingChunkedHeaderIfThereIsNoContentLength()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.Equal("chunked", responseHeaders["Transfer-Encoding"][0]);
            Assert.False(responseHeaders.ContainsKey("Content-Length"));
        }

        private static HandlerStub CreateOKHandlerStub()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return new HandlerStub() { Response = response };
        }

        private static Dictionary<string, object> CreateOwinEnvironment(string method, string scheme, string hostHeaderValue, string pathBase, string path)
        {
            var environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            environment["owin.RequestMethod"] = method;
            environment["owin.RequestScheme"] = scheme;
            environment["owin.RequestHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { { "Host", new string[] { hostHeaderValue } } };
            environment["owin.RequestPathBase"] = pathBase;
            environment["owin.RequestPath"] = path;
            environment["owin.RequestQueryString"] = "";
            environment["owin.RequestBody"] = new MemoryStream();
            environment["owin.CallCancelled"] = new CancellationToken();
            environment["owin.ResponseHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment["owin.ResponseBody"] = new MemoryStream();
            return environment;
        }

        private static IHostBufferPolicySelector CreateBufferPolicySelector(bool bufferInput, bool bufferOutput)
        {
            var bufferPolicySelector = new Mock<IHostBufferPolicySelector>();
            bufferPolicySelector.Setup(bps => bps.UseBufferedInputStream(It.IsAny<object>())).Returns(bufferInput);
            bufferPolicySelector.Setup(bps => bps.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>())).Returns(bufferOutput);
            return bufferPolicySelector.Object;
        }

        public class HandlerStub : HttpMessageHandler
        {
            public HttpRequestMessage Request { get; private set; }
            public CancellationToken CancellationToken { get; private set; }
            public HttpResponseMessage Response { get; set; }
            public IPrincipal User { get; set; }
            public bool AddNoRouteMatchedKey { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                CancellationToken = cancellationToken;
                User = Thread.CurrentPrincipal;

                if (AddNoRouteMatchedKey)
                {
                    request.Properties["MS_NoRouteMatched"] = true;
                }

                return TaskHelpers.FromResult<HttpResponseMessage>(Response);
            }
        }
    }
}
