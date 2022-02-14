//-----------------------------------------------------------------------------
// <copyright file="ODataBatchHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        internal const string PreferenceContinueOnError = "continue-on-error";
        internal const string PreferenceContinueOnErrorFalse = "continue-on-error=false";

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
        /// <param name="header">The request header.</param>
        /// <param name="enableContinueOnErrorHeader">Flag indicating if continue on error header is enabled.</param>
        internal void SetContinueOnError(IWebApiHeaders header, bool enableContinueOnErrorHeader)
        {
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(header);
            if ((preferHeader != null && preferHeader.Contains(PreferenceContinueOnError) && !preferHeader.Contains(PreferenceContinueOnErrorFalse)) 
                || (!enableContinueOnErrorHeader))
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
