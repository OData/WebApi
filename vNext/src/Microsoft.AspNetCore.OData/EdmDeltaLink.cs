// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaLink"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Added/Modified Link object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaLink : EdmEntityObject, IEdmDeltaLink
    {
        private Uri _source;
        private Uri _target;
        private string _relationship;
        private EdmDeltaType _edmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaLink.</param>
        public EdmDeltaLink(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaLink"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaLink.</param>
        public EdmDeltaLink(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaLink.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaLink(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
            _edmType = new EdmDeltaType(entityType, EdmDeltaEntityKind.LinkEntry);
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