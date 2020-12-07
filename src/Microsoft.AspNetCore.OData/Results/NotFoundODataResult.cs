// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce a Not Found(404) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 404.</remarks>
    public class NotFoundODataResult : NotFoundResult
    {
        /// <summary>
        /// OData Error Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError ODataError { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public NotFoundODataResult()
        {
            ODataError oDataError = new ODataError
            {
                ErrorCode = "404",
                Message = "Not found"
            };
            ODataError = oDataError;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">Error Message</param>
        public NotFoundODataResult(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="odataError">OData Error.</param>
        public NotFoundODataResult(ODataError odataError)
        {
            ODataError = odataError;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ODataError oDataError;
            if (ODataError != null)
            {
                oDataError = ODataError;
            }
            else
            {
                oDataError = new ODataError
                {
                    ErrorCode = "404",
                    Message = Message != null ? Message : "Not found"
                };
            }

            ObjectResult objectResult = new ObjectResult(oDataError)
            {
                StatusCode = StatusCodes.Status404NotFound
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
