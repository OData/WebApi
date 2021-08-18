//-----------------------------------------------------------------------------
// <copyright file="ETagMessageHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Defines a <see cref="ActionFilterAttribute"/> to add an ETag header value to an OData response when the response
    /// is a single resource that has an ETag defined.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public partial class ETagMessageHandler : ActionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            // Need a value to operate on.
            ObjectResult result = actionExecutedContext.Result as ObjectResult;
            if (result == null)
            {
                return;
            }

            HttpResponse response = actionExecutedContext.HttpContext.Response;
            HttpRequest request = actionExecutedContext.HttpContext.Request;

            EntityTagHeaderValue etag = GetETag(
                response?.StatusCode,
                request.ODataFeature().Path,
                request.GetModel(),
                result.Value,
                request.GetETagHandler());

            if (etag != null)
            {
                response.Headers["ETag"] = etag.ToString();
            }
        }
    }
}
