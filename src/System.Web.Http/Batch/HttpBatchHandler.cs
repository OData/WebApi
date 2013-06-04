// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace System.Web.Http.Batch
{
    /// <summary>
    /// Defines the abstraction for handling HTTP batch requests.
    /// </summary>
    // Class Hierarchy
    // - HttpBatchHandler
    //   - DefaultHttpBatchHandler
    //   - ODataBatchHandler
    //     - DefaultODataBatchHandler
    //     - UnbufferedODataBatchHandler
    public abstract class HttpBatchHandler : HttpMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        protected HttpBatchHandler(HttpServer httpServer)
        {
            if (httpServer == null)
            {
                throw Error.ArgumentNull("httpServer");
            }

            Invoker = new HttpMessageInvoker(httpServer);
        }

        /// <summary>
        /// Gets the invoker to send the batch requests to the <see cref="HttpServer"/>.
        /// </summary>
        public HttpMessageInvoker Invoker { get; private set; }

        /// <inheritdoc/>
        protected sealed override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[HttpPropertyKeys.IsBatchRequest] = true;

            try
            {
                return await ProcessBatchAsync(request, cancellationToken);
            }
            catch (HttpResponseException httpResponseException)
            {
                return httpResponseException.Response;
            }
            catch (Exception exception)
            {
                return request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
            }
        }

        /// <summary>
        /// Processes the incoming batch request as a single <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The batch request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The batch response.</returns>
        public abstract Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}