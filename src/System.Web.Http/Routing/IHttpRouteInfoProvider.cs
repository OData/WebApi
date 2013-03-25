// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Provides information for defining a route.
    /// </summary>
    public interface IHttpRouteInfoProvider
    {
        /// <summary>
        /// Gets the name of the route.
        /// </summary>
        string RouteName { get; }

        /// <summary>
        /// Gets the route template describing the URI pattern to match against.
        /// </summary>
        string RouteTemplate { get; }
    }
}
