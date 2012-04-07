// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.Dependencies
{
    /// <summary>
    /// Represents a scope that is tracked by the dependency injection container. The scope is
    /// used to keep track of resources that have been provided, so that they can then be
    /// subsequently released when <see cref="IDisposable.Dispose"/> is called.
    /// </summary>
    public interface IDependencyScope : IDisposable
    {
        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>. Must never throw.
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>The requested object, if found; <c>null</c> otherwise.</returns>
        object GetService(Type serviceType);

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>. Must never throw.
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>A sequence of isntances of the requested <paramref name="serviceType"/>. The sequence
        /// should be empty (not null) if no objects of the given type are available.</returns>
        IEnumerable<object> GetServices(Type serviceType);
    }
}
