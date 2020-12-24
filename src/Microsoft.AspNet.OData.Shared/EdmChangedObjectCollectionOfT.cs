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
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmChangedObject"/>s.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmChangedObjectCollection<TStructuralType> : EdmChangedObjectCollection, ICollection<IEdmChangedObject<TStructuralType>>, IEdmObject
    {
        private IList<IEdmChangedObject<TStructuralType>> _items;
        private Type _clrType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm entity type of the collection.</param>
        public EdmChangedObjectCollection(IEdmEntityType entityType)
            : base(entityType)
        {
            _items = new Collection<IEdmChangedObject<TStructuralType>>();
            _clrType = typeof(TStructuralType);
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
            _clrType = typeof(TStructuralType);
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
                throw Error.Argument("item", SRResources.ChangedObjectTypeMismatch, _clrType, item.GetType());
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
        internal EdmChangedObjectCollection CopyChangedValues(ICollection<TStructuralType> original)
        {
            //Here we are getting the keys and using the keys to find the original object 
            //to patch from the list of collection.

            Contract.Assert(EntityType.Key() != null);

            List<IEdmStructuralProperty> keys = EntityType.Key().ToList();
            EdmChangedObjectCollection edmChangedObjectCollection = new EdmChangedObjectCollection(EntityType);
           
            foreach (dynamic changedObj in _items)
            {
                DataModificationOperationKind operation = DataModificationOperationKind.Update;                

                //Get filtered item based on keys
                TStructuralType originalObj = GetFilteredItem(_clrType, keys, original, changedObj);

                try
                {
                    IEdmDeltaDeletedEntityObject deletedObj = changedObj as IEdmDeltaDeletedEntityObject;

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;
                     
                        if (originalObj != null)
                        {
                            //This case handle deletions
                            original.Remove(originalObj);
                        }

                        edmChangedObjectCollection.Add(deletedObj);
                    }
                    else
                    {
                        if (originalObj == null)
                        {
                            operation = DataModificationOperationKind.Insert;
                            //This case handle additions
                            originalObj = Activator.CreateInstance(changedObj.ExpectedClrType);
                            changedObj.Patch(originalObj);
                            original.Add(originalObj);
                        }
                        else
                        {
                            //Patch for addition/update. This will call Delta<T> for each item in the collection
                            changedObj.Patch(originalObj);
                        }

                        edmChangedObjectCollection.Add(changedObj);
                    }                    
                }
                catch
                {
                    //For handling the failed operations.
                    IEdmChangedObject changedObject = HandleFailedOperation(changedObj, operation, originalObj, keys);

                    Contract.Assert(changedObject != null);
                    edmChangedObjectCollection.Add(changedObject);
                }                
            }

            return edmChangedObjectCollection;
        }

       
        private IEdmChangedObject HandleFailedOperation(dynamic changedObj, DataModificationOperationKind operation, TStructuralType originalObj,
            List<IEdmStructuralProperty> keys)
        {
            IEdmChangedObject edmChangedObject = null;
            DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);

            // This handles the Data Modification exception. This adds Core.DataModificationException annotation and also copy other instance annotations.
            //The failed operation will be based on the protocol
            switch (operation)
            {
                case DataModificationOperationKind.Update:
                    edmChangedObject = changedObj;
                    break;
                case DataModificationOperationKind.Insert:
                    {
                        EdmDeltaDeletedEntityObject edmDeletedObject = CreateDeletedEntityForFailedOperation(changedObj, keys, dataModificationExceptionType);
                        edmChangedObject = edmDeletedObject;
                        break;
                    }
                case DataModificationOperationKind.Delete:
                    {
                        dynamic edmDeltaEntityObject = CreateEntityObjectforFailedOperation(changedObj, originalObj, dataModificationExceptionType);
                        edmChangedObject = edmDeltaEntityObject;
                        break;
                    }
            }

            return edmChangedObject;
        }

        private dynamic CreateEntityObjectforFailedOperation(dynamic changedObj, TStructuralType originalObj, DataModificationExceptionType dataModificationExceptionType)
        {
            Type type = typeof(Delta<>).MakeGenericType(_clrType);
            IEdmStructuredTypeReference structuredType = EntityType.ToEdmTypeReference(true) as IEdmStructuredTypeReference;

            dynamic edmDeltaEntityObject = Activator.CreateInstance(type, _clrType, structuredType.StructuralProperties().Select(x => x.Name), null,
                structuredType, changedObj.InstanceAnnotationsPropertyInfo);

            SetProperties(originalObj, edmDeltaEntityObject);

            IODataInstanceAnnotationContainer instanceAnnotations = changedObj.TryGetInstanceAnnotations();

            if (instanceAnnotations != null)
            {
                edmDeltaEntityObject.TrySetPropertyValue(edmDeltaEntityObject.InstanceAnnotationsPropertyInfo.Name, instanceAnnotations);
            }

            edmDeltaEntityObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
            edmDeltaEntityObject.TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationExceptionType);
            return edmDeltaEntityObject;
        }

        private void SetProperties(TStructuralType originalObj, dynamic edmDeltaEntityObject)
        {
            foreach (string property in edmDeltaEntityObject.GetChangedPropertyNames())
            {
                edmDeltaEntityObject.TrySetPropertyValue(property, _clrType.GetProperty(property).GetValue(originalObj));
            }

            foreach (string property in edmDeltaEntityObject.GetUnchangedPropertyNames())
            {
                edmDeltaEntityObject.TrySetPropertyValue(property, _clrType.GetProperty(property).GetValue(originalObj));
            }
        }

        private EdmDeltaDeletedEntityObject CreateDeletedEntityForFailedOperation(dynamic changedObj, List<IEdmStructuralProperty> keys, DataModificationExceptionType dataModificationExceptionType)
        {
            Type type = typeof(EdmDeltaDeletedEntityObject<>).MakeGenericType(changedObj.ExpectedClrType);
            
            EdmDeltaDeletedEntityObject edmDeletedObject = Activator.CreateInstance(type, EntityType, true, changedObj.InstanceAnnotationsPropertyInfo) as EdmDeltaDeletedEntityObject;

            foreach (string property in changedObj.GetChangedPropertyNames())
            {
                SetPropertyValues(changedObj, edmDeletedObject, property);
            }

            foreach (string property in changedObj.GetUnchangedPropertyNames())
            {
                SetPropertyValues(changedObj, edmDeletedObject, property);
            }

            object annValue;
            changedObj.TryGetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, out annValue);

            IODataInstanceAnnotationContainer instanceAnnotations = annValue as IODataInstanceAnnotationContainer;

            if (instanceAnnotations != null)
            {
                edmDeletedObject.TrySetInstanceAnnotations(instanceAnnotations);
            }

            edmDeletedObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;

            TryGetContentId(changedObj, keys, edmDeletedObject);

            //CopyInstanceAnnotations(changedObj, edmDeletedObject);
            edmDeletedObject.TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationExceptionType);
            return edmDeletedObject;
        }

        private static void TryGetContentId(dynamic changedObj, List<IEdmStructuralProperty> keys, EdmDeltaDeletedEntityObject edmDeletedObject)
        {
            bool takeContentId = false;
            for (int i = 0; i < keys.Count; i++)
            {
                object value;
                edmDeletedObject.TryGetPropertyValue(keys[i].Name, out value);

                if (value == null)
                {
                    takeContentId = true;
                    break;
                }
            }

            if (takeContentId)
            {
                object contentId = changedObj.TransientInstanceAnnotationContainer.GetResourceAnnotation("Core.ContentID");
                if (contentId != null)
                {
                    edmDeletedObject.Id = contentId.ToString();
                }
                else
                {
                    edmDeletedObject.Id = string.Empty;
                }
            }
        }

        private static void SetPropertyValues(dynamic changedObj, EdmDeltaDeletedEntityObject edmDeletedObject, string property)
        {
            object objectVal;
            if (changedObj.TryGetPropertyValue(property, out objectVal))
            {
                edmDeletedObject.TrySetPropertyValue(property, objectVal);
            }
        }

        private static void CopyInstanceAnnotations(dynamic changedObj, dynamic destinationObject)
        {
            //This is for copying both Persistent and Transient Instance Annotations.
            EdmEntityObject entityObject = changedObj as EdmEntityObject;
            // cast to delta if delta take that instance anno . 

            if (entityObject != null)
            {
                destinationObject.PersistentInstanceAnnotationsContainer = entityObject.PersistentInstanceAnnotationsContainer;
                destinationObject.TransientInstanceAnnotationContainer = entityObject.TransientInstanceAnnotationContainer; 
            }           
        }

        private static TStructuralType GetFilteredItem(Type type, List<IEdmStructuralProperty> keys, IEnumerable<TStructuralType> originalList, IEdmChangedObject changedObject)
        {
            //This logic is for filtering the object based on the set of keys,
            //There will only be very few key elements usually, mostly 1, so performance wont be impacted.

            object keyValue;
            object[] keyValues = new object[keys.Count];
            PropertyInfo[] propertyInfos = new PropertyInfo[keys.Count];
            for (int i = 0; i < keys.Count; i++)
            {
                string keyName = keys[i].Name;
                changedObject.TryGetPropertyValue(keyName, out keyValue);
                keyValues[i] = keyValue;
                propertyInfos[i] = type.GetProperty(keyName);
            }
                        
            foreach (TStructuralType item in originalList)
            {
                bool isMatch = true;

                for (int i = 0; i < keyValues.Length; i++)
                {
                    if (!Equals(propertyInfos[i].GetValue(item), keyValues[i]))
                    {
                        // Not a match, so try the next one
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    return item;
                }                
            }

            return default(TStructuralType);
        }

        /// <summary>
        /// Patch for EdmChangedobjectCollection, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>
        /// <param name="original">Original collection of the Type which needs to be updated</param>        
        public EdmChangedObjectCollection Patch(ICollection<TStructuralType> original)
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