// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.Owin
{
    public class HttpMessageHandlerAdapterTest
    {
        [Fact]
        public void ConstructorWithOptions_IfOptionsIsNull_Throws()
        {
            // Arrange
            HttpMessageHandlerOptions options = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(options), "options");
        }

        [Fact]
        public void ConstructorWithOptions_IfMessageHandlerIsNull_Throws()
        {
            // Arrange
            HttpMessageHandlerOptions options = new HttpMessageHandlerOptions
            {
                MessageHandler = null,
                BufferPolicySelector = CreateDummyBufferPolicy()
            };

            // Act & Assert.
            Assert.ThrowsArgument(() => CreateProductUnderTest(options), "options",
                "HttpMessageHandlerOptions.MessageHandler must not be null.");
        }

        [Fact]
        public void ConstructorWithOptions_IfBufferPolicySelectorIsNull_Throws()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                HttpMessageHandlerOptions options = new HttpMessageHandlerOptions
                {
                    MessageHandler = messageHandler,
                    BufferPolicySelector = null
                };

                // Act & Assert.
                Assert.ThrowsArgument(() => CreateProductUnderTest(options), "options",
                    "HttpMessageHandlerOptions.BufferPolicySelector must not be null.");
            }
        }

        [Fact]
        public void ConstructorWithList_IfMessageHandlerIsNull_Throws()
        {
            // Arrange
            HttpMessageHandler messageHandler = null;
            IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();

            // Act & Assert.
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(messageHandler, bufferPolicySelector),
                "messageHandler");
        }

        [Fact]
        public void ConstructorWithList_IfBufferPolicySelectorIsNull_Throws()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = null;

                // Act & Assert.
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(messageHandler, bufferPolicySelector),
                    "bufferPolicySelector");
            }
        }

        [Fact]
        public void MessageHandler_IfUsingListConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler expectedMessageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();
                HttpMessageHandlerAdapter product = CreateProductUnderTest(expectedMessageHandler,
                    bufferPolicySelector);

                // Act
                HttpMessageHandler messageHandler = product.MessageHandler;

                // Assert
                Assert.Same(expectedMessageHandler, messageHandler);
            }
        }

        [Fact]
        public void MessageHandler_IfUsingOptionsConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler expectedMessageHandler = CreateDummyMessageHandler())
            {
                HttpMessageHandlerAdapter product = CreateProductUnderTest(new HttpMessageHandlerOptions
                {
                    MessageHandler = expectedMessageHandler,
                    BufferPolicySelector = CreateDummyBufferPolicy()
                });

                // Act
                HttpMessageHandler messageHandler = product.MessageHandler;

                // Assert
                Assert.Same(expectedMessageHandler, messageHandler);
            }
        }

        [Fact]
        public void BufferPolicySelector_IfUsingListConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector expectedBufferPolicySelector = CreateDummyBufferPolicy();
                HttpMessageHandlerAdapter product = CreateProductUnderTest(messageHandler,
                    expectedBufferPolicySelector);

                // Act
                IHostBufferPolicySelector bufferPolicySelector = product.BufferPolicySelector;

                // Assert
                Assert.Same(expectedBufferPolicySelector, bufferPolicySelector);
            }
        }

        [Fact]
        public void BufferPolicySelector_IfUsingOptionsConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector expectedBufferPolicySelector = CreateDummyBufferPolicy();
                HttpMessageHandlerAdapter product = CreateProductUnderTest(new HttpMessageHandlerOptions
                {
                    MessageHandler = messageHandler,
                    BufferPolicySelector = expectedBufferPolicySelector
                });

                // Act
                IHostBufferPolicySelector bufferPolicySelector = product.BufferPolicySelector;

                // Assert
                Assert.Same(expectedBufferPolicySelector, bufferPolicySelector);
            }
        }

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
            var adapter = CreateProductUnderTest(new HttpMessageHandlerOptions
            {
                MessageHandler = handler,
                BufferPolicySelector = bufferPolicySelector
            });

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
            string body = null;
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = async (r, i) =>
            {
                body = await r.Content.ReadAsStringAsync();
                return new HttpResponseMessage();
            };
            var handler = CreateLambdaMessageHandler(sendAsync);
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var expectedBody = "This is the request body.";
            environment["owin.RequestBody"] = new MemoryStream(Encoding.UTF8.GetBytes(expectedBody));
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            Assert.Equal(expectedBody, body);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Invoke_RespectsInputBufferingSetting(bool bufferInput)
        {
            var expectedBody = "This is the request body.";
            var requestBodyMock = new Mock<MemoryStream>(Encoding.UTF8.GetBytes(expectedBody));
            requestBodyMock.CallBase = true;
            requestBodyMock.Setup(s => s.CanSeek).Returns(false);
            MemoryStream requestBody = requestBodyMock.Object;

            string body = null;
            bool originalStreamDisposed = false;
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = async (r, i) =>
            {
                body = await r.Content.ReadAsStringAsync();
                originalStreamDisposed = !requestBody.CanRead;
                return new HttpResponseMessage();
            };
            var handler = CreateLambdaMessageHandler(sendAsync);
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: bufferInput, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            environment["owin.RequestBody"] = requestBody;
            var adapter = new HttpMessageHandlerAdapter(next: null, messageHandler: handler, bufferPolicySelector: bufferPolicySelector);

            adapter.Invoke(new OwinContext(environment)).Wait();

            // Assert that Web API gets the right body
            Assert.Equal(expectedBody, body);
            // The original stream should have been fully read and then disposed only when buffering.
            Assert.Equal(bufferInput, originalStreamDisposed);

            if (bufferInput)
            {
                // Assert that the OWIN environment still has a request body that can be read
                var owinRequestBody = environment["owin.RequestBody"] as Stream;
                byte[] bodyBytes = new byte[25];
                int charsRead = owinRequestBody.Read(bodyBytes, 0, 25);
                Assert.Equal(expectedBody, Encoding.UTF8.GetString(bodyBytes));
            }
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

        [Fact]
        public void Invoke_SetsOwinRequestContext()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpResponseMessage response = CreateResponse())
            {
                HttpRequestMessage request = null;
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, c) =>
                {
                    request = r;
                    return Task.FromResult(response);
                };

                using (HttpMessageHandler messageHandler = CreateLambdaMessageHandler(sendAsync))
                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(messageHandler,
                    bufferPolicySelector))
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();
                    IOwinResponse owinResponse = CreateFakeOwinResponse();
                    IOwinContext expectedContext = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    Task task = product.Invoke(expectedContext);

                    // Assert
                    Assert.NotNull(task);
                    task.WaitUntilCompleted();
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);

                    HttpRequestContext requestContext = request.GetRequestContext();
                    Assert.IsType<OwinHttpRequestContext>(requestContext);
                    OwinHttpRequestContext typedContext = (OwinHttpRequestContext)requestContext;
                    Assert.Same(expectedContext, typedContext.Context);
                    Assert.Same(request, typedContext.Request);
                }
            }
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

        [Fact]
        public void Invoke_IfBufferingFaults_SendsErrorResponse()
        {
            // Arrange
            Exception expectedException = CreateException();
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(messageHandler,
                bufferPolicySelector))
            using (MemoryStream output = new MemoryStream())
            {
                IOwinRequest owinRequest = CreateFakeOwinRequest();

                int statusCode = 0;
                Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
                mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                mock.SetupGet(r => r.Body).Returns(output);
                mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((v) => statusCode = v);
                IOwinResponse owinResponse = mock.Object;

                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                // Act
                Task task = product.Invoke(context);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);

                Assert.Equal(500, statusCode);
                using (HttpRequestMessage request = CreateRequest(includeErrorDetail: true))
                using (HttpResponseMessage expectedResponse = request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError, expectedException))
                {
                    string expectedContents = expectedResponse.Content.ReadAsStringAsync().Result;
                    Assert.Equal(expectedContents, Encoding.UTF8.GetString(output.ToArray()));
                }
            }
        }

        [Fact]
        public void Invoke_IfBufferingFaults_DisposesOriginalResponse()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (SpyDisposeFaultingHttpContent spy = new SpyDisposeFaultingHttpContent(CreateException()))
            using (HttpResponseMessage response = CreateResponse(spy))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(messageHandler,
                bufferPolicySelector))
            using (MemoryStream output = new MemoryStream())
            {
                IOwinRequest owinRequest = CreateFakeOwinRequest();

                int statusCode = 0;
                Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
                mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                mock.SetupGet(r => r.Body).Returns(output);
                mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((v) => statusCode = v);
                IOwinResponse owinResponse = mock.Object;

                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                // Act
                Task task = product.Invoke(context);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);

                Assert.True(spy.Disposed);
            }
        }

        [Fact]
        public void Invoke_IfBufferingErrorFaults_SendsEmptyErrorResponse()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpContent content = CreateFaultingContent(CreateException()))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.Formatters.Clear();
                configuration.Formatters.Add(CreateFaultingFormatter(CreateException()));

                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, i) =>
                {
                    r.SetConfiguration(configuration);
                    return Task.FromResult(response);
                };

                using (HttpMessageHandler messageHandler = CreateLambdaMessageHandler(sendAsync))
                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(messageHandler,
                    bufferPolicySelector))
                using (MemoryStream output = new MemoryStream())
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();

                    int statusCode = 0;
                    Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
                    mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                    mock.SetupGet(r => r.Body).Returns(output);
                    mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((v) => statusCode = v);
                    IOwinResponse owinResponse = mock.Object;

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act
                    Task task = product.Invoke(context);

                    // Assert
                    Assert.NotNull(task);
                    task.WaitUntilCompleted();
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);

                    Assert.Equal(500, statusCode);
                    Assert.Equal(new byte[0], output.ToArray());
                }
            }
        }

        [Fact]
        public void Invoke_IfStreamingFaults_DisposesResponseBodyAndReturnsCompletedTask()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: false);

            using (HttpContent content = CreateFaultingContent(CreateException()))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(messageHandler,
                    bufferPolicySelector))
                using (SpyDisposeStream spy = new SpyDisposeStream())
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();

                    IOwinResponse owinResponse = CreateFakeOwinResponse(spy);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    Task task = product.Invoke(context);

                    // Assert
                    Assert.NotNull(task);
                    task.WaitUntilCompleted();
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);

                    Assert.True(spy.Disposed);
                }
            }
        }

        private static IHostBufferPolicySelector CreateBufferPolicySelector(bool bufferInput, bool bufferOutput)
        {
            var mock = new Mock<IHostBufferPolicySelector>();
            mock.Setup(bps => bps.UseBufferedInputStream(It.IsAny<object>())).Returns(bufferInput);
            mock.Setup(bps => bps.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>())).Returns(bufferOutput);
            return mock.Object;
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static IHostBufferPolicySelector CreateDummyBufferPolicy()
        {
            return new Mock<IHostBufferPolicySelector>(MockBehavior.Strict).Object;
        }

        private static HttpMessageHandler CreateDummyMessageHandler()
        {
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            return mock.Object;
        }

        private static Exception CreateException()
        {
            return new Exception();
        }

        private static IOwinRequest CreateFakeOwinRequest()
        {
            Mock<IHeaderDictionary> headersMock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            headersMock.Setup(h => h.GetEnumerator()).Returns(
                new Mock<IEnumerator<KeyValuePair<string, string[]>>>().Object);

            Mock<IOwinRequest> mock = new Mock<IOwinRequest>(MockBehavior.Strict);
            mock.Setup(r => r.Method).Returns("GET");
            mock.Setup(r => r.Uri).Returns(new Uri("http://ignore"));
            mock.Setup(r => r.Body).Returns(Stream.Null);
            mock.Setup(r => r.Headers).Returns(headersMock.Object);
            mock.Setup(r => r.User).Returns((IPrincipal)null);
            mock.Setup(r => r.CallCancelled).Returns(CancellationToken.None);
            return mock.Object;
        }

        private static IOwinResponse CreateFakeOwinResponse()
        {
            Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
            mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
            return mock.Object;
        }

        private static IOwinResponse CreateFakeOwinResponse(Stream body)
        {
            Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
            mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
            mock.SetupGet(r => r.Body).Returns(body);
            return mock.Object;
        }

        private static Task CreateFaultedTask(Exception exception)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            source.SetException(exception);
            return source.Task;
        }

        private static FaultingHttpContent CreateFaultingContent(Exception exception)
        {
            return new FaultingHttpContent(exception);
        }

        private static MediaTypeFormatter CreateFaultingFormatter(Exception exception)
        {
            Mock<MediaTypeFormatter> mock = new Mock<MediaTypeFormatter>();
            mock.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            mock.Setup(f => f.GetPerRequestFormatterInstance(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                It.IsAny<MediaTypeHeaderValue>())).Returns(mock.Object);
            mock
                .Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(),
                    It.IsAny<HttpContent>(), It.IsAny<TransportContext>()))
                .Returns(CreateFaultedTask(exception));
            return mock.Object;
        }

        private static HttpMessageHandler CreateLambdaMessageHandler(
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            return new LambdaHttpMessageHandler(sendAsync);
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

        private static HttpMessageHandlerAdapter CreateProductUnderTest(HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
        {
            return new HttpMessageHandlerAdapter(next: null, messageHandler: messageHandler,
                bufferPolicySelector: bufferPolicySelector);
        }

        private static HttpMessageHandlerAdapter CreateProductUnderTest(HttpMessageHandlerOptions options)
        {
            return new HttpMessageHandlerAdapter(next: null, options: options);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestMessage CreateRequest(bool includeErrorDetail)
        {
            HttpRequestMessage request = CreateRequest();
            request.SetRequestContext(new HttpRequestContext
            {
                IncludeErrorDetail = includeErrorDetail
            });
            return request;
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static HttpResponseMessage CreateResponse(HttpContent content)
        {
            return new HttpResponseMessage
            {
                Content = content
            };
        }

        private static HttpMessageHandler CreateStubMessageHandler(HttpResponseMessage response)
        {
            return new LambdaHttpMessageHandler((r, c) => Task.FromResult(response));
        }

        private static IOwinContext CreateStubOwinContext(IOwinRequest request, IOwinResponse response)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.Setup(c => c.Request).Returns(request);
            mock.Setup(c => c.Response).Returns(response);
            return mock.Object;
        }

        private static IOwinContext CreateStubOwinContext(IOwinRequest request, IOwinResponse response, bool isLocal)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.Setup(c => c.Request).Returns(request);
            mock.Setup(c => c.Response).Returns(response);
            mock.Setup(c => c.Get<bool>("server.IsLocal")).Returns(isLocal);
            return mock.Object;
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
                    request.Properties[HttpPropertyKeys.NoRouteMatched] = true;
                }

                return TaskHelpers.FromResult<HttpResponseMessage>(Response);
            }
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }

        private class FaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public FaultingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        private class SpyDisposeStream : Stream
        {
            public bool Disposed { get; private set; }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class SpyDisposeFaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public SpyDisposeFaultingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            public bool Disposed { get; private set; }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
