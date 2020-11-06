// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce a Bad Request (400) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 400.</remarks>
    public class BadRequestODataResult : BadRequestResult
    {
        private string _message;

        /// <summary>
        /// Instantiate the Class.
        /// </summary>
        /// <param name="message">Error Message</param>
        public BadRequestODataResult(string message)
        {
            _message = message;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ODataError oDataError = new ODataError
            {
                ErrorCode = "400",
                Message = _message
            };

            ObjectResult objectResult = new ObjectResult(oDataError)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
