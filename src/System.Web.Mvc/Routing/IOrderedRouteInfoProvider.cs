// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Provides information for building a <see cref="Route"/>.
    /// </summary>
    public interface IOrderedRouteInfoProvider : IRouteInfoProvider
    {
        /// <summary>
        /// Gets the order of the route relative to other routes. Default value is 0.
        /// </summary>
        int Order { get; }
    }
}