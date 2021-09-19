//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaEntityObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmChangedObject"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Entry object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaEntityObject : EdmEntityObject, IEdmChangedObject
    {
        private EdmDeltaType _edmType;
        private IEdmNavigationSource _navigationSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaEntityObject.</param>
        public EdmDeltaEntityObject(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaEntityObject"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaEntityObject.</param>
        public EdmDeltaEntityObject(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaEntityObject.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaEntityObject(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
            _edmType = new EdmDeltaType(entityType, EdmDeltaEntityKind.Entry);
        }

        /// <inheritdoc />
        public override EdmDeltaEntityKind DeltaKind
        {
            get
            {
                return EdmDeltaEntityKind.Entry;
            }
        }

        /// <summary>
        /// The navigation source of the entity. If null, then the entity is from the current feed.
        /// </summary>
        public IEdmNavigationSource NavigationSource
        {
            get
            {
                return _navigationSource;
            }
            set
            {
                _navigationSource = value;
            }
        }
    }
}
