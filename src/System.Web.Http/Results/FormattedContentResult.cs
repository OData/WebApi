// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>Represents an action result that returns formatted content.</summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class FormattedContentResult<T> : IHttpActionResult
    {
        private readonly HttpStatusCode _statusCode;
        private readonly T _content;
        private readonly MediaTypeFormatter _formatter;
        private readonly MediaTypeHeaderValue _mediaType;
        private readonly StatusCodeResult.IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedContentResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="content">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">
        /// The value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </param>
        /// <param name="request">The request message which led to this result.</param>
        public FormattedContentResult(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, HttpRequestMessage request)
            : this(statusCode, content, formatter, mediaType, new StatusCodeResult.DirectDependencyProvider(request))
        {
        }

        internal FormattedContentResult(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, ApiController controller)
            : this(statusCode, content, formatter, mediaType, new StatusCodeResult.ApiControllerDependencyProvider(
                controller))
        {
        }

        private FormattedContentResult(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, StatusCodeResult.IDependencyProvider dependencies)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            Contract.Assert(dependencies != null);

            _statusCode = statusCode;
            _content = content;
            _formatter = formatter;
            _mediaType = mediaType;
            _dependencies = dependencies;
        }

        /// <summary>Gets the HTTP status code for the response message.</summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <summary>Gets the content value to format in the entity body.</summary>
        public T Content
        {
            get { return _content; }
        }

        /// <summary>Gets the formatter to use to format the content.</summary>
        public MediaTypeFormatter Formatter
        {
            get { return _formatter; }
        }

        /// <summary>
        /// Gets the value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </summary>
        public MediaTypeHeaderValue MediaType
        {
            get { return _mediaType; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            return Execute(_statusCode, _content, _formatter, _mediaType, _dependencies.Request);
        }

        internal static HttpResponseMessage Execute(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, HttpRequestMessage request)
        {
            HttpResponseMessage mutableResponse = new HttpResponseMessage(statusCode);
            HttpResponseMessage response = null;

            try
            {
                mutableResponse.Content = new ObjectContent<T>(content, formatter, mediaType);
                mutableResponse.RequestMessage = request;

                response = mutableResponse;
                mutableResponse = null;
            }
            finally
            {
                if (mutableResponse != null)
                {
                    mutableResponse.Dispose();
                }
            }

            return response;
        }
    }
}
