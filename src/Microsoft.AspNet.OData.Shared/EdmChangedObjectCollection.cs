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
        private ICollection<EdmStructuredObject> originalList;

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
        /// <param name="originalCollection"></param>
        /// <returns>ChangedObjectCollection response</returns>
        public EdmChangedObjectCollection Patch(ICollection<EdmStructuredObject> originalCollection)
        {
            this.originalList = originalCollection;
            PatchHandler = new DefaultTypelessPatchHandler(originalCollection, _entityType);

            return CopyChangedValues();
        }

        /// <summary>
        /// Handler for users Create, Get and Delete Methods
        /// </summary>
        internal TypelessPatchMethodHandler PatchHandler { get; set; }

        /// <summary>
        /// Patch for EdmChangedObjectCollection, a collection for IEdmChangedObject 
        /// </summary>
        /// <returns>ChangedObjectCollection response</returns>
        public EdmChangedObjectCollection Patch(TypelessPatchMethodHandler patchHandler)
        {
            PatchHandler = patchHandler;
            return CopyChangedValues();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal EdmChangedObjectCollection CopyChangedValues()
        {
            EdmChangedObjectCollection changedObjectCollection = new EdmChangedObjectCollection(_entityType);
            List<IEdmStructuralProperty> keys = _entityType.Key().ToList();

            foreach (IEdmChangedObject changedObj in Items)
            {                
                DataModificationOperationKind operation = DataModificationOperationKind.Update;
                EdmStructuredObject originalObj = null;
                string errorMessage = string.Empty;
                string geterrorMessage = string.Empty;
                IDictionary<string, object> keyValues = GetKeyValues(keys, changedObj);

                try
                {
                    EdmStructuredObject original = null;
                    EdmDeltaDeletedEntityObject deletedObj = changedObj as EdmDeltaDeletedEntityObject;

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;

                        if (PatchHandler.TryDelete(keyValues, out errorMessage) != PatchStatus.Success)
                        {
                            //Handle Failed Operation - Delete                            
                            PatchStatus status = PatchHandler.TryGet(keyValues, out original, out geterrorMessage);
                            if (status == PatchStatus.Success)
                            {
                                IEdmChangedObject changedObject = HandleFailedOperation(deletedObj, operation, original, keys, errorMessage);
                                changedObjectCollection.Add(changedObject);
                                continue;
                            }
                        }

                        changedObjectCollection.Add(deletedObj);
                    }
                    else
                    {
                        EdmDeltaEntityObject deltaEntityObject = changedObj as EdmDeltaEntityObject;

                        PatchStatus status = PatchHandler.TryGet(keyValues, out original, out geterrorMessage);

                        if (status == PatchStatus.NotFound)
                        {
                            operation = DataModificationOperationKind.Insert;

                            if (PatchHandler.TryCreate(out original, out errorMessage) != PatchStatus.Success)
                            {
                                //Handle failed Opreataion - create
                                IEdmChangedObject changedObject = HandleFailedOperation(deltaEntityObject, operation, original, keys, errorMessage);
                                changedObjectCollection.Add(changedObject);
                                continue;
                            }
                        }
                        else if (status == PatchStatus.Success)
                        {
                            operation = DataModificationOperationKind.Update;
                        }
                        else
                        {
                            //Handle failed operation 
                            IEdmChangedObject changedObject = HandleFailedOperation(deltaEntityObject, operation, null, keys, geterrorMessage);
                            changedObjectCollection.Add(changedObject);
                            continue;
                        }                                              

                        //Patch for addition/update. 
                        PatchItem(changedObj as EdmEntityObject, original);

                        changedObjectCollection.Add(changedObj);
                    }
                }
                catch(Exception ex)
                {
                    //Handle Failed Operation
                    IEdmChangedObject changedObject = HandleFailedOperation(changedObj as EdmEntityObject, operation, originalObj, keys, ex.Message);
                    
                    Contract.Assert(changedObject != null);
                    changedObjectCollection.Add(changedObject);
                }
            }

            return changedObjectCollection;
        }

    
        private static IDictionary<string, object> GetKeyValues(List<IEdmStructuralProperty> keys, IEdmChangedObject changedObj)
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

        private void PatchItem(EdmStructuredObject changedObj, EdmStructuredObject originalObj)
        {
            foreach (string propertyName in changedObj.GetChangedPropertyNames())
            {
                object value;
                if (changedObj.TryGetPropertyValue(propertyName, out value))
                {
                    EdmChangedObjectCollection changedColl = value as EdmChangedObjectCollection;
                    if (changedColl != null)
                    {
                        TypelessPatchMethodHandler patchHandler = PatchHandler.GetNestedPatchHandler(originalObj, propertyName);
                        if (patchHandler != null) 
                        {
                            changedColl.Patch(patchHandler);
                        }
                        else
                        {
                            object obj;
                            originalObj.TryGetPropertyValue(propertyName, out obj);

                            ICollection<EdmStructuredObject> edmColl = obj as ICollection<EdmStructuredObject>;

                            changedColl.Patch(edmColl);
                        }

                        
                    }
                    else
                    {
                        originalObj.TrySetPropertyValue(propertyName, value);
                    }
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

        private IEdmChangedObject HandleFailedOperation(EdmEntityObject changedObj, DataModificationOperationKind operation, EdmStructuredObject originalObj, 
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
                    edmChangedObject = changedObj as IEdmChangedObject;
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