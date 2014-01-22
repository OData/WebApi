// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData
{
    /// <summary>
    /// Represents a <see cref="Delta"/> that can be used when a backing CLR type exists for 
    /// the entity type whose changes are tracked.
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
    }
}
