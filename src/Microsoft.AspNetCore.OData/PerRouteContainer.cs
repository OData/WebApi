// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    internal class PerRouteContainer : PerRouteContainerBase
    {
        private ConcurrentDictionary<string, IServiceProvider> _perRouteContainers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        public PerRouteContainer()
        {
            this._perRouteContainers = new ConcurrentDictionary<string, IServiceProvider>();
        }

        /// <summary>
        /// Gets the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        public override IServiceProvider GetODataRootContainer(string routeName)
        {
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.ArgumentNull("routeName");
            }

            IServiceProvider rootContainer;
            if (_perRouteContainers.TryGetValue(routeName, out rootContainer))
            {
                return rootContainer;
            }

            throw Error.InvalidOperation(SRResources.NullContainer);
        }

        /// <summary>
        /// Sets the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="rootContainer">The root container to set.</param>
        /// <returns>The root container for the route name.</returns>
        /// <remarks>Used by unit tests to insert root containers.</remarks>
        internal override void SetODataRootContainer(string routeName, IServiceProvider rootContainer)
        {
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.ArgumentNull("routeName");
            }

            if (rootContainer == null)
            {
                throw Error.InvalidOperation(SRResources.NullContainer);
            }

            this._perRouteContainers.AddOrUpdate(routeName, rootContainer, (k, v) => rootContainer);
        }
    }
}
