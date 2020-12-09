// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmChangedObject"/>s.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmChangedObjectCollection<TStructuralType> : EdmChangedObjectCollection, ICollection<IEdmChangedObject<TStructuralType>>, IEdmObject
    {
        private Collection<IEdmChangedObject<TStructuralType>> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm entity type of the collection.</param>
        public EdmChangedObjectCollection(IEdmEntityType entityType)
            : base(entityType)
        {
            _items = new Collection<IEdmChangedObject<TStructuralType>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm type of the collection.</param>
        /// <param name="changedObjectList">The list that is wrapped by the new collection.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EdmChangedObjectCollection(IEdmEntityType entityType, Collection<IEdmChangedObject<TStructuralType>> changedObjectList)
            : base(entityType, changedObjectList as IList<IEdmChangedObject>)
        {            
            _items = changedObjectList;
        }

        /// <inheritdoc/>
        public override IEnumerable ChangedObjectCollection { get { return _items; } }

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

        /// <inheritdoc/>
        protected override void InsertItem(int index, IEdmChangedObject item)
        {
            IEdmChangedObject<TStructuralType> _item = item as IEdmChangedObject<TStructuralType>;
            Contract.Assert(_item != null);

            _items.Add(_item);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            _items.Clear();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            Contract.Assert(_items.Count > index);
            
            _items.RemoveAt(index);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, IEdmChangedObject item)
        {
            IEdmChangedObject<TStructuralType> _item = item as IEdmChangedObject<TStructuralType>;
            Contract.Assert(_item != null);

            _items[index] = _item;
        }

        /// <summary>
        /// Copy changed values is an implementation of Patch
        /// </summary>
        /// <param name="original"></param>              
        public void CopyChangedValues(ICollection<TStructuralType> original)
        {
            //Here we need to Find the key of the Type, then only we will be able to find from the collection that which item in the collection
            //corresponds to the item in delta list(Edmchangedobjectcoll). For this we use somewhat the same logic used in 
            //the method private static PropertyConfiguration GetKeyProperty(EntityTypeConfiguration entityType) in EntityKeyConvention class
            //Once we find the key we use that to pick the corresponding item from the original collection (by comparing the value of the key, eg: Id)
            Type type = original.First().GetType();
            string key = GetKeyProperty(type.GetProperties(), type.Name);

            foreach (dynamic changedObj in _items)
            {
                object Id;
                IEdmDeltaDeletedEntityObject deletedObj = changedObj as IEdmDeltaDeletedEntityObject;

                if(deletedObj != null)
                {
                    TStructuralType originalObj = original.FirstOrDefault(x => x.GetType().GetProperty(key).GetValue(x).ToString() == deletedObj.Id);

                    if (originalObj != null)
                    {
                        //This case handle deletions
                        original.Remove(originalObj);
                    }
                }
                else
                {                 
                    changedObj.TryGetPropertyValue(key, out Id);
                    TStructuralType originalObj = original.FirstOrDefault(x => x.GetType().GetProperty(key).GetValue(x).ToString() == Id.ToString());

                    if (originalObj == null)
                    {
                        //This case handle additions
                        originalObj = Activator.CreateInstance(changedObj.ExpectedClrType);
                        original.Add(originalObj);
                    }

                    //Patch for addition/update. This will call Delta<T> for each item in the collection
                    changedObj.Patch(originalObj);
                }               
            }
        }

        /// <summary>
        /// Patch for EdmChangedobjectCollection, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>
        /// <param name="original"></param>        
        public void Patch(ICollection<TStructuralType> original)
        {
            CopyChangedValues(original);
        }

        private static string GetKeyProperty(PropertyInfo[] allProperties, string entityName)
        {
            var keys =
               allProperties
               .Where(p => (p.Name.Equals(entityName + "Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
               && (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(p.PropertyType) != null || TypeHelper.IsEnum(p.PropertyType)));

            if (keys.Count() == 1)
            {
                return keys.Single().Name;
            }

            return null;
        }
    }
}