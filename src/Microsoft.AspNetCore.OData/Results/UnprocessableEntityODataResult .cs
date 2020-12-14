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
    /// Represents a result that when executed will produce a UnprocessableEntity (422) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 422.</remarks>
    public class UnprocessableEntityODataResult : UnprocessableEntityResult, IODataErrorResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">Error Message</param>
        public UnprocessableEntityODataResult(string message)
        {
            if (message == null)
            {
                throw Common.Error.ArgumentNull("message");
            }

            Error = new ODataError
            {
                Message = message,
                ErrorCode = "422"
            };
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="odataError">OData Error.</param>
        public UnprocessableEntityODataResult(ODataError odataError)
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
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
#endif
