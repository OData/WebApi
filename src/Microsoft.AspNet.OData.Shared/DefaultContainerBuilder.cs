//-----------------------------------------------------------------------------
// <copyright file="DefaultContainerBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// The default container builder implementation based on the Microsoft dependency injection framework.
    /// </summary>
    public class DefaultContainerBuilder : IContainerBuilder
    {
        private readonly IServiceCollection services = new ServiceCollection();

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public virtual IContainerBuilder AddService(
            Microsoft.OData.ServiceLifetime lifetime,
            Type serviceType,
            Type implementationType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (implementationType == null)
            {
                throw Error.ArgumentNull("implementationType");
            }

            services.Add(new ServiceDescriptor(
                serviceType, implementationType, TranslateServiceLifetime(lifetime)));

            return this;
        }

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationFactory"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>The <see cref="Microsoft.OData.IContainerBuilder"/> instance itself.</returns>
        public Microsoft.OData.IContainerBuilder AddService(
            Microsoft.OData.ServiceLifetime lifetime,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (implementationFactory == null)
            {
                throw Error.ArgumentNull("implementationFactory");
            }

            services.Add(new ServiceDescriptor(
                serviceType, implementationFactory, TranslateServiceLifetime(lifetime)));

            return this;
        }

        /// <summary>
        /// Builds a container which implements <see cref="IServiceProvider"/> and contains
        /// all the services registered.
        /// </summary>
        /// <returns>The container built by this builder.</returns>
        public virtual IServiceProvider BuildContainer()
        {
            try
            {
                // On .NET Framework platform, if using Microsoft.Extensions.DependencyInjection > 6.x version,
                // The runtime throws exception directly when calling BuildContainer if we call 'services.BuildServiceProvider()' within it.
                // So, wrap 'services.BuildServiceProvider()' into a private method call, we can catch the runtime exception directly				
                return BuildServiceProviderImpl();
            }
            catch
            {
                MethodInfo buildServiceProviderMethod =
                    typeof(ServiceCollectionContainerBuilderExtensions)
                    .GetMethod(nameof(ServiceCollectionContainerBuilderExtensions.BuildServiceProvider), new[] { typeof(IServiceCollection) });

                return (IServiceProvider)buildServiceProviderMethod.Invoke(null, new object[] { services });
            }
        }

        private IServiceProvider BuildServiceProviderImpl()
        {
            return services.BuildServiceProvider();
        }

        private static Microsoft.Extensions.DependencyInjection.ServiceLifetime TranslateServiceLifetime(
            Microsoft.OData.ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
            case Microsoft.OData.ServiceLifetime.Scoped:
                return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped;
            case Microsoft.OData.ServiceLifetime.Singleton:
                return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton;
            default:
                return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient;
            }
        }
    }
}
