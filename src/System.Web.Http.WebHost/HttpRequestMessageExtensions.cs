// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.WebHost
{
    internal static class HttpRequestMessageExtensions
    {
        internal static readonly string HttpContextBaseKey = "MS_HttpContext";

        public static HttpContextBase GetHttpContext(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            HttpContextBase context;

            if (!request.Properties.TryGetValue(HttpContextBaseKey, out context))
            {
                context = null;
            }

            return context;
        }

        public static void SetHttpContext(this HttpRequestMessage request, HttpContextBase context)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            request.Properties[HttpContextBaseKey] = context;
        }
    }
}