// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaDeletedLink"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Link object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaDeletedLink : EdmEntityObject, IEdmDeltaDeletedLink
    {
        private Uri _source;
        private Uri _target;
        private string _relationship;
        private EdmDeltaType _edmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedLink.</param>
        public EdmDeltaDeletedLink(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedLink"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaDeletedLink.</param>
        public EdmDeltaDeletedLink(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedLink.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaDeletedLink(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
            _edmType = new EdmDeltaType(entityType, EdmDeltaEntityKind.DeletedLinkEntry);
        }

        /// <inheritdoc />
        public Uri Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        /// <inheritdoc />
        public Uri Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
            }
        }

        /// <inheritdoc />
        public string Relationship
        {
            get
            {
                return _relationship;
            }
            set
            {
                _relationship = value;
            }
        }

        /// <inheritdoc />
        public EdmDeltaEntityKind DeltaKind
        {
            get
            {
                Contract.Assert(_edmType != null);
                return _edmType.DeltaKind;
            }
        }
    }
}