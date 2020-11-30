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
    public class UnauthorizedODataResult : UnauthorizedResult
    {
        private string _message;

        /// <summary>
        /// Instantiate the Class.
        /// </summary>
        /// <param name="message">Error Message</param>
        public UnauthorizedODataResult(string message)
        {
            _message = message;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ODataError oDataError = new ODataError
            {
                ErrorCode = "401",
                Message = _message
            };

            ObjectResult objectResult = new ObjectResult(oDataError)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
