// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An interface for managing per-route service containers.
    /// </summary>
    public interface IPerRouteContainer
    {
        /// <summary>
        /// Add a routing mapping
        /// </summary>
        /// <param name="routeName">The route name</param>
        /// <param name="routePrefix">The route prefix</param>
        void AddRoute(string routeName, string routePrefix);

        /// <summary>
        /// Get the route prefix
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The route prefix.</returns>
        string GetRoutePrefix(string routeName);

        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// Create a root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        IServiceProvider CreateODataRootContainer(string routeName, Action<IContainerBuilder> configureAction);

        /// <summary>
        /// Check if the root container for a given route name exists.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>true if root container for the route name exists, false otherwise.</returns>
        bool HasODataRootContainer(string routeName);

        /// <summary>
        /// Get the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        IServiceProvider GetODataRootContainer(string routeName);
    }
}
