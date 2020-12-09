// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce an <see cref="ActionResult"/>.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> response.</remarks>
    public class ODataErrorResult : ActionResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError ODataError { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ODataErrorResult(string errorCode, string message)
        {
            ODataError oDataError = new ODataError
            {
                ErrorCode = errorCode,
                Message = message
            };
            ODataError = oDataError;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(ODataError)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
