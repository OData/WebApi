// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IDeltaSet"/> that is a collection of <see cref="IDeltaSetItem"/>s.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [NonValidatingParameterBinding]
    public class DeltaSet<TStructuralType> : Collection<IDeltaSetItem>, IDeltaSet where TStructuralType : class
    {        
        private Type _clrType;
        IList<string> _keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaSet{TStructuralType}"/> class.
        /// </summary>
        /// <param name="keys">List of key names for the type</param>
        public DeltaSet(IList<string> keys)           
        {
            _keys = keys;            
            _clrType = typeof(TStructuralType);            
        }

    
       /// <inheritdoc/>      
        protected override void InsertItem(int index, IDeltaSetItem item)
        {
            Delta<TStructuralType> deltaItem = item as Delta<TStructuralType>;

            //To ensure we dont insert null or a non related type to deltaset
            if (deltaItem == null)
            {
                throw Error.Argument("item", SRResources.ChangedObjectTypeMismatch, item.GetType(), typeof(TStructuralType));
            }

            base.InsertItem(index, item);
        }


        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>
        /// <param name="originalCollection">Original collection of the Type which needs to be updated</param>
        /// /// <returns>DeltaSet response</returns>
        public DeltaSet<TStructuralType> Patch(ICollection<TStructuralType> originalCollection)
        {
            PatchMethodHandler<TStructuralType> patchHandler = new DefaultPatchHandler<TStructuralType>(originalCollection);

            return CopyChangedValues(patchHandler);
        }

        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>     
        /// <returns>DeltaSet response</returns>
        public DeltaSet<TStructuralType> Patch(IPatchMethodHandler patchHandler)
        {            
            return CopyChangedValues(patchHandler as PatchMethodHandler<TStructuralType>);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal DeltaSet<TStructuralType> CopyChangedValues(PatchMethodHandler<TStructuralType> patchHandler)
        {
            //Here we are getting the keys and using the keys to find the original object 
            //to patch from the list of collection

            DeltaSet<TStructuralType> deltaSet = CreateDeltaSet();

            foreach (Delta<TStructuralType> changedObj in Items)
            {
                DataModificationOperationKind operation = DataModificationOperationKind.Update;

                //Get filtered item based on keys
                TStructuralType original = null;
                string errorMessage = string.Empty;
                string getErrorMessage = string.Empty;

                Dictionary<string, object> keyValues = new Dictionary<string, object>();

                foreach (string key in _keys)
                {
                    object value;

                    if (changedObj.TryGetPropertyValue(key, out value))                   
                    {
                        keyValues.Add(key, value);
                    }
                }

                try
                {
                    PatchStatus patchStatus = patchHandler.TryGet(keyValues, out original, out getErrorMessage);

                    DeltaDeletedEntityObject<TStructuralType> deletedObj = changedObj as DeltaDeletedEntityObject<TStructuralType>;

                    if (patchStatus == PatchStatus.Failure || (deletedObj != null && patchStatus == PatchStatus.NotFound))
                    {
                        IDeltaSetItem deltaSetItem = changedObj;

                        DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
                        dataModificationExceptionType.MessageType = new MessageType { Message = getErrorMessage };

                        deltaSetItem.TransientInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationExceptionType);

                        continue;
                    }

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;

                        changedObj.Patch(original, patchHandler);

                        if (patchHandler.TryDelete(keyValues, out errorMessage) != PatchStatus.Success)
                        {
                            //Handle Failed Operation - Delete                           
                            
                            if (patchStatus == PatchStatus.Success)
                            {
                                IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, errorMessage);
                                deltaSet.Add(changedObject);
                                continue;
                            }                            
                        }
                                               
                        deltaSet.Add(deletedObj);
                    }
                    else
                    {
                        if (patchStatus == PatchStatus.NotFound)
                        {
                            operation = DataModificationOperationKind.Insert;

                            if (patchHandler.TryCreate(changedObj, out original, out errorMessage) != PatchStatus.Success)
                            {
                                //Handle failed Opreataion - create
                                IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, errorMessage);
                                deltaSet.Add(changedObject);
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
                            IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, getErrorMessage);
                            deltaSet.Add(changedObject);
                            continue;
                        }
                        
                        //Patch for addition/update. This will call Delta<T> for each item in the collection
                        // This will work in case we use delegates for using users method to create an object
                        changedObj.Patch(original, patchHandler);                                                

                        deltaSet.Add(changedObj);
                    }
                }
                catch (Exception ex)
                {
                    //For handling the failed operations.
                    IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, ex.Message);                    
                    deltaSet.Add(changedObject);
                }
            }

            return deltaSet;
        }

        private DeltaSet<TStructuralType> CreateDeltaSet()
        {
            Type type = typeof(DeltaSet<>).MakeGenericType(_clrType);

            return Activator.CreateInstance(type, _keys) as DeltaSet<TStructuralType>;            
        }

        private IDeltaSetItem HandleFailedOperation(Delta<TStructuralType> changedObj, DataModificationOperationKind operation, TStructuralType originalObj, string errorMessage)
        {
            IDeltaSetItem deltaSetItem = null;
            DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
            dataModificationExceptionType.MessageType = new MessageType { Message = errorMessage };

            // This handles the Data Modification exception. This adds Core.DataModificationException annotation and also copy other instance annotations.
            //The failed operation will be based on the protocol
            switch (operation)
            {
                case DataModificationOperationKind.Update:
                    deltaSetItem = changedObj;
                    break;
                case DataModificationOperationKind.Insert:
                    {
                        deltaSetItem = CreateDeletedEntityForFailedOperation(changedObj);

                        break;
                    }
                case DataModificationOperationKind.Delete:
                    {
                        deltaSetItem = CreateEntityObjectForFailedOperation(changedObj, originalObj);                        
                        break;
                    }
            }


            deltaSetItem.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
            deltaSetItem.TransientInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationExceptionType);

            Contract.Assert(deltaSetItem != null);

            return deltaSetItem;
        }

        private IDeltaSetItem CreateEntityObjectForFailedOperation(Delta<TStructuralType> changedObj, TStructuralType originalObj)
        {
            Type type = typeof(Delta<>).MakeGenericType(_clrType);

            Delta<TStructuralType> deltaObject = Activator.CreateInstance(type, _clrType, null, null,
                changedObj.InstanceAnnotationsPropertyInfo) as Delta<TStructuralType>;

            SetProperties(originalObj, deltaObject);

            if (deltaObject.InstanceAnnotationsPropertyInfo != null)
            {
                object instAnnValue;
                changedObj.TryGetPropertyValue(deltaObject.InstanceAnnotationsPropertyInfo.Name, out instAnnValue);
                if (instAnnValue != null)
                {
                    IODataInstanceAnnotationContainer instanceAnnotations = instAnnValue as IODataInstanceAnnotationContainer;

                    if (instanceAnnotations != null)
                    {
                        deltaObject.TrySetPropertyValue(deltaObject.InstanceAnnotationsPropertyInfo.Name, instanceAnnotations);
                    }
                }
            }

            return deltaObject;
        }

        private void SetProperties(TStructuralType originalObj, Delta<TStructuralType> edmDeltaEntityObject)
        {
            foreach (string property in edmDeltaEntityObject.GetUnchangedPropertyNames())
            {
                edmDeltaEntityObject.TrySetPropertyValue(property, _clrType.GetProperty(property).GetValue(originalObj));
            }
        }

        private DeltaDeletedEntityObject<TStructuralType> CreateDeletedEntityForFailedOperation(Delta<TStructuralType> changedObj)
        {
            Type type = typeof(DeltaDeletedEntityObject<>).MakeGenericType(changedObj.ExpectedClrType);

            DeltaDeletedEntityObject<TStructuralType> deletedObject = Activator.CreateInstance(type, true, changedObj.InstanceAnnotationsPropertyInfo) as DeltaDeletedEntityObject<TStructuralType>;

            foreach (string property in changedObj.GetChangedPropertyNames())
            {
                SetPropertyValues(changedObj, deletedObject, property);
            }

            foreach (string property in changedObj.GetUnchangedPropertyNames())
            {
                SetPropertyValues(changedObj, deletedObject, property);
            }

            object annValue;
            if (changedObj.TryGetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, out annValue))            
            {
                IODataInstanceAnnotationContainer instanceAnnotations = annValue as IODataInstanceAnnotationContainer;

                if (instanceAnnotations != null)
                {
                    deletedObject.TrySetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, instanceAnnotations);
                }
            }

            deletedObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;

            ValidateForDeletedEntityId(_keys, deletedObject);
            
            return deletedObject;
        }

        //This is for ODL to work to set id as empty, because if there are missing keys, id wouldnt be set and we need to set it as empty.
        private static void ValidateForDeletedEntityId(IList<string> keys, DeltaDeletedEntityObject<TStructuralType> edmDeletedObject)
        {
            bool hasnullKeys = false;
            for (int i = 0; i < keys.Count; i++)
            {
                object value;
                edmDeletedObject.TryGetPropertyValue(keys[i], out value);

                if (value == null)
                {
                    hasnullKeys = true;
                    break;
                }
            }

            if (hasnullKeys)
            {               
                edmDeletedObject.Id = new Uri(string.Empty);                
            }
        }

        private static void SetPropertyValues(Delta<TStructuralType> changedObj, DeltaDeletedEntityObject<TStructuralType> edmDeletedObject, string property)
        {
            object objectVal;
            if (changedObj.TryGetPropertyValue(property, out objectVal))
            {
                edmDeletedObject.TrySetPropertyValue(property, objectVal);
            }
        }
    }
}