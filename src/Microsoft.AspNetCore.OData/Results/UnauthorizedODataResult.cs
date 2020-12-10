// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce a Unauthorized(401) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 404.</remarks>
    public class UnauthorizedODataResult : UnauthorizedResult, IODataErrorResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">Error Message</param>
        public UnauthorizedODataResult(string message)
        {
            Error = new ODataError
            {
                Message = message,
                ErrorCode = "401"
            };
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="odataError">OData Error.</param>
        public UnauthorizedODataResult(ODataError odataError)
        {
            Error = odataError;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(Error)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
