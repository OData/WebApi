// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmEntityObject"/>s.
    /// </summary>
    public class EdmEntityObjectCollection : Collection<IEdmEntityObject>, IEdmObject
    {
        private IEdmCollectionTypeReference _edmType;

        /// <summary>
        /// Initialzes a new instance of the <see cref="EdmEntityObjectCollection"/> class.
        /// </summary>
        /// <param name="edmType">The edm type of the collection.</param>
        public EdmEntityObjectCollection(IEdmCollectionTypeReference edmType)
        {
            Initialize(edmType);
        }

        /// <summary>
        /// Initialzes a new instance of the <see cref="EdmEntityObjectCollection"/> class.
        /// </summary>
        /// <param name="edmType">The edm type of the collection.</param>
        /// <param name="list">The list that is wrapped by the new collection.</param>
        public EdmEntityObjectCollection(IEdmCollectionTypeReference edmType, IList<IEdmEntityObject> list)
            : base(list)
        {
            Initialize(edmType);
        }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return _edmType;
        }

        private void Initialize(IEdmCollectionTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            if (!edmType.ElementType().IsEntity())
            {
                throw Error.Argument("edmType",
                    SRResources.UnexpectedElementType, edmType.ElementType().ToTraceString(), edmType.ToTraceString(), typeof(IEdmEntityType).Name);
            }

            _edmType = edmType;
        }
    }
}
