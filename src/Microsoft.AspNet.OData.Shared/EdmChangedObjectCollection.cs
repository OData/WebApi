// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmChangedObject"/>s.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmChangedObjectCollection : Collection<IEdmChangedObject>, IEdmObject
    {
        private IEdmEntityType _entityType;
        private EdmDeltaCollectionType _edmType;
        private IEdmCollectionTypeReference _edmTypeReference;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm entity type of the collection.</param>
        public EdmChangedObjectCollection(IEdmEntityType entityType)
            : base(Enumerable.Empty<IEdmChangedObject>().ToList<IEdmChangedObject>())
        {
            Initialize(entityType);
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm type of the collection.</param>
        /// <param name="changedObjectList">The list that is wrapped by the new collection.</param>
        public EdmChangedObjectCollection(IEdmEntityType entityType, IList<IEdmChangedObject> changedObjectList)
            : base(changedObjectList)
        {
            Initialize(entityType);
        }
 
        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return _edmTypeReference;
        }

        private void Initialize(IEdmEntityType entityType)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            _entityType = entityType;
            _edmType = new EdmDeltaCollectionType(new EdmEntityTypeReference(_entityType, isNullable: true));
            _edmTypeReference = new EdmCollectionTypeReference(_edmType);
        }
                
        /// <summary>
        /// Get the Change Object collection
        /// </summary>
        public virtual IEnumerable ChangedObjectCollection { get; }        
    }
}