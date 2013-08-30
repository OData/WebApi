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
        string Name { get; }

        /// <summary>
        /// Gets the route template describing the URI pattern to match against.
        /// </summary>
        string Template { get; }

        /// <summary>
        /// Gets the order of the route relative to other routes. Default value is 0.
        /// </summary>
        int Order { get; }
    }
}
