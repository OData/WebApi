// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    /// <summary>
    /// Extensions for HttpRequest.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Get the OData feature as context.
        /// </summary>
        /// <returns>The OData feature</returns>
        public static IODataFeature ODataContext(this HttpRequest request)
        {
            return request.ODataFeature();
        }

        /// <summary>
        /// Create a response
        /// </summary>
        /// <returns>The OData path</returns>
        public static HttpResponse CreateResponse(this HttpRequest request, HttpStatusCode status, object content)
        {
            HttpResponse response = request.HttpContext.Response;
            response.StatusCode = (int)status;
            //response.?? = content;
            return response;
        }
    }
}
