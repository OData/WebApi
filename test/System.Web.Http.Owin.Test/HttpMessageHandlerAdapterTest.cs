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
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class HttpMessageHandlerAdapterTest
    {
        [Fact]
        public void Invoke_BuildsAppropriateRequestMessage()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public void Invoke_BuildsUriWithQueryStringIfPresent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            environment["owin.RequestQueryString"] ="id=45";

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers?id=45", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public void Invoke_BuildsUriWithHostAndPort()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost:12345", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost:12345/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public void Invoke_Throws_IfHostHeaderMissing()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var requestHeaders = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            requestHeaders.Remove("Host");

            Assert.Throws<InvalidOperationException>(
                () => new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait(),
                "The OWIN environment does not contain a value for the required 'Host' header.");
        }

        [Fact]
        public void Invoke_Throws_IfHostHeaderHasNoValues()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var requestHeaders = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            requestHeaders["Host"] = new string[0];

            Assert.Throws<InvalidOperationException>(
                () => new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait(),
                "The OWIN environment does not contain a value for the required 'Host' header.");
        }

        [Fact]
        public void Invoke_AddsRequestHeadersToRequestMessage()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var requestHeaders = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            requestHeaders["Accept"] = new string[] { "application/json", "application/xml" };
            requestHeaders["Content-Length"] = new string[] { "45" };

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

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
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            environment["owin.RequestBody"] = new MemoryStream(Encoding.UTF8.GetBytes("This is the request body."));

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Equal("This is the request body.", request.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Invoke_RespectsInputBufferingSetting(bool bufferInput)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: bufferInput, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var requestBodyMock = new Mock<MemoryStream>(Encoding.UTF8.GetBytes("This is the request body."));
            requestBodyMock.CallBase = true;
            requestBodyMock.Setup(s => s.CanSeek).Returns(false);
            MemoryStream requestBody = requestBodyMock.Object;
            environment["owin.RequestBody"] = requestBody;

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            if (bufferInput)
            {
                Assert.False(requestBody.CanRead);
                // Assert that the OWIN environment still has a request body that can be read
                var owinRequestBody = environment["owin.RequestBody"] as Stream;
                byte[] bodyBytes = new byte[25];
                int charsRead = owinRequestBody.Read(bodyBytes, 0, 25);
                Assert.Equal("This is the request body.", Encoding.UTF8.GetString(bodyBytes));
            }
            else
            {
                Assert.True(requestBody.CanRead);
            }
            // Assert that Web API gets the right body
            var request = handler.Request;
            Assert.Equal("This is the request body.", request.Content.ReadAsStringAsync().Result);

        }

        [Fact]
        public void Invoke_SetsOwinEnvironment()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Same(environment, request.GetOwinEnvironment());
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Invoke_SetsRequestIsLocalProperty(bool? isLocal, bool expectedRequestLocal)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            if (isLocal.HasValue)
            {
                environment["server.IsLocal"] = isLocal.Value;
            }

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Equal(expectedRequestLocal, request.IsLocal());
        }

        [Fact]
        public void Invoke_SetsClientCertificate()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var clientCert = new Mock<X509Certificate2>().Object;
            environment["ssl.ClientCertificate"] = clientCert;

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            var request = handler.Request;
            Assert.Equal(clientCert, request.GetClientCertificate());
        }

        [Fact]
        public void Invoke_CallsMessageHandler_WithEnvironmentCancellationToken()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var cancellationToken = new CancellationToken();
            environment["owin.CallCancelled"] = cancellationToken;

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            Assert.Equal(cancellationToken, handler.CancellationToken);
        }

        [Fact]
        public void Invoke_CallsMessageHandler_WithEnvironmentUser()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var user = new Mock<IPrincipal>().Object;
            environment["server.User"] = user;

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            Assert.Equal(user, handler.User);
        }

        [Fact]
        public void Invoke_Throws_IfMessageHandlerReturnsNull()
        {
            HttpResponseMessage response = null;
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            Assert.Throws<InvalidOperationException>(
                () => new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait(),
                "The message handler did not return a response message.");
        }

        [Fact]
        public void Invoke_DoesNotCallNext_IfMessageHandlerDoesNotReturn404()
        {
            bool nextCalled = false;
            var next = new Func<IDictionary<string, object>, Task>(env =>
            {
                nextCalled = true;
                return TaskHelpers.Completed();
            });
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new MockHandler() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(next, handler, bufferPolicySelector).Invoke(environment).Wait();

            Assert.False(nextCalled);
        }

        [Fact]
        public void Invoke_DoesNotCallNext_IfMessageHandlerDoesNotAddNoRouteMatchedProperty()
        {
            bool nextCalled = false;
            var next = new Func<IDictionary<string, object>, Task>(env =>
            {
                nextCalled = true;
                return TaskHelpers.Completed();
            });
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new MockHandler() { Response = response, AddNoRouteMatchedKey = false };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(next, handler, bufferPolicySelector).Invoke(environment).Wait();

            Assert.False(nextCalled);
        }

        [Fact]
        public void Invoke_CallsNext_IfMessageHandlerReturns404WithNoRouteMatched()
        {
            bool nextCalled = false;
            var next = new Func<IDictionary<string, object>, Task>(env =>
                {
                    nextCalled = true;
                    return TaskHelpers.Completed();
                });
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new MockHandler() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(next, handler, bufferPolicySelector).Invoke(environment).Wait();

            Assert.True(nextCalled);
        }

        [Fact]
        public void Invoke_SetsResponseStatusCodeAndReasonPhrase()
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = "OH NO!" };
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, environment["owin.ResponseStatusCode"]);
            Assert.Equal("OH NO!", environment["owin.ResponseReasonPhrase"]);
        }

        [Fact]
        public void Invoke_SetsResponseHeaders()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Location = new Uri("http://www.location.com/");
            response.Content = new StringContent(@"{""x"":""y""}", Encoding.UTF8, "application/json");
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

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
            response.Content = new StringContent(@"{""x"":""y""}", Encoding.UTF8, "application/json");
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var responseStream = new MemoryStream();
            environment["owin.ResponseBody"] = responseStream;

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

            responseStream.Seek(0, SeekOrigin.Begin);
            byte[] bodyBytes = new byte[9];
            int charsRead = responseStream.Read(bodyBytes, 0, 9);
            // Assert that we can read 9 characters and no more characters after that
            Assert.Equal(9, charsRead);
            Assert.Equal(-1, responseStream.ReadByte());
            Assert.Equal(@"{""x"":""y""}", Encoding.UTF8.GetString(bodyBytes));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Invoke_RespectsOutputBufferingSetting(bool bufferOutput)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());
            var handler = new MockHandler() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: bufferOutput);
            var environment = CreateEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");

            new HttpMessageHandlerAdapter(env => TaskHelpers.Completed(), handler, bufferPolicySelector).Invoke(environment).Wait();

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

        private static Dictionary<string, object> CreateEnvironment(string method, string scheme, string hostHeaderValue, string pathBase, string path)
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
    }

    public class MockHandler : HttpMessageHandler
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
