// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace System.Web.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a POST operation with an entity to an entity set.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header and generates a location header
    /// that is the same as the edit link of the created entity.</remarks>
    public class CreatedODataResult<T> : IHttpActionResult
    {
        private readonly NegotiatedContentResult<T> _innerResult;
        private Uri _locationHeader;
        private Uri _entityIdHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The created entity.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public CreatedODataResult(T entity, ApiController controller)
            : this(new NegotiatedContentResult<T>(HttpStatusCode.Created, CheckNull(entity), controller))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The created entity.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        /// <param name="locationHeader">The location header for the created entity.</param>
        public CreatedODataResult(T entity, IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters, Uri locationHeader)
            : this(new NegotiatedContentResult<T>(HttpStatusCode.Created, CheckNull(entity), contentNegotiator, request, formatters))
        {
            if (locationHeader == null)
            {
                throw Error.ArgumentNull("locationHeader");
            }

            _locationHeader = locationHeader;
        }

        private CreatedODataResult(NegotiatedContentResult<T> innerResult)
        {
            Contract.Assert(innerResult != null);
            _innerResult = innerResult;
        }

        /// <summary>
        /// Gets the entity that was created.
        /// </summary>
        public T Entity
        {
            get
            {
                return _innerResult.Content;
            }
        }

        /// <summary>
        /// Gets the content negotiator to handle content negotiation.
        /// </summary>
        public IContentNegotiator ContentNegotiator
        {
            get
            {
                return _innerResult.ContentNegotiator;
            }
        }

        /// <summary>
        /// Gets the request message which led to this result.
        /// </summary>
        public HttpRequestMessage Request
        {
            get
            {
                return _innerResult.Request;
            }
        }

        /// <summary>
        /// Gets the formatters to use to negotiate and format the created entity.
        /// </summary>
        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get
            {
                return _innerResult.Formatters;
            }
        }

        /// <summary>
        /// Gets the location header of the created entity.
        /// </summary>
        public Uri LocationHeader
        {
            get
            {
                _locationHeader = _locationHeader ?? GenerateLocationHeader();
                return _locationHeader;
            }
        }

        // internal just for unit test.
        internal Uri EntityId
        {
            get
            {
                _entityIdHeader = _entityIdHeader ?? ResultHelpers.GenerateODataLink(Request, Entity, isEntityId: true);
                return _entityIdHeader;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            IHttpActionResult result = GetInnerActionResult();
            HttpResponseMessage response = await result.ExecuteAsync(cancellationToken);
            response.Headers.Location = LocationHeader;
            ResultHelpers.AddEntityId(response, () => EntityId);

            return response;
        }

        internal IHttpActionResult GetInnerActionResult()
        {
            if (RequestPreferenceHelpers.RequestPrefersReturnNoContent(Request))
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }
            else
            {
                return _innerResult;
            }
        }

        internal Uri GenerateLocationHeader()
        {
            return ResultHelpers.GenerateODataLink(Request, Entity, isEntityId: false);
        }

        private static T CheckNull(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            return entity;
        }
    }
}
