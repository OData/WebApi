// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Defines the abstraction for handling OData batch requests.
    /// </summary>
    public abstract partial class ODataBatchHandler
    {
        // Maxing out the received message size as we depend on the hosting layer to enforce this limit.
        private ODataMessageQuotas _messageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };

        // Preference odata.continue-on-error.
        internal const string PreferenceContinueOnError = "odata.continue-on-error";

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
        /// Gets or sets if the continue-on-error header is enable or not.
        /// </summary>
        internal bool ContinueOnError { get; private set; }

        /// <summary>
        /// Set ContinueOnError based on the request and headers.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="header">The request header.</param>
        internal void SetContinueOnError(IWebApiRequestMessage request, IWebApiHeaders header)
        {
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(header);
            if ((preferHeader != null && preferHeader.Contains(PreferenceContinueOnError)) || (!request.Options.EnableContinueOnErrorHeader))
            {
                ContinueOnError = true;
            }
            else
            {
                ContinueOnError = false;
            }
        }
    }
}
