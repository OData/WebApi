// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using System.Web.Http.Owin.Properties;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace System.Web.Http.Owin
{
    /// <summary>
    /// Represents an OWIN component that submits requests to an <see cref="HttpMessageHandler"/> when invoked.
    /// </summary>
    public class HttpMessageHandlerAdapter : IDisposable
    {
        private AppFunc _next;
        private HttpMessageInvoker _messageInvoker;
        private IHostBufferPolicySelector _bufferPolicySelector;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageHandlerAdapter" /> class.
        /// </summary>
        /// <param name="next">The next component in the pipeline.</param>
        /// <param name="messageHandler">The <see cref="HttpMessageHandler" /> to submit requests to.</param>
        /// <param name="bufferPolicySelector">The <see cref="IHostBufferPolicySelector"/> that determines whether or not to buffer requests and responses.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "In accordance with OWIN design")]
        public HttpMessageHandlerAdapter(AppFunc next, HttpMessageHandler messageHandler, IHostBufferPolicySelector bufferPolicySelector)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            if (bufferPolicySelector == null)
            {
                throw new ArgumentNullException("bufferPolicySelector");
            }

            _next = next;
            _messageInvoker = new HttpMessageInvoker(messageHandler);
            _bufferPolicySelector = bufferPolicySelector;
        }

        /// <summary>
        /// Invokes the component within the OWIN pipeline.
        /// </summary>
        /// <param name="environment">The OWIN environment for the request.</param>
        /// <returns>A <see cref="Task"/> that will complete when the request is processed.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "See comment below")]
        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            Stream requestBody = environment.GetOwinValue<Stream>(OwinConstants.RequestBodyKey);
            HttpRequestMessage request = CreateRequestMessage(environment, requestBody);
            if (!requestBody.CanSeek && _bufferPolicySelector.UseBufferedInputStream(hostContext: environment))
            {
                await BufferRequestBodyAsync(environment, request.Content);
            }
            CancellationToken cancellationToken = environment.GetOwinValue<CancellationToken>(OwinConstants.CallCancelledKey);

            SetPrincipal(environment);

            HttpResponseMessage response = null;
            bool callNext = false;
            try
            {
                response = await _messageInvoker.SendAsync(request, cancellationToken);

                // Handle null responses
                if (response == null)
                {
                    throw Error.InvalidOperation(OwinResources.SendAsync_ReturnedNull);
                }

                // Handle soft 404s where no route matched - call the next component
                if (IsSoftNotFound(request, response))
                {
                    callNext = true;
                }
                else
                {
                    if (response.Content != null && _bufferPolicySelector.UseBufferedOutputStream(response))
                    {
                        response = await BufferResponseBodyAsync(request, response);
                    }

                    FixUpContentLengthHeaders(response);
                    await SendResponseMessageAsync(environment, response);
                }
            }
            finally
            {
                // Note that the HttpRequestMessage is explicitly NOT disposed.  Disposing it would close the input stream
                // and prevent cascaded components from accessing it.  The server MUST handle any necessary cleanup upon
                // request completion.
                request.DisposeRequestResources();
                if (response != null)
                {
                    response.Dispose();
                }
            }

            // Call the next component if no route matched
            if (callNext)
            {
                await _next.Invoke(environment);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not out of scope")]
        private static HttpRequestMessage CreateRequestMessage(IDictionary<string, object> environment, Stream requestBody)
        {
            string requestMethod = environment.GetOwinValue<string>(OwinConstants.RequestMethodKey);
            IDictionary<string, string[]> requestHeaders = environment.GetOwinValue<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey);
            string requestPathBase;
            Uri requestUri = CreateRequestUri(environment, requestHeaders, out requestPathBase);

            // Create the request
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(requestMethod), requestUri);

            // Set the body
            HttpContent content = new StreamContent(requestBody);
            request.Content = content;

            // Copy the headers
            foreach (KeyValuePair<string, string[]> header in requestHeaders)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    bool success = request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    Contract.Assert(success, "Every header can be added either to the request headers or to the content headers");
                }
            }

            // Map the OWIN environment keys to the request properties keys that Web API expects
            MapRequestProperties(request, environment, requestPathBase);

            return request;
        }

        // Implements the algorithm for reconstructing a URI according to section 5.4 of the OWIN specification
        private static Uri CreateRequestUri(IDictionary<string, object> environment, IDictionary<string, string[]> requestHeaders, out string requestPathBase)
        {
            StringBuilder uriBuilder = new StringBuilder();

            // Append request scheme
            string requestScheme = environment.GetOwinValue<string>(OwinConstants.RequestSchemeKey);
            uriBuilder.Append(requestScheme);

            uriBuilder.Append("://");

            // Append host and port
            string[] hostHeaderValues;
            if (requestHeaders.TryGetValue("Host", out hostHeaderValues) && hostHeaderValues.Length > 0)
            {
                uriBuilder.Append(hostHeaderValues[0]);
            }
            else
            {
                throw Error.InvalidOperation(OwinResources.CreateRequestURI_MissingHostHeader);
            }

            // Append request path
            requestPathBase = environment.GetOwinValue<string>(OwinConstants.RequestPathBaseKey);
            uriBuilder.Append(requestPathBase);
            string requestPath = environment.GetOwinValue<string>(OwinConstants.RequestPathKey);
            uriBuilder.Append(requestPath);

            // Append query string
            string requestQueryString = environment.GetOwinValue<string>(OwinConstants.RequestQueryStringKey);
            if (requestQueryString.Length > 0)
            {
                uriBuilder.Append('?');
                uriBuilder.Append(requestQueryString);
            }

            return new Uri(uriBuilder.ToString(), UriKind.Absolute);
        }

        private static void MapRequestProperties(HttpRequestMessage request, IDictionary<string, object> environment, string requestPathBase)
        {
            // Set the environment on the request
            request.SetOwinEnvironment(environment);

            // Set the virtual path root for link resolution and link generation to work
            // OWIN spec requires request path base to be either the empty string or start with "/"
            request.SetVirtualPathRoot(requestPathBase.Length == 0 ? "/" : requestPathBase);

            // Set a delegate to get the client certificate
            request.Properties[HttpPropertyKeys.RetrieveClientCertificateDelegateKey] = new Func<HttpRequestMessage, X509Certificate2>(
                req =>
                {
                    X509Certificate2 clientCertificate;
                    return environment.TryGetValue<X509Certificate2>(OwinConstants.ClientCertifiateKey, out clientCertificate) ? clientCertificate : null;
                });

            // Set a lazily-evaluated way of determining whether the request is local or not
            Lazy<bool> isLocal = new Lazy<bool>(() =>
                {
                    bool local;
                    if (environment.TryGetValue(OwinConstants.IsLocalKey, out local))
                    {
                        return local;
                    }
                    return false;
                }, isThreadSafe: false);
            request.Properties[HttpPropertyKeys.IsLocalKey] = isLocal;
        }

        private static async Task BufferRequestBodyAsync(IDictionary<string, object> environment, HttpContent content)
        {
            await content.LoadIntoBufferAsync();
            // We need to replace the request body with a buffered stream so that other
            // components can read the stream
            environment[OwinConstants.RequestBodyKey] = await content.ReadAsStreamAsync();
        }

        private static void SetPrincipal(IDictionary<string, object> environment)
        {
            // Set the principal
            IPrincipal user;
            if (environment.TryGetValue<IPrincipal>(OwinConstants.UserKey, out user))
            {
                Thread.CurrentPrincipal = user;
            }
        }

        private static bool IsSoftNotFound(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                bool routingFailure;
                if (request.Properties.TryGetValue<bool>(HttpPropertyKeys.NoRouteMatched, out routingFailure) && routingFailure)
                {
                    return true;
                }
            }
            return false;
        }

        private static async Task<HttpResponseMessage> BufferResponseBodyAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            Exception exception = null;
            try
            {
                await response.Content.LoadIntoBufferAsync();
            }
            catch (Exception e)
            {
                exception = e;
            }

            // If the content can't be buffered, create a buffered error response for the exception
            // This code will commonly run when a formatter throws during the process of serialization
            if (exception != null)
            {
                response.Dispose();
                response = request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
                await response.Content.LoadIntoBufferAsync();
            }
            return response;
        }

        // Responsible for setting Content-Length and Transfer-Encoding if needed
        private static void FixUpContentLengthHeaders(HttpResponseMessage response)
        {
            HttpContent responseContent = response.Content;
            if (responseContent != null)
            {
                if (response.Headers.TransferEncodingChunked == true)
                {
                    // According to section 4.4 of the HTTP 1.1 spec, HTTP responses that use chunked transfer
                    // encoding must not have a content length set. Chunked should take precedence over content
                    // length in this case because chunked is always set explicitly by users while the Content-Length
                    // header can be added implicitly by System.Net.Http.
                    responseContent.Headers.ContentLength = null;
                }
                else
                {
                    // Triggers delayed content-length calculations.
                    if (responseContent.Headers.ContentLength == null)
                    {
                        // If there is no content-length we can compute, then the response should use
                        // chunked transfer encoding to prevent the server from buffering the content
                        response.Headers.TransferEncodingChunked = true;
                    }
                }
            }
        }

        private static Task SendResponseMessageAsync(IDictionary<string, object> environment, HttpResponseMessage response)
        {
            environment[OwinConstants.ResponseStatusCodeKey] = response.StatusCode;
            environment[OwinConstants.ResponseReasonPhraseKey] = response.ReasonPhrase;

            // Copy non-content headers
            IDictionary<string, string[]> responseHeaders = environment.GetOwinValue<IDictionary<string, string[]>>(OwinConstants.ResponseHeadersKey);
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
            {
                responseHeaders[header.Key] = header.Value.AsArray();
            }

            HttpContent responseContent = response.Content;
            if (responseContent == null)
            {
                // Set the content-length to 0 to prevent the server from sending back the response chunked
                responseHeaders["Content-Length"] = new string[] { "0" };
                return TaskHelpers.Completed();
            }
            else
            {
                // Copy content headers
                foreach (KeyValuePair<string, IEnumerable<string>> contentHeader in responseContent.Headers)
                {
                    responseHeaders[contentHeader.Key] = contentHeader.Value.AsArray();
                }

                // Copy body
                Stream responseBody = environment.GetOwinValue<Stream>(OwinConstants.ResponseBodyKey);
                return responseContent.CopyToAsync(responseBody);
            }
        }

        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                _messageInvoker.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
