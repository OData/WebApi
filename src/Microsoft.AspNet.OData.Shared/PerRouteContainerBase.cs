// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A base class for for managing per-route service containers.
    /// </summary>
    public abstract class PerRouteContainerBase : IPerRouteContainer
    {
        private IDictionary<string, string> routeMapping = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        public Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="routePrefix"></param>
        public virtual void AddRoute(string routeName, string routePrefix)
        {
            routeMapping[routeName] = routePrefix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeName"></param>
        /// <returns></returns>
        public string GetRoutePrefix(string routeName)
        {
            return routeMapping[routeName];
        }
        /// <summary>
        /// Create a root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        public IServiceProvider CreateODataRootContainer(string routeName, Action<IContainerBuilder> configureAction)
        {
            IServiceProvider rootContainer = this.CreateODataRootContainer(configureAction);
            this.SetContainer(routeName, rootContainer);

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
        /// Check if the root container for a given route name exists.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>true if root container for the route name exists, false otherwise.</returns>
        public bool HasODataRootContainer(string routeName)
        {
            IServiceProvider rootContainer = this.GetContainer(routeName);
            return rootContainer != null;
        }

        /// <summary>
        /// Get the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        /// <remarks>
        /// This function will throw an exception if no container is found
        /// in order to localize the failure and provide a consistent error
        /// message. Use <see cref="HasODataRootContainer"/> to test of a container
        /// exists without throwing an exception.
        /// </remarks>
        public IServiceProvider GetODataRootContainer(string routeName)
        {
            IServiceProvider rootContainer = this.GetContainer(routeName);
            if (rootContainer == null)
            {
                if (String.IsNullOrEmpty(routeName))
                {
                    throw Error.InvalidOperation(SRResources.MissingNonODataContainer);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.MissingODataContainer, routeName);
                }
            }

            return rootContainer;
        }

        /// <summary>
        /// Set the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="rootContainer">The root container to set.</param>
        /// <remarks>Used by unit tests to insert root containers.</remarks>
        internal void SetODataRootContainer(string routeName, IServiceProvider rootContainer)
        {
            this.SetContainer(routeName, rootContainer);
        }

        /// <summary>
        /// Get the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        protected abstract IServiceProvider GetContainer(string routeName);

        /// <summary>
        /// Set the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="rootContainer">The root container to set.</param>
        protected abstract void SetContainer(string routeName, IServiceProvider rootContainer);

        /// <summary>
        /// Create a container builder with the default OData services.
        /// </summary>
        /// <returns>An instance of <see cref="IContainerBuilder"/> to manage services.</returns>
        protected IContainerBuilder CreateContainerBuilderWithCoreServices()
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

            // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
            builder.AddService(
                ServiceLifetime.Singleton,
                typeof(ODataUriResolver),
                sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

            return builder;
        }
    }
}
