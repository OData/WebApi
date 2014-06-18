// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Batch;
using Microsoft.OData.Core;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// Defines the abstraction for handling OData batch requests.
    /// </summary>
    public abstract class ODataBatchHandler : HttpBatchHandler
    {
        // Maxing out the received message size as we depend on the hosting layer to enforce this limit.
        private ODataMessageQuotas _messageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        protected ODataBatchHandler(HttpServer httpServer)
            : base(httpServer)
        {
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageQuotas"/> used for reading/writing the batch request/response.
        /// </summary>
        public ODataMessageQuotas MessageQuotas
        {
            get { return _messageQuotas; }
        }

        /// <summary>
        /// Gets or sets the name of the OData route associated with this batch handler.
        /// </summary>
        public string ODataRouteName { get; set; }

        /// <summary>
        /// Creates the batch response message.
        /// </summary>
        /// <param name="responses">The responses for the batch requests.</param>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The batch response message.</returns>
        public virtual Task<HttpResponseMessage> CreateResponseMessageAsync(
            IEnumerable<ODataBatchResponseItem> responses,
            HttpRequestMessage request,
            CancellationToken cancellationToken)
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
        public virtual void ValidateRequest(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.ValidateODataBatchRequest();
        }

        /// <summary>
        /// Gets the base URI for the batched requests.
        /// </summary>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <returns>The base URI.</returns>
        public virtual Uri GetBaseUri(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetODataBatchBaseUri(ODataRouteName);
        }
    }
}