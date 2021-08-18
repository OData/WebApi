//-----------------------------------------------------------------------------
// <copyright file="TypedEdmEntityObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
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
        /// <param name="edmModel">The <see cref="IEdmModel"/>.</param>
        public TypedEdmEntityObject(object instance, IEdmEntityTypeReference edmType, IEdmModel edmModel)
            : base(instance, edmType, edmModel)
        {
        }
    }
}
