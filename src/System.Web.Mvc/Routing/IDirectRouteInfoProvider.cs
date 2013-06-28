// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Provides information for building a <see cref="Route"/>.
    /// </summary>
    public interface IDirectRouteInfoProvider
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

        /// <summary>
        /// Gets the set of allowed HTTP methods for that route. If the route allow any method to be used, the value is null.
        /// </summary>
        ICollection<string> Verbs { get; }
    }
}