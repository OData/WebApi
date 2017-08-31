// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace System.Web.OData
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