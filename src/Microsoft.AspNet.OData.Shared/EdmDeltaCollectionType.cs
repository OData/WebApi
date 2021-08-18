//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaCollectionType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
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
