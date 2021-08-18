//-----------------------------------------------------------------------------
// <copyright file="TypedEdmComplexObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
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
        /// <param name="edmModel">The <see cref="IEdmModel"/>.</param>
        public TypedEdmComplexObject(object instance, IEdmComplexTypeReference edmType, IEdmModel edmModel)
            : base(instance, edmType, edmModel)
        {
        }
    }
}
