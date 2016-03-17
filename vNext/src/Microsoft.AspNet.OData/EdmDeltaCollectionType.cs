// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Implementing IEdmCollectionType to identify collection of DeltaFeed.
    /// </summary>
    internal class EdmDeltaCollectionType : IEdmCollectionType
    {
        private IEdmTypeReference _entityTypeReference;

        internal EdmDeltaCollectionType(IEdmTypeReference entityTypeReference)
        {
            if (entityTypeReference == null)
            {
                throw Error.ArgumentNull("entityTypeReference");
            }
            _entityTypeReference = entityTypeReference;
        }

        /// <inheritdoc />
        public EdmTypeKind TypeKind
        {
            get
            {
                return EdmTypeKind.Collection;
            }
        }

        /// <inheritdoc />
        public IEdmTypeReference ElementType
        {
            get
            {
                return _entityTypeReference;
            }
        }
    }
}