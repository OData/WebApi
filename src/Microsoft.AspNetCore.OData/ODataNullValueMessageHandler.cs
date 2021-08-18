//-----------------------------------------------------------------------------
// <copyright file="ODataNullValueMessageHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see href="IResultFilter" /> that converts null values in OData responses to
    /// HTTP NotFound responses or NoContent responses following the OData specification.
    /// </summary>
    public partial class ODataNullValueMessageHandler : IResultFilter
    {
        /// <inheritdocs/>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            // Only operate on OData requests.
            if (request.ODataFeature().Path != null)
            {
                // This message handler is intended for helping with queries that return a null value, for example in a
                // get request for a particular entity on an entity set, for a single valued navigation property or for
                // a structural property of a given entity. The only case in which a data modification request will result
                // in a 204 response status code, is when a primitive property is set to null through a PUT request to the
                // property URL and in that case, the user can return the right status code himself.
                ObjectResult objectResult = context.Result as ObjectResult;
                if (request.Method == HttpMethod.Get.ToString() && objectResult != null && objectResult.Value == null &&
                    response.StatusCode == (int)HttpStatusCode.OK)
                {
                    HttpStatusCode? newStatusCode = GetUpdatedResponseStatusCodeOrNull(request.ODataFeature().Path);
                    if (newStatusCode.HasValue)
                    {
                        response.StatusCode = (int)newStatusCode.Value;
                    }
                }
            }
        }

        /// <inheritdocs/>
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Can't add to headers here because response has already begun.
        }
    }
}
