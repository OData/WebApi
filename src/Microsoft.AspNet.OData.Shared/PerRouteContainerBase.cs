// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    internal abstract class PerRouteContainerBase : IPerRouteContainer
    {
        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        public Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// Create a root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        public IServiceProvider CreateODataRootContainer(string routeName, Action<IContainerBuilder> configureAction)
        {
            IServiceProvider rootContainer = this.CreateODataRootContainer(configureAction);
            this.SetODataRootContainer(routeName, rootContainer);

            return rootContainer;
        }

        /// <summary>
        /// Create a root container not associated with a route.
        /// </summary>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        public IServiceProvider CreateODataRootContainer(Action<IContainerBuilder> configureAction)
        {
            IContainerBuilder builder = CreateContainerBuilderWithCoreServices();

            if (configureAction != null)
            {
                configureAction(builder);
            }

            IServiceProvider rootContainer = builder.BuildContainer();
            if (rootContainer == null)
            {
                throw Error.InvalidOperation(SRResources.NullContainer);
            }

            return rootContainer;
        }

        /// <summary>
        /// Get the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        public abstract IServiceProvider GetODataRootContainer(string routeName);

        /// <summary>
        /// Get the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="rootContainer">The root container to set.</param>
        /// <remarks>Used by unit tests to insert root containers.</remarks>
        internal abstract void SetODataRootContainer(string routeName, IServiceProvider rootContainer);

        /// <summary>
        /// Create a container builder with the default OData services.
        /// </summary>
        /// <returns>An instance of <see cref="IContainerBuilder"/> to manage services.</returns>
        private IContainerBuilder CreateContainerBuilderWithCoreServices()
        {
            IContainerBuilder builder;
            if (this.BuilderFactory != null)
            {
                builder = this.BuilderFactory();
                if (builder == null)
                {
                    throw Error.InvalidOperation(SRResources.NullContainerBuilder);
                }
            }
            else
            {
                builder = new DefaultContainerBuilder();
            }

            builder.AddDefaultODataServices();
            return builder;
        }
    }
}
