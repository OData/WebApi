// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;

namespace System.Web.Http
{
    /// <summary>
    /// Defines an implementation of an <see cref="HttpMessageHandler"/> which dispatches an 
    /// incoming <see cref="HttpRequestMessage"/> and creates an <see cref="HttpResponseMessage"/> as a result.
    /// </summary>
    public class HttpServer : DelegatingHandler
    {
        private static readonly Lazy<IPrincipal> _anonymousPrincipal = new Lazy<IPrincipal>(() => new GenericPrincipal(new GenericIdentity(String.Empty), new string[0]), isThreadSafe: true);
        private readonly HttpConfiguration _configuration;
        private readonly HttpMessageHandler _dispatcher;
        private bool _disposed;
        private bool _initialized = false;
        private object _initializationLock = new object();
        private object _initializationTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class with default configuration and dispatcher.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The configuration object is disposed as part of this class.")]
        public HttpServer()
            : this(new HttpConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class with default dispatcher.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure this <see cref="HttpServer"/> instance.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The configuration object is disposed as part of this class.")]
        public HttpServer(HttpConfiguration configuration)
            : this(configuration, new HttpControllerDispatcher(configuration))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class with a custom dispatcher.
        /// </summary>
        /// <param name="dispatcher">Http dispatcher responsible for handling incoming requests.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The configuration object is disposed as part of this class.")]
        public HttpServer(HttpControllerDispatcher dispatcher)
            : this(new HttpConfiguration(), dispatcher)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure this <see cref="HttpServer"/> instance.</param>
        /// <param name="dispatcher">Http dispatcher responsible for handling incoming requests.</param>
        public HttpServer(HttpConfiguration configuration, HttpMessageHandler dispatcher)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (dispatcher == null)
            {
                throw Error.ArgumentNull("dispatcher");
            }

            _dispatcher = dispatcher;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        public HttpMessageHandler Dispatcher
        {
            get { return _dispatcher; }
        }

        /// <summary>
        /// Gets the <see cref="HttpConfiguration"/>.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged SRResources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _configuration.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Dispatches an incoming <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the ongoing operation.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner.")]
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (_disposed)
            {
                return TaskHelpers.FromResult(request.CreateResponse(HttpStatusCode.ServiceUnavailable));
            }

            // The first request initializes the server
            EnsureInitialized();

            // Capture current synchronization context and add it as a parameter to the request
            SynchronizationContext context = SynchronizationContext.Current;
            if (context != null)
            {
                request.Properties.Add(HttpPropertyKeys.SynchronizationContextKey, context);
            }

            // Add HttpConfiguration object as a parameter to the request 
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, _configuration);

            // Ensure we have a principal, even if the host didn't give us one
            IPrincipal originalPrincipal = Thread.CurrentPrincipal;
            if (originalPrincipal == null)
            {
                Thread.CurrentPrincipal = _anonymousPrincipal.Value;
            }

            return base.SendAsync(request, cancellationToken)
                       .Finally(() => Thread.CurrentPrincipal = originalPrincipal);
        }

        private void EnsureInitialized()
        {
            LazyInitializer.EnsureInitialized(ref _initializationTarget, ref _initialized, ref _initializationLock, () =>
            {
                Initialize();

                // Attach tracing before creating pipeline to allow injection of message handlers
                ITraceManager traceManager = _configuration.Services.GetTraceManager();
                Contract.Assert(traceManager != null);
                traceManager.Initialize(_configuration);

                // Create pipeline
                InnerHandler = HttpPipelineFactory.Create(_configuration.MessageHandlers, _dispatcher);

                return null;
            });
        }

        /// <summary>
        /// Prepares the server for operation.
        /// </summary>
        /// <remarks>
        /// This method must be called after all configuration is complete
        /// but before the first request is processed.
        /// </remarks>
        protected virtual void Initialize()
        {
            // Register the default IRequiredMemberSelector for formatters that haven't been assigned one
            ModelMetadataProvider metadataProvider = _configuration.Services.GetModelMetadataProvider();
            IEnumerable<ModelValidatorProvider> validatorProviders = _configuration.Services.GetModelValidatorProviders();
            IRequiredMemberSelector defaultRequiredMemberSelector = new ModelValidationRequiredMemberSelector(metadataProvider, validatorProviders);

            foreach (MediaTypeFormatter formatter in _configuration.Formatters)
            {
                if (formatter.RequiredMemberSelector == null)
                {
                    formatter.RequiredMemberSelector = defaultRequiredMemberSelector;
                }
            }
        }
    }
}
