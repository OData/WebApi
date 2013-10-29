// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    internal interface IDirectRouteProvider
    {
        HttpRouteEntry CreateRoute(DirectRouteProviderContext context);
    }
}
