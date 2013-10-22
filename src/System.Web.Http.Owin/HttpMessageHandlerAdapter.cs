// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Owin.Properties;
using Microsoft.Owin;

namespace System.Web.Http.Owin
{
    /// <summary>
    /// Represents an OWIN component that submits requests to an <see cref="HttpMessageHandler"/> when invoked.
    /// </summary>
    public class HttpMessageHandlerAdapter : OwinMiddleware, IDisposable
    {
        private readonly HttpMessageHandler _messageHandler;
        private readonly HttpMessageInvoker _messageInvoker;
        private readonly IHostBufferPolicySelector _bufferPolicySelector;

        private bool _disposed;

        /// <summary>Initializes a new instance of the <see cref="HttpMessageHandlerAdapter"/> class.</summary>
        /// <param name="next">The next component in the pipeline.</param>
        /// <param name="options">The options to configure this adapter.</param>
        public HttpMessageHandlerAdapter(OwinMiddleware next, HttpMessageHandlerOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _messageHandler = options.MessageHandler;

            if (_messageHandler == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(HttpMessageHandlerOptions).Name, "MessageHandler"), "options");
            }

            _messageInvoker = new HttpMessageInvoker(_messageHandler);
            _bufferPolicySelector = options.BufferPolicySelector;

            if (_bufferPolicySelector == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(HttpMessageHandlerOptions).Name, "BufferPolicySelector"), "options");
            }
        }

        /// <summary>Initializes a new instance of the <see cref="HttpMessageHandlerAdapter"/> class.</summary>
        /// <param name="next">The next component in the pipeline.</param>
        /// <param name="messageHandler">The <see cref="HttpMessageHandler"/> to submit requests to.</param>
        /// <param name="bufferPolicySelector">
        /// The <see cref="IHostBufferPolicySelector"/> that determines whether or not to buffer requests and
        /// responses.
        /// </param>
        public HttpMessageHandlerAdapter(OwinMiddleware next, HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
            : this(next, CreateOptions(messageHandler, bufferPolicySelector))
        {
        }

        /// <summary>Gets the <see cref="HttpMessageHandler"/> to submit requests to.</summary>
        public HttpMessageHandler MessageHandler
        {
            get { return _messageHandler; }
        }

        /// <summary>
        /// Gets the <see cref="IHostBufferPolicySelector"/> that determines whether or not to buffer requests and
        /// responses.
        /// </summary>
        public IHostBufferPolicySelector BufferPolicySelector
        {
            get { return _bufferPolicySelector; }
        }

        /// <inheritdoc />
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinRequest owinRequest = context.Request;
            IOwinResponse owinResponse = context.Response;

            if (owinRequest == null)
            {
                throw Error.InvalidOperation(OwinResources.OwinContext_NullRequest);
            }
            if (owinResponse == null)
            {
                throw Error.InvalidOperation(OwinResources.OwinContext_NullResponse);
            }

            return InvokeCore(context, owinRequest, owinResponse);
        }

        private async Task InvokeCore(IOwinContext context, IOwinRequest owinRequest,
            IOwinResponse owinResponse)
        {
            HttpContent requestContent;

            if (!owinRequest.Body.CanSeek && _bufferPolicySelector.UseBufferedInputStream(hostContext: context))
            {
                requestContent = await CreateBufferedRequestContentAsync(owinRequest);
            }
            else
            {
                requestContent = CreateStreamedRequestContent(owinRequest);
            }

            HttpRequestMessage request = CreateRequestMessage(owinRequest, requestContent);
            MapRequestProperties(request, context);

            SetPrincipal(owinRequest.User);

            HttpResponseMessage response = null;
            bool callNext;

            try
            {
                response = await _messageInvoker.SendAsync(request, owinRequest.CallCancelled);

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
                    callNext = false;

                    if (response.Content != null && _bufferPolicySelector.UseBufferedOutputStream(response))
                    {
                        response = await BufferResponseBodyAsync(request, response);
                    }

                    FixUpContentLengthHeaders(response);
                    await SendResponseMessageAsync(response, owinResponse);
                }
            }
            finally
            {
                request.DisposeRequestResources();
                request.Dispose();
                if (response != null)
                {
                    response.Dispose();
                }
            }

            // Call the next component if no route matched
            if (callNext && Next != null)
            {
                await Next.Invoke(context);
            }
        }

        private static HttpContent CreateStreamedRequestContent(IOwinRequest owinRequest)
        {
            // Note that we must NOT dispose owinRequest.Body in this case. Disposing it would close the input
            // stream and prevent cascaded components from accessing it. The server MUST handle any necessary
            // cleanup upon request completion. NonOwnedStream prevents StreamContent (or its callers including
            // HttpRequestMessage) from calling Close or Dispose on owinRequest.Body.
            return new StreamContent(new NonOwnedStream(owinRequest.Body));
        }

        private static async Task<HttpContent> CreateBufferedRequestContentAsync(IOwinRequest owinRequest)
        {
            // We need to replace the request body with a buffered stream so that other components can read the stream.
            // For this stream to be useful, it must NOT be diposed along with the request. Streams created by
            // StreamContent do get disposed along with the request, so use MemoryStream to buffer separately.
            MemoryStream buffer = new MemoryStream();

            using (StreamContent copier = new StreamContent(owinRequest.Body))
            {
                await copier.CopyToAsync(buffer);
            }

            // Provide the non-disposing, buffered stream to later OWIN components (set to the stream's beginning).
            buffer.Position = 0;
            owinRequest.Body = buffer;

            // For MemoryStream, Length is guaranteed to be an int.
            return new ByteArrayContent(buffer.GetBuffer(), 0, (int)buffer.Length);
        }

        private static HttpRequestMessage CreateRequestMessage(IOwinRequest owinRequest, HttpContent requestContent)
        {
            // Create the request
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(owinRequest.Method), owinRequest.Uri);

            try
            {
                // Set the body
                request.Content = requestContent;

                // Copy the headers
                foreach (KeyValuePair<string, string[]> header in owinRequest.Headers)
                {
                    if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        bool success = requestContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        Contract.Assert(success,
                            "Every header can be added either to the request headers or to the content headers");
                    }
                }
            }
            catch
            {
                request.Dispose();
                throw;
            }

            return request;
        }

        private static void MapRequestProperties(HttpRequestMessage request, IOwinContext context)
        {
            // Set the OWIN context on the request
            request.SetOwinContext(context);

            // Set a request context on the request that lazily populates each property.
            HttpRequestContext requestContext = new OwinHttpRequestContext(context, request);
            request.SetRequestContext(requestContext);
        }

        private static void SetPrincipal(IPrincipal user)
        {
            if (user != null)
            {
                Thread.CurrentPrincipal = user;
            }
        }

        private static bool IsSoftNotFound(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                bool routingFailure;
                if (request.Properties.TryGetValue<bool>(HttpPropertyKeys.NoRouteMatched, out routingFailure)
                    && routingFailure)
                {
                    return true;
                }
            }
            return false;
        }

        private static async Task<HttpResponseMessage> BufferResponseBodyAsync(HttpRequestMessage request,
            HttpResponseMessage response)
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

                try
                {
                    await response.Content.LoadIntoBufferAsync();
                }
                catch
                {
                    response.Dispose();
                    // We tried to send back an error with the exception, but we couldn't. It's an edge case; the best
                    // we can do is to send back and empty 500.
                    response = request.CreateResponse(HttpStatusCode.InternalServerError);
                }
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

        private static Task SendResponseMessageAsync(HttpResponseMessage response, IOwinResponse owinResponse)
        {
            owinResponse.StatusCode = (int)response.StatusCode;
            owinResponse.ReasonPhrase = response.ReasonPhrase;

            // Copy non-content headers
            IDictionary<string, string[]> responseHeaders = owinResponse.Headers;
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

                return SendResponseContentAsync(responseContent, owinResponse.Body);
            }
        }

        private static async Task SendResponseContentAsync(HttpContent content, Stream body)
        {
            try
            {
                await content.CopyToAsync(body);
            }
            catch
            {
                AbortResponseStream(body);
            }
        }

        private static void AbortResponseStream(Stream body)
        {
            // OWIN doesn't yet support an explicit Abort even. Calling Dispose on the body seems like the best we can
            // do for nowe.
            body.Dispose();
        }

        private static HttpMessageHandlerOptions CreateOptions(HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }

            if (bufferPolicySelector == null)
            {
                throw new ArgumentNullException("bufferPolicySelector");
            }

            return new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = bufferPolicySelector
            };
        }

        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.
        /// </param>
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
