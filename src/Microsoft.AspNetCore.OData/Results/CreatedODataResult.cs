﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a create operation that adds an entity to an entity set.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header. It generates a location
    /// header containing the edit link of the created entity and, if response has status code: NoContent, also
    /// generates an OData-EntityId header.</remarks>
    public class CreatedODataResult<T> : IActionResult
    {
        private readonly T _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The created entity.</param>
        public CreatedODataResult(T entity)
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
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;
            IActionResult result = GetInnerActionResult(request);
            response.Headers["Location"] = GenerateLocationHeader(request).ToString();

            // Since AddEntityId relies on the response, make sure to execute the result
            // before calling AddEntityId() to ensure the response code is set correctly.
            await result.ExecuteResultAsync(context);
            ResultHelpers.AddEntityId(response, () => GenerateEntityId(request));
            ResultHelpers.AddServiceVersion(response, () => ResultHelpers.GetVersionString(request));
        }

        // internal just for unit test.
        internal IActionResult GetInnerActionResult(HttpRequest request)
        {
            if (RequestPreferenceHelpers.RequestPrefersReturnNoContent(new WebApiRequestHeaders(request.Headers)))
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
            else
            {
                ObjectResult objectResult = new ObjectResult(_innerResult)
                {
                    StatusCode = StatusCodes.Status201Created
                };

                return objectResult;
            }
        }

        // internal just for unit test.
        internal Uri GenerateEntityId(HttpRequest request)
        {
            return ResultHelpers.GenerateODataLink(request, _innerResult, isEntityId: true);
        }

        // internal just for unit test.
        internal Uri GenerateLocationHeader(HttpRequest request)
        {
            return ResultHelpers.GenerateODataLink(request, _innerResult, isEntityId: false);
        }
    }
}
