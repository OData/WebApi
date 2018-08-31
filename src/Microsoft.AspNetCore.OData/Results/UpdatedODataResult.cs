// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Results
{
    internal interface IInnerActionResult
    {
        IActionResult GetInnerActionResult(HttpRequest request);
    }

    /// <summary>
    /// Represents an action result that is a response to a PUT, PATCH, or a MERGE operation on an OData entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header.</remarks>
    public class UpdatedODataResult<T> : IActionResult, IInnerActionResult
    {
        private readonly T _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        public UpdatedODataResult(T entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            this._innerResult = entity;
        }

        /// <inheritdoc/>
        public async virtual Task ExecuteResultAsync(ActionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            HttpRequest request = context.HttpContext.Request;
            IActionResult result = GetInnerActionResult(request);
            await result.ExecuteResultAsync(context);
            ResultHelpers.AddServiceVersion(response, () => ResultHelpers.GetVersionString(request));
        }

        /// <inheritdoc/>
        public IActionResult GetInnerActionResult(HttpRequest request)
        {
            if (RequestPreferenceHelpers.RequestPrefersReturnContent(new WebApiRequestHeaders(request.Headers)))
            {
                ObjectResult objectResult = new ObjectResult(_innerResult)
                {
                    StatusCode = StatusCodes.Status200OK
                };

                return objectResult;
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }
    }
}
