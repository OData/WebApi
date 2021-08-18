//-----------------------------------------------------------------------------
// <copyright file="CreatedODataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a create operation that adds an entity to an entity set.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header. It generates a location
    /// header containing the edit link of the created entity and, if response has status code: NoContent, also
    /// generates an OData-EntityId header.</remarks>
    public class CreatedODataResult<T> : IHttpActionResult
    {
        private readonly NegotiatedContentResult<T> _innerResult;
        private Uri _locationHeader;

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
                _locationHeader = _locationHeader ?? GenerateLocationHeader(Request);
                return _locationHeader;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            IHttpActionResult result = GetInnerActionResult(Request);
            HttpResponseMessage response = await result.ExecuteAsync(cancellationToken);
            response.Headers.Location = LocationHeader;
            ResultHelpers.AddEntityId(response, () => GenerateEntityId(Request));
            ResultHelpers.AddServiceVersion(response, () => ODataUtils.ODataVersionToString(ResultHelpers.GetODataResponseVersion(Request)));
            return response;
        }

        internal IHttpActionResult GetInnerActionResult(HttpRequestMessage request)
        {
            WebApiRequestHeaders headers = new WebApiRequestHeaders(request.Headers);
            if (RequestPreferenceHelpers.RequestPrefersReturnNoContent(headers))
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, request);
            }
            else
            {
                return _innerResult;
            }
        }

        internal Uri GenerateEntityId(HttpRequestMessage request)
        {
            return ResultHelpers.GenerateODataLink(request, Entity, isEntityId: true);
        }

        internal Uri GenerateLocationHeader(HttpRequestMessage request)
        {
            return ResultHelpers.GenerateODataLink(request, Entity, isEntityId: false);
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
