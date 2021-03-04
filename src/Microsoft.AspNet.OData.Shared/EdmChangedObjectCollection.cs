// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Org.OData.Core.V1;
using static Microsoft.AspNet.OData.PatchMethodHandler;

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
 
        /// <summary>
        /// Represents EntityType of the changedobject
        /// </summary>
        protected IEdmEntityType EntityType { get { return _entityType; } }

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
        /// Patch for Types without underlying CLR types
        /// </summary>
        /// <param name="original"></param>
        /// <returns>ChangedObjectCollection response</returns>
        public EdmChangedObjectCollection Patch(ICollection<EdmStructuredObject> original)
        {
            return CopyChangedValues(original);
        }

        /// <summary>
        /// Patch for EdmChangedObjectCollection, a collection for IEdmChangedObject 
        /// </summary>
        /// <param name="createDelegate">Delegate for using users GetOrCreate nmethod</param>
        /// <param name="deleteDelegate">Delegate for using users Delete method</param>
        /// <returns>ChangedObjectCollection response</returns>
        public EdmChangedObjectCollection Patch(GetOrCreate createDelegate, Delete deleteDelegate)
        {
            return CopyChangedValues(null, createDelegate, deleteDelegate, true);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal EdmChangedObjectCollection CopyChangedValues(ICollection<EdmStructuredObject> original,
            GetOrCreate createDelegate = null, Delete deleteDelegate = null, bool useOriginalList = false)
        {
            EdmChangedObjectCollection changedObjectCollection = new EdmChangedObjectCollection(_entityType);
            List<IEdmStructuralProperty> keys = _entityType.Key().ToList();

            foreach (dynamic changedObj in Items)
            {                
                DataModificationOperationKind operation = DataModificationOperationKind.Update;
                EdmStructuredObject originalObj = null;

                if (useOriginalList)
                {
                    Dictionary<string, object> keyValues = GetKeyValues(keys, changedObj);

                    originalObj = createDelegate(keyValues) as EdmStructuredObject;

                    Contract.Assert(originalObj != null);

                }
                else
                {
                    originalObj = GetFilteredItem(keys, original, changedObj);
                }

                try
                {
                    IEdmDeltaDeletedEntityObject deletedObj = changedObj as IEdmDeltaDeletedEntityObject;

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;

                        if (useOriginalList)
                        {
                            deleteDelegate(originalObj);
                        }
                        else
                        {
                            if (originalObj != null)
                            {
                                //This case handle deletions
                                original.Remove(originalObj);
                            }
                        }

                        changedObjectCollection.Add(deletedObj);
                    }
                    else
                    {
                        if (originalObj == null)
                        {
                            operation = DataModificationOperationKind.Insert;
                            //This case handle additions
                            originalObj = new EdmEntityObject(changedObj.ActualEdmType as IEdmEntityType);
                            PatchItem(changedObj, originalObj);
                            original.Add(originalObj);
                        }
                        else
                        {
                            //Patch for addition/update. This will call Delta<T> for each item in the collection
                            PatchItem(changedObj, originalObj);
                        }

                        changedObjectCollection.Add(changedObj);
                    }
                }
                catch(Exception ex)
                {
                    //Handle Failed Operation
                    IEdmChangedObject changedObject = HandleFailedOperation(changedObj, operation, originalObj, keys, ex.Message);
                    
                    Contract.Assert(changedObject != null);
                    changedObjectCollection.Add(changedObject);
                }
            }

            return changedObjectCollection;
        }

        private static IDictionary<string, object> GetKeyValues(List<IEdmStructuralProperty> keys, dynamic changedObj)
        {
            IDictionary<string, object> keyValues = new Dictionary<string, object>();

            foreach (IEdmStructuralProperty key in keys)
            {
                object value;
                changedObj.TryGetPropertyValue(key.Name, out value);

                if (value != null)
                {
                    keyValues.Add(key.Name, value);
                }
            }

            return keyValues;
        }

        private static void PatchItem(EdmStructuredObject changedObj, EdmStructuredObject originalObj)
        {
            foreach (string propertyName in changedObj.GetChangedPropertyNames())
            {
                object value;
                if (changedObj.TryGetPropertyValue(propertyName, out value))
                {
                    originalObj.TrySetPropertyValue(propertyName, value);
                }
            }

            foreach (string propertyName in changedObj.GetUnchangedPropertyNames())
            {
                object value;
                if (changedObj.TryGetPropertyValue(propertyName, out value))
                {
                    originalObj.TrySetPropertyValue(propertyName, value);
                }
            }
        }

        private static EdmStructuredObject GetFilteredItem(List<IEdmStructuralProperty> keys, IEnumerable<EdmStructuredObject> originalList, dynamic changedObject)
        {
            //This logic is for filtering the object based on the set of keys,
            //There will only be very few key elements usually, mostly 1, so performance wont be impacted.

            object keyValue;
            object[] keyValues = new object[keys.Count];
            
            for (int i = 0; i < keys.Count; i++)
            {
                string keyName = keys[i].Name;
                changedObject.TryGetPropertyValue(keyName, out keyValue);
                keyValues[i] = keyValue;                
            }

            foreach (EdmStructuredObject item in originalList)
            {
                bool isMatch = true;

                for (int i = 0; i < keyValues.Length; i++)
                {
                    object itemValue;
                    if (item.TryGetPropertyValue(keys[i].Name, out itemValue))
                    {
                        if (!Equals(itemValue, keyValues[i]))
                        {
                            // Not a match, so try the next one
                            isMatch = false;
                            break;
                        }
                    }
                }

                if (isMatch)
                {
                    return item;
                }
            }

            return default(EdmStructuredObject);
        }

        private IEdmChangedObject HandleFailedOperation(dynamic changedObj, DataModificationOperationKind operation, EdmStructuredObject originalObj, 
            List<IEdmStructuralProperty> keys, string errorMessage)
        {
            IEdmChangedObject edmChangedObject = null;
            DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
            dataModificationExceptionType.MessageType = new MessageType();
            dataModificationExceptionType.MessageType.Message = errorMessage;

            // This handles the Data Modification exception. This adds Core.DataModificationException annotation and also copy other instance annotations.
            //The failed operation will be based on the protocol
            switch (operation)
            {
                case DataModificationOperationKind.Update:
                    edmChangedObject = changedObj;
                    break;
                case DataModificationOperationKind.Insert:
                    {
                        EdmDeltaDeletedEntityObject edmDeletedObject = new EdmDeltaDeletedEntityObject(EntityType);
                        PatchItem(edmDeletedObject, changedObj);

                        TryGetContentId(changedObj, keys, edmDeletedObject);

                        edmDeletedObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
                        edmDeletedObject.PersistentInstanceAnnotationsContainer = changedObj.PersistentInstanceAnnotationsContainer;

                        edmDeletedObject.TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationExceptionType);
                        edmChangedObject = edmDeletedObject;
                        break;
                    }
                case DataModificationOperationKind.Delete:
                case DataModificationOperationKind.Unlink:
                    {
                        EdmDeltaEntityObject edmEntityObject = new EdmDeltaEntityObject(EntityType);
                        PatchItem(originalObj, edmEntityObject);

                        edmEntityObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
                        edmEntityObject.PersistentInstanceAnnotationsContainer = changedObj.PersistentInstanceAnnotationsContainer;

                        edmEntityObject.TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationExceptionType);
                        edmChangedObject = edmEntityObject;
                        break;
                    }
            }

            return edmChangedObject;
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
    }
}