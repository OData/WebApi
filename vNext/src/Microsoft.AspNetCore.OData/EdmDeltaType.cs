// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Implementing IEdmType to identify objects which are part of DeltaFeed Payload.
    /// </summary>
    internal class EdmDeltaType : IEdmType
    {
        private IEdmEntityType _entityType;
        private EdmDeltaEntityKind _deltaKind;

        internal EdmDeltaType(IEdmEntityType entityType, EdmDeltaEntityKind deltaKind)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }
            _entityType = entityType;
            _deltaKind = deltaKind;
        }

        /// <inheritdoc />
        public EdmTypeKind TypeKind
        {
            get 
            {
                return EdmTypeKind.Entity; 
            }
        }

        public IEdmEntityType EntityType
        {
            get
            {
                return _entityType;
            }
        }

        /// <summary>
        /// Returning DeltaKind of the object within DeltaFeed payload
        /// </summary>
        public EdmDeltaEntityKind DeltaKind
        {
            get
            {
                return _deltaKind;
            }
        }
    }
}
