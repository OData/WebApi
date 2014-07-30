// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.OData.Extensions;

namespace System.Web.OData.Formatter
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetFakeODataRouteName(this HttpRequestMessage request)
        {
            request.ODataProperties().RouteName = HttpRouteCollectionExtensions.RouteName;
        }
    }
}
