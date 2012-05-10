// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Dispatches an incoming <see cref="HttpRequestMessage"/> to an <see cref="IHttpController"/> implementation for processing.
    /// </summary>
    public class HttpControllerDispatcher : HttpMessageHandler
    {
        private IHttpControllerSelector _controllerSelector;
        private readonly HttpConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerDispatcher"/> class.
        /// </summary>
        public HttpControllerDispatcher(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Gets the <see cref="HttpConfiguration"/>.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the ongoing operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We report the error in the HTTP response.")]
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Runs Content Negotiation and Error Handling on the result of SendAsyncInternal
            try
            {
                return SendAsyncInternal(request, cancellationToken)
                      .Catch(info => info.Handled(HandleException(request, info.Exception)));
            }
            catch (Exception exception)
            {
                return TaskHelpers.FromResult(HandleException(request, exception));
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner.")]
        private Task<HttpResponseMessage> SendAsyncInternal(HttpRequestMessage request, CancellationToken cancellationToken)
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

            // Set the controller configuration on the request properties
            HttpConfiguration requestConfig = request.GetConfiguration();
            if (requestConfig == null)
            {
                request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, httpControllerDescriptor.Configuration);
            }
            else
            {
                if (requestConfig != httpControllerDescriptor.Configuration)
                {
                    request.Properties[HttpPropertyKeys.HttpConfigurationKey] = httpControllerDescriptor.Configuration;
                }
            }

            // Create context
            HttpControllerContext controllerContext = new HttpControllerContext(httpControllerDescriptor.Configuration, routeData, request);
            controllerContext.Controller = httpController;
            controllerContext.ControllerDescriptor = httpControllerDescriptor;

            return httpController.ExecuteAsync(controllerContext, cancellationToken);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns HttpResponseMessage instance.")]
        private static HttpResponseMessage HandleException(HttpRequestMessage request, Exception exception)
        {
            Exception unwrappedException = exception.GetBaseException();
            HttpResponseException httpResponseException = unwrappedException as HttpResponseException;

            if (httpResponseException != null)
            {
                return httpResponseException.Response;
            }

            return request.CreateErrorResponse(HttpStatusCode.InternalServerError, unwrappedException);
        }
    }
}
