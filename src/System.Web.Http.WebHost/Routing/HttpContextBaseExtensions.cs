// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.WebHost.Routing
{
    internal static class HttpContextBaseExtensions
    {
        internal static readonly string HttpRequestMessageKey = "MS_HttpRequestMessage";

        public static HttpRequestMessage GetHttpRequestMessage(this HttpContextBase context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (!context.Items.Contains(HttpRequestMessageKey))
            {
                return null;
            }

            return context.Items[HttpRequestMessageKey] as HttpRequestMessage;
        }

        public static void SetHttpRequestMessage(this HttpContextBase context, HttpRequestMessage request)
        {
            context.Items.Add(HttpRequestMessageKey, request);
        }
    }
}
