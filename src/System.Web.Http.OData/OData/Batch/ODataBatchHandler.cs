// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Batch;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Batch
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
    }
}