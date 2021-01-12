// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmChangedObject"/>s.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmChangedObjectCollection<TStructuralType> : EdmChangedObjectCollection, ICollection<IEdmChangedObject<TStructuralType>>, IEdmObject
    {
        private IList<IEdmChangedObject<TStructuralType>> _items;

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
        public EdmChangedObjectCollection(IEdmEntityType entityType, IList<IEdmChangedObject<TStructuralType>> changedObjectList)
            : base(entityType, changedObjectList as IList<IEdmChangedObject>)
        {            
            _items = changedObjectList;
        }

        /// <inheritdoc/>
        public void Add(IEdmChangedObject<TStructuralType> item)
        {
            InsertItem(_items.Count, item);
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
            int count = _items.Count;
            RemoveItem(_items.IndexOf(item));

            return count == _items.Count + 1;
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
            if (_item == null)
            {
                throw Error.Argument("item", SRResources.ChangedObjectTypeMismatch, typeof(TStructuralType), item.GetType());
            }

            _items.Add(_item);
            base.InsertItem(index, item);
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
            base.RemoveItem(index);
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
        /// <param name="original">Original collection of the Type which needs to be updated</param>              
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public EdmChangedObjectCollection<TStructuralType> CopyChangedValues(ICollection<TStructuralType> original)
        {
            //Here we need to Find the key of the Type, then only we will be able to find from the collection that which item in the collection
            //corresponds to the item in delta list(Edmchangedobjectcoll).
            Type type = typeof(TStructuralType);
            IEnumerable<IEdmStructuralProperty> keys = EntityType.Key();

            EdmChangedObjectCollection<TStructuralType> edmChangedObjectCollection = new EdmChangedObjectCollection<TStructuralType>(EntityType);

            foreach (dynamic changedObj in _items)
            {
                DataModificationOperationKind operation = DataModificationOperationKind.Update;
                IEdmChangedObject edmChangedObject = null;
               
                //Get filtered item based on keys
                TStructuralType originalObj = GetFilteredItem(type, keys, original as IEnumerable<TStructuralType>, changedObj); ;

                try
                {
                    IEdmDeltaDeletedEntityObject deletedObj = changedObj as IEdmDeltaDeletedEntityObject;

                    if (deletedObj != null)
                    {
                        if (deletedObj.Reason == DeltaDeletedEntryReason.Deleted)
                        {
                            operation = DataModificationOperationKind.Delete;
                        }
                        else
                        {
                            operation = DataModificationOperationKind.Unlink;
                        }

                        if (originalObj != null)
                        {
                            //This case handle deletions
                            original.Remove(originalObj);
                        }
                    }
                    else
                    {
                        if (originalObj == null)
                        {
                            operation = DataModificationOperationKind.Insert;
                            //This case handle additions
                            originalObj = Activator.CreateInstance(changedObj.ExpectedClrType);
                            original.Add(originalObj);
                        }

                        //Patch for addition/update. This will call Delta<T> for each item in the collection
                        changedObj.Patch(originalObj);
                    }

                    edmChangedObject = changedObj;
                }
                catch
                {
                    HandleFailedOperation(changedObj, operation, ref edmChangedObject, originalObj);
                }

                CopyInstanceAnnotations(changedObj, edmChangedObject);

                edmChangedObjectCollection.Add(edmChangedObject);
            }

            return edmChangedObjectCollection;
        }

        private static void CopyInstanceAnnotations(dynamic changedObj, IEdmChangedObject edmChangedObject)
        {
            if (changedObj.InstanceAnnotations != null)
            {
                if (edmChangedObject.InstanceAnnotations == null)
                {
                    edmChangedObject.InstanceAnnotations = new ODataInstanceAnnotationContainer();
                }

                Dictionary<string, object> instanceAnnotations = changedObj.InstanceAnnotations.GetResourceAnnotations();

                if (instanceAnnotations != null)
                {
                    foreach (KeyValuePair<string, object> annotation in instanceAnnotations)
                    {
                        edmChangedObject.InstanceAnnotations.AddResourceAnnotation(annotation.Key, annotation.Value);
                    }
                }
            }
        }

        private void HandleFailedOperation(dynamic changedObj, DataModificationOperationKind operation, ref IEdmChangedObject edmChangedObject, TStructuralType originalObj)
        {
            switch (operation)
            {
                case DataModificationOperationKind.Update:
                    edmChangedObject = changedObj;
                    break;
                case DataModificationOperationKind.Insert:
                    {
                        Type type = typeof(EdmDeltaDeletedEntityObject<>).MakeGenericType(changedObj.ExpectedClrType);
                        EdmDeltaDeletedEntityObject edmDeletedObject  = Activator.CreateInstance(type, EntityType) as EdmDeltaDeletedEntityObject;
                        edmDeletedObject.Id = changedObj.InstanceAnnotations.GetResourceAnnotation("Core.ContentID") ?? string.Empty;
                        edmChangedObject = edmDeletedObject;
                        break;
                    }
                case DataModificationOperationKind.Delete:
                case DataModificationOperationKind.Unlink:
                    {
                        Type type = typeof(EdmDeltaEntityObject<>).MakeGenericType(typeof(TStructuralType));
                        EdmDeltaEntityObject edmDeltaEntityObject = Activator.CreateInstance(type, EntityType) as EdmDeltaEntityObject;
                        foreach(PropertyInfo property in typeof(TStructuralType).GetProperties())
                        {
                            edmDeltaEntityObject.TrySetPropertyValue(property.Name, property.GetValue(originalObj));
                        }
                        edmChangedObject = edmDeltaEntityObject;
                        break;
                    }
            }

            DataModificationExceptionType dataModificationException = new DataModificationExceptionType(operation);
            edmChangedObject.DataModificationException = new DataModificationException();
            edmChangedObject.DataModificationException.InstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationException);            
        }

        private static TStructuralType GetFilteredItem(Type type, IEnumerable<IEdmStructuralProperty> keys, IEnumerable<TStructuralType> originalList, IEdmChangedObject changedObject)
        {   
            //This logic is for filtering the object based on the set of keys,
            //There will only be very few key elements usually, mostly 1, so performance wont be impacted.
            foreach(IEdmStructuralProperty key in keys)
            {
                object obj;
                if (changedObject.TryGetPropertyValue(key.Name, out obj))
                {
                    originalList = originalList.Where(x => type.GetProperty(key.Name).GetValue(x).ToString() == obj.ToString());
                }
            }

            return originalList.SingleOrDefault();
        }

        /// <summary>
        /// Patch for EdmChangedobjectCollection, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>
        /// <param name="original">Original collection of the Type which needs to be updated</param>        
        public EdmChangedObjectCollection<TStructuralType> Patch(ICollection<TStructuralType> original)
        {
            return CopyChangedValues(original);
        }

        internal ICollection<TStructuralType> GetInstance()
        {
            ICollection<TStructuralType> collection = new List<TStructuralType>();

            foreach(dynamic item in Items)
            {
                collection.Add(item.GetInstance());
            }

            return collection;
        }
    }
}