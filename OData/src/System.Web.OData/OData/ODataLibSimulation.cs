// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

// TODO: This file should be REMOVED after changing reference to ODataLib v7.0.
namespace Microsoft.OData
{
    /// <summary>
    /// Enumerates all kinds of lifetime of a service in an <see cref="IContainerBuilder"/>.
    /// </summary>
    internal enum ServiceLifetime
    {
        /// <summary>
        /// Indicates that a single instance of the service will be created.
        /// </summary>
        Singleton,

        /// <summary>
        /// Indicates that a new instance of the service will be created for each scope.
        /// </summary>
        Scoped,

        /// <summary>
        /// Indicates that a new instance of the service will be created every time it is requested.
        /// </summary>
        Transient
    }

    /// <summary>
    /// An interface that decouples ODataLib from any implementation of dependency injection container.
    /// </summary>
    internal interface IContainerBuilder
    {
        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        IContainerBuilder AddService(
            ServiceLifetime lifetime,
            Type serviceType,
            Type implementationType);

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationFactory"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        IContainerBuilder AddService(
            ServiceLifetime lifetime,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory);

        /// <summary>
        /// Builds a container which implements <see cref="IServiceProvider"/> and contains
        /// all the services registered.
        /// </summary>
        /// <returns>The container built by this builder.</returns>
        IServiceProvider BuildContainer();
    }

    internal class ServicePrototype<TService>
    {
        public ServicePrototype(TService instance)
        {
            Debug.Assert(instance != null, "instance != null");

            this.Instance = instance;
        }

        public TService Instance { get; private set; }
    }

    /// <summary>
    /// Extension methods for <see cref="IContainerBuilder"/>.
    /// </summary>
    internal static class ContainerBuilderExtensions
    {
        #region Overloads for IContainerBuilder.AddService

        /// <summary>
        /// Adds a service of <typeparamref name="TService"/> with an <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="builder">The <see cref="IContainerBuilder"/> to add the service to.</param>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public static IContainerBuilder AddService<TService, TImplementation>(
            this IContainerBuilder builder,
            ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            Debug.Assert(builder != null, "builder != null");

            return builder.AddService(lifetime, typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IContainerBuilder"/> to add the service to.</param>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public static IContainerBuilder AddService(
            this IContainerBuilder builder,
            ServiceLifetime lifetime,
            Type serviceType)
        {
            Debug.Assert(builder != null, "builder != null");
            Debug.Assert(serviceType != null, "serviceType != null");

            return builder.AddService(lifetime, serviceType, serviceType);
        }

        /// <summary>
        /// Adds a service of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="builder">The <see cref="IContainerBuilder"/> to add the service to.</param>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public static IContainerBuilder AddService<TService>(
            this IContainerBuilder builder,
            ServiceLifetime lifetime)
            where TService : class
        {
            Debug.Assert(builder != null, "builder != null");

            return builder.AddService(lifetime, typeof(TService));
        }

        /// <summary>
        /// Adds a service of <typeparamref name="TService"/> with an <paramref name="implementationFactory"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="builder">The <see cref="IContainerBuilder"/> to add the service to.</param>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public static IContainerBuilder AddService<TService>(
            this IContainerBuilder builder,
            ServiceLifetime lifetime,
            Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            Debug.Assert(builder != null, "builder != null");
            Debug.Assert(implementationFactory != null, "implementationFactory != null");

            return builder.AddService(lifetime, typeof(TService), implementationFactory);
        }

        #endregion

        /// <summary>
        /// Adds a service prototype of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service prototype to add.</typeparam>
        /// <param name="builder">The <see cref="IContainerBuilder"/> to add the service to.</param>
        /// <param name="instance">The service prototype to add.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public static IContainerBuilder AddServicePrototype<TService>(
            this IContainerBuilder builder,
            TService instance)
        {
            Debug.Assert(builder != null, "builder != null");

            return builder.AddService(ServiceLifetime.Singleton, sp => new ServicePrototype<TService>(instance));
        }

        /// <summary>
        /// Adds the default OData services to the <see cref="IContainerBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IContainerBuilder"/> to add the services to.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public static IContainerBuilder AddDefaultODataServices(this IContainerBuilder builder)
        {
            Debug.Assert(builder != null, "builder != null");

            // ODL code to add some default OData services.
            return builder;
        }
    }
}
