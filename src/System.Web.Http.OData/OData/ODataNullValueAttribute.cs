// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Represents an <see href="IActionFilter" /> that converts null values in OData $value responses to HTTP 404 responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ODataNullValueAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            HttpRequestMessage request = actionExecutedContext.Request;
            HttpResponseMessage response = actionExecutedContext.Response;
            if (IsRawValueRequest(request) && response.IsSuccessStatusCode)
            {
                ObjectContent content = response.Content as ObjectContent;
                if (content != null && content.Value == null)
                {
                    actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
        }

        private static bool IsRawValueRequest(HttpRequestMessage request)
        {
            ODataPath path = request.ODataProperties().Path;
            return path != null && path.Segments.LastOrDefault() is ValuePathSegment;
        }
    }
}
