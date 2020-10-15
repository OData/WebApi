// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
    public class EdmChangedObjectCollection<TStructuralType> : EdmChangedObjectCollection, ICollection<IEdmChangedObject<TStructuralType>>, IEdmObject
    {
        private IEdmEntityType _entityType;
        private EdmDeltaCollectionType _edmType;
        private IEdmCollectionTypeReference _edmTypeReference;
        ICollection<IEdmChangedObject<TStructuralType>> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm entity type of the collection.</param>
        public EdmChangedObjectCollection(IEdmEntityType entityType)
            : base(entityType)
        {
            Initialize(entityType);
            _items = new Collection<IEdmChangedObject<TStructuralType>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm type of the collection.</param>
        /// <param name="changedObjectList">The list that is wrapped by the new collection.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EdmChangedObjectCollection(IEdmEntityType entityType, IList<IEdmChangedObject<TStructuralType>> changedObjectList)
            : base(entityType, changedObjectList as IList<IEdmChangedObject>)
        {
            Initialize(entityType);
            _items = changedObjectList;
        }

 
        /// <inheritdoc/>
        public void Add(IEdmChangedObject<TStructuralType> item)
        {
            _items.Add(item);
        }

       
        /// <inheritdoc/>
        public bool Contains(IEdmChangedObject<TStructuralType> item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(IEdmChangedObject<TStructuralType>[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public bool Remove(IEdmChangedObject<TStructuralType> item)
        {
            return _items.Remove(item);
        }

        /// <inheritdoc/>
        IEnumerator<IEdmChangedObject<TStructuralType>> IEnumerable<IEdmChangedObject<TStructuralType>>.GetEnumerator()
        {
            return _items.GetEnumerator();
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
    }
}