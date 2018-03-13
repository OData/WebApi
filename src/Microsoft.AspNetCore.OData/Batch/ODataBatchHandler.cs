// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Defines the abstraction for handling OData batch requests.
    /// </summary>
    /// <remarks>
    /// This class implements a BatchHandler semantics for AspNetCore, which uses
    /// an <see cref="IRouter"/> for dispatching requests.
    /// </remarks>
    public abstract partial class ODataBatchHandler
    {
        /// <summary>
        /// Gets or sets the OData route associated with this batch handler.
        /// </summary>
        public ODataRoute ODataRoute { get; set; }

        /// <summary>
        /// Abstract method for processing a batch request.
        /// </summary>
        /// <param name="context">The http content.</param>
        /// ><param name="nextHandler">The next handler in the middleware chain.</param>
        /// <returns></returns>
        public abstract Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler);

        /// <summary>
        /// Creates the batch response message.
        /// </summary>
        /// <param name="responses">The responses for the batch requests.</param>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <returns>The batch response message.</returns>
        public virtual Task CreateResponseMessageAsync(IEnumerable<ODataBatchResponseItem> responses, HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.CreateODataBatchResponseAsync(responses, MessageQuotas);
        }

        /// <summary>
        /// Validates the incoming request that contains the batch request messages.
        /// </summary>
        /// <param name="request">The request containing the batch request messages.</param>
        /// <returns>true if the request is valid, otherwise false.</returns>
        public virtual Task<bool> ValidateRequest(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ValidateODataBatchRequest();
        }

        /// <summary>
        /// Gets the base URI for the batched requests.
        /// </summary>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <returns>The base URI.</returns>
        public virtual Uri GetBaseUri(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetODataBatchBaseUri(ODataRouteName, ODataRoute);
        }
    }
}