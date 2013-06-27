// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns a <see cref="HttpStatusCode.BadRequest"/> response and performs
    /// content negotiation on an <see cref="HttpError"/> based on a <see cref="ModelStateDictionary"/>.
    /// </summary>
    public class InvalidModelStateResult : IHttpActionResult
    {
        private readonly ModelStateDictionary _modelState;
        private readonly IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="InvalidModelStateResult"/> class.</summary>
        /// <param name="modelState">The model state to include in the error.</param>
        /// <param name="includeErrorDetail">
        /// <see langword="true"/> if the error should include exception messages; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public InvalidModelStateResult(ModelStateDictionary modelState, bool includeErrorDetail,
            IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
            : this(modelState, new DirectDependencyProvider(includeErrorDetail, contentNegotiator, request,
                formatters))
        {
        }

        internal InvalidModelStateResult(ModelStateDictionary modelState, ApiController controller)
            : this(modelState, new ApiControllerDependencyProvider(controller))
        {
        }

        private InvalidModelStateResult(ModelStateDictionary modelState,
            IDependencyProvider dependencies)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException("modelState");
            }

            Contract.Assert(dependencies != null);

            _modelState = modelState;
            _dependencies = dependencies;
        }

        /// <summary>Gets the model state to include in the error.</summary>
        public ModelStateDictionary ModelState
        {
            get { return _modelState; }
        }

        /// <summary>Gets a value indicating whether the error should include exception messages.</summary>
        public bool IncludeErrorDetail
        {
            get { return _dependencies.IncludeErrorDetail; }
        }

        /// <summary>Gets the content negotiator to handle content negotiation.</summary>
        public IContentNegotiator ContentNegotiator
        {
            get { return _dependencies.ContentNegotiator; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <summary>Gets the formatters to use to negotiate and format the content.</summary>
        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get { return _dependencies.Formatters; }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpError error = new HttpError(_modelState, _dependencies.IncludeErrorDetail);
            return NegotiatedContentResult<HttpError>.Execute(HttpStatusCode.BadRequest, error,
                _dependencies.ContentNegotiator, _dependencies.Request, _dependencies.Formatters);
        }

        /// <summary>Defines a provider for dependencies that are not always directly available.</summary>
        /// <remarks>
        /// This abstraction supports the unit testing scenario of creating the result without creating a content
        /// negotiator, request message, or formatters. (The ApiController provider implementation does lazy evaluation
        /// to make that scenario work.)
        /// </remarks>
        private interface IDependencyProvider
        {
            bool IncludeErrorDetail { get; }

            IContentNegotiator ContentNegotiator { get; }

            HttpRequestMessage Request { get; }

            IEnumerable<MediaTypeFormatter> Formatters { get; }
        }

        private sealed class DirectDependencyProvider : IDependencyProvider
        {
            private readonly bool _includeErrorDetail;
            private readonly IContentNegotiator _contentNegotiator;
            private readonly HttpRequestMessage _request;
            private readonly IEnumerable<MediaTypeFormatter> _formatters;

            public DirectDependencyProvider(bool includeErrorDetail, IContentNegotiator contentNegotiator,
                HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            {
                if (contentNegotiator == null)
                {
                    throw new ArgumentNullException("contentNegotiator");
                }

                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (formatters == null)
                {
                    throw new ArgumentNullException("formatters");
                }

                _includeErrorDetail = includeErrorDetail;
                _contentNegotiator = contentNegotiator;
                _request = request;
                _formatters = formatters;
            }

            public bool IncludeErrorDetail
            {
                get { return _includeErrorDetail; }
            }

            public IContentNegotiator ContentNegotiator
            {
                get { return _contentNegotiator; }
            }

            public HttpRequestMessage Request
            {
                get { return _request; }
            }

            public IEnumerable<MediaTypeFormatter> Formatters
            {
                get { return _formatters; }
            }
        }

        private sealed class ApiControllerDependencyProvider : IDependencyProvider
        {
            private readonly ApiController _controller;

            private IDependencyProvider _resolved;

            public ApiControllerDependencyProvider(ApiController controller)
            {
                if (controller == null)
                {
                    throw new ArgumentNullException("controller");
                }

                _controller = controller;
            }

            public bool IncludeErrorDetail
            {
                get
                {
                    EnsureResolved();
                    return _resolved.IncludeErrorDetail;
                }
            }

            public IContentNegotiator ContentNegotiator
            {
                get
                {
                    EnsureResolved();
                    return _resolved.ContentNegotiator;
                }
            }

            public HttpRequestMessage Request
            {
                get
                {
                    EnsureResolved();
                    return _resolved.Request;
                }
            }

            public IEnumerable<MediaTypeFormatter> Formatters
            {
                get
                {
                    EnsureResolved();
                    return _resolved.Formatters;
                }
            }

            private void EnsureResolved()
            {
                if (_resolved == null)
                {
                    HttpConfiguration configuration = _controller.Configuration;

                    if (configuration == null)
                    {
                        throw new InvalidOperationException(
                            SRResources.HttpControllerContext_ConfigurationMustNotBeNull);
                    }

                    HttpRequestMessage request = _controller.Request;

                    if (request == null)
                    {
                        throw new InvalidOperationException(SRResources.ApiController_RequestMustNotBeNull);
                    }

                    bool includeErrorDetail = request.ShouldIncludeErrorDetail();

                    ServicesContainer services = configuration.Services;
                    Contract.Assert(services != null);
                    IContentNegotiator contentNegotiator = services.GetContentNegotiator();

                    if (contentNegotiator == null)
                    {
                        throw new InvalidOperationException(Error.Format(
                            SRResources.HttpRequestMessageExtensions_NoContentNegotiator, typeof(IContentNegotiator)));
                    }

                    IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;
                    Contract.Assert(formatters != null);

                    _resolved = new DirectDependencyProvider(includeErrorDetail, contentNegotiator, request,
                        formatters);
                }
            }
        }
    }
}
