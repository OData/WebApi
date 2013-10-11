// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Dispatches an incoming <see cref="HttpRequestMessage"/> to an <see cref="IHttpController"/> implementation for processing.
    /// </summary>
    public class HttpControllerDispatcher : HttpMessageHandler
    {
        private readonly HttpConfiguration _configuration;
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IExceptionHandler _exceptionHandler;

        private IHttpControllerSelector _controllerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerDispatcher"/> class.
        /// </summary>
        public HttpControllerDispatcher(HttpConfiguration configuration)
            : this(configuration, ExceptionServices.CreateLogger(EnsureNonNull(configuration)),
            ExceptionServices.CreateHandler(configuration))
        {
        }

        internal HttpControllerDispatcher(HttpConfiguration configuration, IExceptionLogger exceptionLogger,
            IExceptionHandler exceptionHandler)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            Contract.Assert(exceptionLogger != null);
            Contract.Assert(exceptionHandler != null);

            _configuration = configuration;
            _exceptionLogger = exceptionLogger;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Gets the <see cref="HttpConfiguration"/>.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
        }

        internal IExceptionLogger ExceptionLogger
        {
            get { return _exceptionLogger; }
        }

        internal IExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler; }
        }

        private IHttpControllerSelector ControllerSelector
        {
            get
            {
                if (_controllerSelector == null)
                {
                    _controllerSelector = _configuration.Services.GetHttpControllerSelector();
                }

                return _controllerSelector;
            }
        }

        /// <summary>
        /// Dispatches an incoming <see cref="HttpRequestMessage"/> to an <see cref="IHttpController"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the ongoing operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We report the error in the HTTP response.")]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo exceptionInfo = null;

            try
            {
                return await SendAsyncCore(request, cancellationToken);
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
                ExceptionCatchBlocks.HttpControllerDispatcher, isTopLevelCatchBlock: false);
            await _exceptionLogger.LogAsync(exceptionContext, canBeHandled: true,
                cancellationToken: cancellationToken);
            HttpResponseMessage response = await _exceptionHandler.HandleAsync(exceptionContext, cancellationToken);

            if (response == null)
            {
                exceptionInfo.Throw();
            }

            return response;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner.")]
        private Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IHttpRouteData routeData = request.GetRouteData();
            Contract.Assert(routeData != null);

            HttpControllerDescriptor httpControllerDescriptor = ControllerSelector.SelectController(request);
            if (httpControllerDescriptor == null)
            {
                return TaskHelpers.FromResult(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    SRResources.NoControllerSelected));
            }

            IHttpController httpController = httpControllerDescriptor.CreateController(request);
            if (httpController == null)
            {
                return TaskHelpers.FromResult(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    SRResources.NoControllerCreated));
            }

            HttpConfiguration controllerConfiguration = httpControllerDescriptor.Configuration;

            // Set the controller configuration on the request properties
            HttpConfiguration requestConfig = request.GetConfiguration();
            if (requestConfig == null)
            {
                request.SetConfiguration(controllerConfiguration);
            }
            else
            {
                if (requestConfig != controllerConfiguration)
                {
                    request.SetConfiguration(controllerConfiguration);
                }
            }

            HttpRequestContext requestContext = request.GetRequestContext();

            // if the host doesn't create the context we will fallback to creating it.
            if (requestContext == null)
            {
                requestContext = new RequestBackedHttpRequestContext(request)
                {
                    // we are caching controller configuration to support per controller configuration.
                    Configuration = controllerConfiguration,
                };

                // if the host did not set a request context we will also set it back to the request.
                request.SetRequestContext(requestContext);
            }

            // Create context
            HttpControllerContext controllerContext = new HttpControllerContext(requestContext, request,
                httpControllerDescriptor, httpController);

            return httpController.ExecuteAsync(controllerContext, cancellationToken);
        }

        private static HttpConfiguration EnsureNonNull(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            return configuration;
        }
    }
}
