// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce an <see cref="ActionResult"/>.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> response.</remarks>
    public class ODataErrorResult : ActionResult, IODataErrorResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ODataErrorResult(string errorCode, string message)
        {
            if (errorCode == null)
            {
                throw Common.Error.ArgumentNull("errorCode");
            }

            if (message == null)
            {
                throw Common.Error.ArgumentNull("message");
            }

            Error = new ODataError
            {
                ErrorCode = errorCode,
                Message = message
            };
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ODataErrorResult(ODataError odataError)
        {
            if (odataError == null)
            {
                throw Common.Error.ArgumentNull("odataError");
            }

            Error = odataError;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(Error)
            {
                StatusCode = Convert.ToInt32(Error.ErrorCode)
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
