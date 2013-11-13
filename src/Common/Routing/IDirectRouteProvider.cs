// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// Defines a provider that creates a route directly to a set of action descriptors (an attribute route).
    /// </summary>
    public interface IDirectRouteProvider
    {
        /// <summary>Creates a direct route entry.</summary>
        /// <param name="context">The context to use to create the route.</param>
        /// <returns>The direct route entry.</returns>
        RouteEntry CreateRoute(DirectRouteProviderContext context);
    }
}
