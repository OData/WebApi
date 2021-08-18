//-----------------------------------------------------------------------------
// <copyright file="PerRouteContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Serialization;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A class for managing per-route service containers.
    /// </summary>
    public class PerRouteContainer : PerRouteContainerBase
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
        /// Gets the container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        protected override IServiceProvider GetContainer(string routeName)
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

            return null;
        }

        /// <summary>
        /// Sets the container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="rootContainer">The root container to set.</param>
        /// <remarks>Used by unit tests to insert root containers.</remarks>
        protected override void SetContainer(string routeName, IServiceProvider rootContainer)
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
