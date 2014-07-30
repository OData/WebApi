// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IEdmComplexObject"/> backed by a CLR object with a one-to-one mapping.
    /// </summary>
    internal class TypedEdmComplexObject : TypedEdmStructuredObject, IEdmComplexObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedEdmComplexObject"/> class.
        /// </summary>
        /// <param name="instance">The backing CLR instance.</param>
        /// <param name="edmType">The <see cref="IEdmComplexTypeReference"/> of this object.</param>
        public TypedEdmComplexObject(object instance, IEdmComplexTypeReference edmType)
            : base(instance, edmType)
        {
        }
    }
}
