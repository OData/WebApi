// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
#else
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
#endif

namespace Microsoft.AspNet.OData.Test.Extensions
{
#if NETCORE
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
#else
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Get the OData properties as context.
        /// </summary>
        /// <returns>The OData feature</returns>
        public static HttpRequestMessageProperties ODataContext(this HttpRequestMessage request)
        {
            return request.ODataProperties();
        }
    }
#endif
}
