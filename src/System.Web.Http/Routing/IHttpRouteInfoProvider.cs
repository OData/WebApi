// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Provides information for defining a route.
    /// </summary>
    public interface IHttpRouteInfoProvider
    {
        /// <summary>
        /// Gets the name of the route to generate.
        /// </summary>
        string RouteName { get; }

        /// <summary>
        /// Gets the route template describing the URI pattern to match against.
        /// </summary>
        string RouteTemplate { get; }

        /// <summary>
        /// Gets the order of the route relative to other routes.
        /// </summary>
        int RouteOrder { get; }
    }
}
