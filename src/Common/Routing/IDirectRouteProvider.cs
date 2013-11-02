// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    internal interface IDirectRouteProvider
    {
        RouteEntry CreateRoute(DirectRouteProviderContext context);
    }
}
