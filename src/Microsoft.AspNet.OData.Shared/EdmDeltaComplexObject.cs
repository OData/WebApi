//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaComplexObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmChangedObject"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Entry object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaComplexObject : EdmComplexObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaComplexObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmComplexType"/> of this object.</param>
        public EdmDeltaComplexObject(IEdmComplexType edmType)
            : this(edmType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaComplexObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmComplexTypeReference"/> of this object.</param>
        public EdmDeltaComplexObject(IEdmComplexTypeReference edmType)
            : this(edmType.ComplexDefinition(), edmType.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaComplexObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmComplexType"/> of this object.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaComplexObject(IEdmComplexType edmType, bool isNullable)
            : base(edmType, isNullable)
        {
        }
    }
}
