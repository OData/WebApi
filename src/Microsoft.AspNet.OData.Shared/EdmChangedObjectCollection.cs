//-----------------------------------------------------------------------------
// <copyright file="EdmChangedObjectCollection.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        public IEdmEntityType EntityType { get { return _entityType; } }

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
        internal EdmChangedObjectCollection Patch(ICollection<IEdmStructuredObject> originalCollection)
        {
            EdmPatchMethodHandler patchHandler = new DefaultEdmPatchMethodHandler(originalCollection, _entityType);

            return CopyChangedValues(patchHandler);
        }

        /// <summary>
        /// Patch for EdmChangedObjectCollection, a collection for IEdmChangedObject 
        /// </summary>
        /// <returns>ChangedObjectCollection response</returns>
        public EdmChangedObjectCollection Patch(EdmPatchMethodHandler patchHandler)
        {            
            return CopyChangedValues(patchHandler);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal EdmChangedObjectCollection CopyChangedValues(EdmPatchMethodHandler patchHandler)
        {
            EdmChangedObjectCollection changedObjectCollection = new EdmChangedObjectCollection(_entityType);
            IEdmStructuralProperty[] keys = _entityType.Key().ToArray();

            foreach (IEdmChangedObject changedObj in Items)
            {                
                DataModificationOperationKind operation = DataModificationOperationKind.Update;
                EdmStructuredObject originalObj = null;
                string errorMessage = string.Empty;
                string getErrorMessage = string.Empty;
                IDictionary<string, object> keyValues = GetKeyValues(keys, changedObj);

                try
                {
                    IEdmStructuredObject original = null;
                    EdmDeltaDeletedEntityObject deletedObj = changedObj as EdmDeltaDeletedEntityObject;

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;

                        if (patchHandler.TryDelete(keyValues, out errorMessage) != PatchStatus.Success)
                        {
                            //Handle Failed Operation - Delete                            
                            PatchStatus patchStatus = patchHandler.TryGet(keyValues, out original, out getErrorMessage);
                            if (patchStatus == PatchStatus.Success)
                            {
                                IEdmChangedObject changedObject = HandleFailedOperation(deletedObj, operation, original, keys, errorMessage, patchHandler);
                                changedObjectCollection.Add(changedObject);
                                continue;
                            }
                        }

                        changedObjectCollection.Add(deletedObj);
                    }
                    else
                    {
                        EdmEntityObject deltaEntityObject = changedObj as EdmEntityObject;

                        PatchStatus patchStatus = patchHandler.TryGet(keyValues, out original, out getErrorMessage);

                        if (patchStatus == PatchStatus.NotFound)
                        {
                            operation = DataModificationOperationKind.Insert;

                            if (patchHandler.TryCreate(changedObj, out original, out errorMessage) != PatchStatus.Success)
                            {
                                //Handle failed Opreataion - create
                                IEdmChangedObject changedObject = HandleFailedOperation(deltaEntityObject, operation, original, keys, errorMessage, patchHandler);
                                changedObjectCollection.Add(changedObject);
                                continue;
                            }
                        }
                        else if (patchStatus == PatchStatus.Success)
                        {
                            operation = DataModificationOperationKind.Update;
                        }
                        else
                        {
                            //Handle failed operation 
                            IEdmChangedObject changedObject = HandleFailedOperation(deltaEntityObject, operation, null, keys, getErrorMessage, patchHandler);
                            changedObjectCollection.Add(changedObject);
                            continue;
                        }                                              

                        //Patch for addition/update. 
                        PatchItem(deltaEntityObject, original as EdmStructuredObject, patchHandler);

                        changedObjectCollection.Add(changedObj);
                    }
                }
                catch (Exception ex)
                {
                    //Handle Failed Operation
                    IEdmChangedObject changedObject = HandleFailedOperation(changedObj as EdmEntityObject, operation, originalObj, keys, ex.Message, patchHandler);
                    
                    Contract.Assert(changedObject != null);
                    changedObjectCollection.Add(changedObject);
                }
            }

            return changedObjectCollection;
        }
            
        private static IDictionary<string, object> GetKeyValues(IEdmStructuralProperty [] keys, IEdmChangedObject changedObj)
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

        private void PatchItem(EdmStructuredObject changedObj, EdmStructuredObject originalObj, EdmPatchMethodHandler patchHandler)
        {
            foreach (string propertyName in changedObj.GetChangedPropertyNames())
            {
                ApplyProperties(changedObj, originalObj, propertyName, patchHandler);
            }

            foreach (string propertyName in changedObj.GetUnchangedPropertyNames())
            {
                ApplyProperties(changedObj, originalObj, propertyName, patchHandler);
            }
        }

        private void ApplyProperties(EdmStructuredObject changedObj, EdmStructuredObject originalObj, string propertyName, EdmPatchMethodHandler patchHandler)
        {
            object value;
            if (changedObj.TryGetPropertyValue(propertyName, out value))
            {
                EdmChangedObjectCollection changedColl = value as EdmChangedObjectCollection;
                if (changedColl != null)
                {
                    EdmPatchMethodHandler patchHandlerNested = patchHandler.GetNestedPatchHandler(originalObj, propertyName);
                    if (patchHandlerNested != null)
                    {
                        changedColl.Patch(patchHandlerNested);
                    }
                    else
                    {
                        object obj;
                        originalObj.TryGetPropertyValue(propertyName, out obj);

                        ICollection<IEdmStructuredObject> edmColl = obj as ICollection<IEdmStructuredObject>;

                        changedColl.Patch(edmColl);
                    }
                }
                else
                {
                    //call patchitem if its single structuredobj
                    EdmStructuredObject structuredObj = value as EdmStructuredObject;

                    if (structuredObj != null)
                    {
                        object obj;
                        originalObj.TryGetPropertyValue(propertyName, out obj);

                        EdmStructuredObject origStructuredObj = obj as EdmStructuredObject;

                        if(origStructuredObj == null)
                        {
                            if(structuredObj is EdmComplexObject)
                            {
                                origStructuredObj = new EdmComplexObject(structuredObj.ActualEdmType as IEdmComplexType);
                            }
                            else
                            {
                                origStructuredObj = new EdmEntityObject(structuredObj.ActualEdmType as IEdmEntityType);
                            }

                            originalObj.TrySetPropertyValue(propertyName, origStructuredObj);
                        }

                        PatchItem(structuredObj, origStructuredObj, patchHandler);                        
                    }
                    else
                    {
                        originalObj.TrySetPropertyValue(propertyName, value);
                    }
                }
            }
        }

        private IEdmChangedObject HandleFailedOperation(EdmEntityObject changedObj, DataModificationOperationKind operation, IEdmStructuredObject originalObj, 
            IEdmStructuralProperty[] keys, string errorMessage, EdmPatchMethodHandler patchHandler)
        {
            IEdmChangedObject edmChangedObject = null;
            DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
            dataModificationExceptionType.MessageType = new MessageType { Message = errorMessage };

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
                        PatchItem(edmDeletedObject, changedObj, patchHandler);

                        ValidateForDeletedEntityId(keys, edmDeletedObject);

                        edmDeletedObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
                        edmDeletedObject.PersistentInstanceAnnotationsContainer = changedObj.PersistentInstanceAnnotationsContainer;

                        edmDeletedObject.AddDataException(dataModificationExceptionType);
                        edmChangedObject = edmDeletedObject;
                        break;
                    }
                case DataModificationOperationKind.Delete:                
                    {
                        EdmDeltaEntityObject edmEntityObject = new EdmDeltaEntityObject(EntityType);
                        PatchItem(originalObj as EdmStructuredObject, edmEntityObject, patchHandler);

                        edmEntityObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
                        edmEntityObject.PersistentInstanceAnnotationsContainer = changedObj.PersistentInstanceAnnotationsContainer;

                        edmEntityObject.AddDataException( dataModificationExceptionType);
                        edmChangedObject = edmEntityObject;
                        break;
                    }
            }

            return edmChangedObject;
        }

        //This is for ODL to work to set id as empty, because if there are missing keys, id wouldnt be set and we need to set it as empty.
        private static void ValidateForDeletedEntityId(IEdmStructuralProperty[] keys, EdmDeltaDeletedEntityObject edmDeletedObject)
        {
            bool hasNullKeys = false;
            for (int i = 0; i < keys.Length; i++)
            {
                object value;
                if (edmDeletedObject.TryGetPropertyValue(keys[i].Name, out value))                
                {
                    hasNullKeys = true;
                    break;
                }
            }

            if (hasNullKeys)
            {               
                edmDeletedObject.Id = string.Empty;                
            }
        }
    }
}
