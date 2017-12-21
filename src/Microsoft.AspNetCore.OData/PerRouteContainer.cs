// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Serialization;

namespace Microsoft.AspNet.OData
{
    internal class PerRouteContainer : PerRouteContainerBase
    {
        private ConcurrentDictionary<string, IServiceProvider> _perRouteContainers;
        private IServiceProvider _nonODataRouteContainer;

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
                return _nonODataRouteContainer;
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
        /// <remarks>Used by unit tests to insert root containers.</remarks>
        internal override void SetODataRootContainer(string routeName, IServiceProvider rootContainer)
        {
            if (rootContainer == null)
            {
                throw Error.InvalidOperation(SRResources.NullContainer);
            }

            if (String.IsNullOrEmpty(routeName))
            {
                _nonODataRouteContainer = rootContainer;
            }
            else
            {
                this._perRouteContainers.AddOrUpdate(routeName, rootContainer, (k, v) => rootContainer);
            }
        }
    }
}
