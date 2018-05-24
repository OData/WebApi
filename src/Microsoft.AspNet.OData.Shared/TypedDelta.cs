// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a <see cref="Delta"/> that can be used when a backing CLR type exists for
    /// the entity type and complex type whose changes are tracked.
    /// </summary>
    public abstract class TypedDelta : Delta
    {
        /// <summary>
        /// Gets the actual type of the entity for which the changes are tracked.
        /// </summary>
        public abstract Type EntityType { get; }

        /// <summary>
        /// Gets the expected type of the entity for which the changes are tracked.
        /// </summary>
        public abstract Type ExpectedClrType { get; }

        /// <summary>
        /// Returns the instance that holds all the changes at current level being tracked by this Delta.
        /// </summary>
        /// <returns>The internal resource instance.</returns>
        internal virtual object GetInstance()
        {
            return null;
        }

        /// <summary>
        /// Helper method to check whether the given type is Delta generic type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it is a Delta generic type; false otherwise.</returns>
        internal static bool IsDeltaOfT(Type type)
        {
            return type != null && TypeHelper.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Delta<>);
        }
    }
}
