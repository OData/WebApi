// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETSTANDARD2_0
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce a Conflict (409) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 409.</remarks>
    public class ConflictODataResult : ConflictResult, IODataErrorResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">Error Message</param>
        public ConflictODataResult(string message)
        {
            if (message == null)
            {
                throw Common.Error.ArgumentNull("message");
            }

            Error = new ODataError
            {
                Message = message,
                ErrorCode = "409"
            };
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="odataError">OData Error.</param>
        public ConflictODataResult(ODataError odataError)
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
                StatusCode = StatusCodes.Status409Conflict
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
#endif