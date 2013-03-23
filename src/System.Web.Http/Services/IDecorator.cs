// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Services
{
    /// <summary>
    /// Defines a decorator that exposes the inner decorated object.
    /// </summary>
    public interface IDecorator<out T>
    {
        /// <summary>
        /// Gets the inner object.
        /// </summary>
        T Inner { get; }
    }
}
