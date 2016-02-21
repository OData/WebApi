// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmComplexObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmComplexObject : EdmStructuredObject, IEdmComplexObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmStructuredType"/> of this object.</param>
        public EdmComplexObject(IEdmComplexType edmType)
            : this(edmType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmComplexTypeReference"/> of this object.</param>
        public EdmComplexObject(IEdmComplexTypeReference edmType)
            : this(edmType.ComplexDefinition(), edmType.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmComplexType"/> of this object.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmComplexObject(IEdmComplexType edmType, bool isNullable)
            : base(edmType, isNullable)
        {
        }
    }
}
