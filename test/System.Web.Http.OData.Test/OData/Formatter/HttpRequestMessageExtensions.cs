// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.OData.Formatter
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetFakeODataRouteName(this HttpRequestMessage request)
        {
            request.SetODataRouteName(HttpRouteCollectionExtensions.RouteName);
        }
    }
}
