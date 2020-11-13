// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaDeletedEntityObject"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Entry object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaDeletedEntityObject<TStructuralType> : EdmDeltaDeletedEntityObject, IEdmDeltaDeletedEntityObject<TStructuralType>
    {        
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaDeletedEntityObject.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
            
        }
    }
}
