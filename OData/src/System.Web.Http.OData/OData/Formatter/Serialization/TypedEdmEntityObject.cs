// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IEdmEntityObject"/> backed by a CLR object with a one-to-one mapping.
    /// </summary>
    internal class TypedEdmEntityObject : TypedEdmStructuredObject, IEdmEntityObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedEdmEntityObject"/> class.
        /// </summary>
        /// <param name="instance">The backing CLR instance.</param>
        /// <param name="edmType">The <see cref="IEdmEntityTypeReference"/> of this object.</param>
        public TypedEdmEntityObject(object instance, IEdmEntityTypeReference edmType)
            : base(instance, edmType)
        {
        }
    }
}