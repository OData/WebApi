// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.Test.AspNet.OData.Formatter
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetFakeODataRouteName(this HttpRequestMessage request)
        {
            request.ODataProperties().RouteName = HttpRouteCollectionExtensions.RouteName;
        }
    }
}
