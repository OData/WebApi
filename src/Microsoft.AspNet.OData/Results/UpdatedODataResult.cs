﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a PUT, PATCH, or a MERGE operation on an OData entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header.</remarks>
    public class UpdatedODataResult<T> : IHttpActionResult
    {
        private readonly NegotiatedContentResult<T> _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public UpdatedODataResult(T entity, ApiController controller)
            : this(new NegotiatedContentResult<T>(HttpStatusCode.OK, CheckNull(entity), controller))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public UpdatedODataResult(T entity, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : this(new NegotiatedContentResult<T>(HttpStatusCode.OK, CheckNull(entity), contentNegotiator, request, formatters))
        {
        }

        private UpdatedODataResult(NegotiatedContentResult<T> innerResult)
        {
            Contract.Assert(innerResult != null);
            _innerResult = innerResult;
        }

        /// <summary>
        /// Gets the entity that was updated.
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
        /// Gets the formatters to use to negotiate and format the content.
        /// </summary>
        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get
            {
                return _innerResult.Formatters;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            IHttpActionResult result = GetInnerActionResult();
            var response = await result.ExecuteAsync(cancellationToken);
            ResultHelpers.AddServiceVersion(response, () => ODataUtils.ODataVersionToString(ResultHelpers.GetODataResponseVersion(Request)));
            return response;
        }

        internal IHttpActionResult GetInnerActionResult()
        {
            if (RequestPreferenceHelpers.RequestPrefersReturnContent(new WebApiRequestHeaders(_innerResult.Request.Headers)))
            {
                return _innerResult;
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, _innerResult.Request);
            }
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
