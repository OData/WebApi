//-----------------------------------------------------------------------------
// <copyright file="PerRouteContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Web.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A class for managing per-route service containers.
    /// </summary>
    public class PerRouteContainer : PerRouteContainerBase
    {
        private const string RootContainerMappingsKey = "Microsoft.AspNet.OData.RootContainerMappingsKey";

        private readonly HttpConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        public PerRouteContainer(HttpConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        protected override IServiceProvider GetContainer(string routeName)
        {
            if (String.IsNullOrEmpty(routeName))
            {
                return configuration.GetNonODataRootContainer();
            }

            IServiceProvider rootContainer;
            if (GetRootContainerMappings().TryGetValue(routeName, out rootContainer))
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
        protected override void SetContainer(string routeName, IServiceProvider rootContainer)
        {
            if (rootContainer == null)
            {
                throw Error.InvalidOperation(SRResources.NullContainer);
            }

            if (String.IsNullOrEmpty(routeName))
            {
                configuration.SetNonODataRootContainer(rootContainer);
            }
            else
            {
                this.GetRootContainerMappings()[routeName] = rootContainer;
            }
        }

        /// <summary>
        /// Gets the root container mappings from the configuration.
        /// </summary>
        /// <returns>The root container mappings from the configuration.</returns>
        private ConcurrentDictionary<string, IServiceProvider> GetRootContainerMappings()
        {
            return (ConcurrentDictionary<string, IServiceProvider>)configuration.Properties.GetOrAdd(
                RootContainerMappingsKey, key => new ConcurrentDictionary<string, IServiceProvider>());
        }
    }
}
