//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
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
