// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
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
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IExceptionHandler _exceptionHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        protected HttpBatchHandler(HttpServer httpServer)
            : this(EnsureNonNull(httpServer), CreateExceptionLogger(httpServer), CreateExceptionHandler(httpServer))
        {
        }

        internal HttpBatchHandler(HttpServer httpServer, IExceptionLogger exceptionLogger,
            IExceptionHandler exceptionHandler)
        {
            if (httpServer == null)
            {
                throw Error.ArgumentNull("httpServer");
            }

            Contract.Assert(exceptionLogger != null);
            Contract.Assert(exceptionHandler != null);

            Invoker = new HttpMessageInvoker(httpServer);
            _exceptionLogger = exceptionLogger;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Gets the invoker to send the batch requests to the <see cref="HttpServer"/>.
        /// </summary>
        public HttpMessageInvoker Invoker { get; private set; }

        internal IExceptionLogger ExceptionLogger
        {
            get { return _exceptionLogger; }
        }

        internal IExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler; }
        }

        /// <inheritdoc/>
        protected sealed override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[HttpPropertyKeys.IsBatchRequest] = true;

            ExceptionDispatchInfo exceptionInfo = null;

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
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            Debug.Assert(exceptionInfo != null);
            Debug.Assert(exceptionInfo.SourceException != null);

            ExceptionContext exceptionContext = new ExceptionContext(exceptionInfo.SourceException, request,
                ExceptionCatchBlocks.HttpBatchHandler, isTopLevelCatchBlock: false);
            await _exceptionLogger.LogAsync(exceptionContext, canBeHandled: true,
                cancellationToken: cancellationToken);
            HttpResponseMessage response = await _exceptionHandler.HandleAsync(exceptionContext, cancellationToken);

            if (response == null)
            {
                exceptionInfo.Throw();
            }

            return response;
        }

        /// <summary>
        /// Processes the incoming batch request as a single <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The batch request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The batch response.</returns>
        public abstract Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken);

        private static IExceptionHandler CreateExceptionHandler(HttpServer httpServer)
        {
            Contract.Assert(httpServer != null);
            HttpConfiguration configuration = httpServer.Configuration;
            Contract.Assert(configuration != null);

            return ExceptionServices.CreateHandler(configuration);
        }

        private static IExceptionLogger CreateExceptionLogger(HttpServer httpServer)
        {
            Contract.Assert(httpServer != null);
            HttpConfiguration configuration = httpServer.Configuration;
            Contract.Assert(configuration != null);

            return ExceptionServices.CreateLogger(configuration);
        }

        private static HttpServer EnsureNonNull(HttpServer httpServer)
        {
            if (httpServer == null)
            {
                throw Error.ArgumentNull("httpServer");
            }

            return httpServer;
        }
    }
}