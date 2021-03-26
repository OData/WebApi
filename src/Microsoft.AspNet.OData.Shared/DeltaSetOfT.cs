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
               
        internal IDictionary<string, IDictionary<string, object>> dictKeyValues;
        internal string currentPathValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaSet{TStructuralType}"/> class.
        /// </summary>
        public DeltaSet()            
        {            
            _clrType = typeof(TStructuralType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaSet{TStructuralType}"/> class.
        /// </summary>
        /// <param name="keys">List of key names for the type</param>
        public DeltaSet(IList<string> keys)           
        {
            _keys = keys;            
            _clrType = typeof(TStructuralType);            
        }



        /// <summary>
        /// Handler for users Create, Get and Delete Methods
        /// </summary>
        internal PatchMethodHandler<TStructuralType> PatchHandler { get; set; }


        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>
        /// <param name="originalCollection">Original collection of the Type which needs to be updated</param>
        /// /// <returns>DeltaSet response</returns>
        public DeltaSet<TStructuralType> Patch(ICollection<TStructuralType> originalCollection)
        {
            PatchHandler = new DefaultPatchHandler<TStructuralType>(originalCollection);

            return CopyChangedValues();
        }

        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>     
        /// <returns>DeltaSet response</returns>
        public DeltaSet<TStructuralType> Patch(IPatchMethodHandler patchHandler)
        {
            this.PatchHandler = patchHandler as PatchMethodHandler<TStructuralType>;
            return CopyChangedValues();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal DeltaSet<TStructuralType> CopyChangedValues()
        {
            //Here we are getting the keys and using the keys to find the original object 
            //to patch from the list of collection

            DeltaSet<TStructuralType> deltaSet = CreateDetlaSet();

            foreach (Delta<TStructuralType> changedObj in Items)
            {
                DataModificationOperationKind operation = DataModificationOperationKind.Update;

                //Get filtered item based on keys
                TStructuralType originalObj = null;
                string errorMessage = string.Empty;

                Dictionary<string, object> keyValues = new Dictionary<string, object>();

                foreach (string key in _keys)
                {
                    object value;
                    changedObj.TryGetPropertyValue(key, out value);

                    if (value != null)
                    {
                        keyValues.Add(key, value);
                    }
                }

                try
                {
                    TStructuralType original = null;                    
                    DeltaDeletedEntityObject<TStructuralType> deletedObj = changedObj as DeltaDeletedEntityObject<TStructuralType>;

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;
                        
                        if(PatchHandler.TryDelete(keyValues, out errorMessage) != PatchStatus.Success)
                        {
                            //Handle Failed Operation - Delete
                           
                            PatchStatus status = PatchHandler.TryGet(keyValues, out original, out errorMessage);
                            if(status == PatchStatus.Success)
                            {
                                IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original as TStructuralType, errorMessage);
                                deltaSet.Add(changedObject);
                                continue;
                            }                            
                        }
                                               
                        deltaSet.Add(deletedObj);
                    }
                    else
                    {                        
                        PatchStatus status = PatchHandler.TryGet(keyValues, out original, out errorMessage);

                        if (status == PatchStatus.NotFound)
                        {
                            operation = DataModificationOperationKind.Insert;

                            if(PatchHandler.TryCreate(out original, out errorMessage) != PatchStatus.Success)
                            {
                                //Handle failed Opreataion - create
                                IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, originalObj, errorMessage);
                                deltaSet.Add(changedObject);
                                continue;
                            }                            
                        }
                        else if(status == PatchStatus.Success)
                        {
                            operation = DataModificationOperationKind.Update;
                        }
                        else
                        {
                            //Handle failed operation 
                            IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, originalObj, errorMessage);;
                            deltaSet.Add(changedObject);
                            continue;
                        }

                        originalObj = original as TStructuralType;

                        //Patch for addition/update. This will call Delta<T> for each item in the collection
                        // This will work in case we use delegates for using users method to create an object
                        changedObj.Patch(originalObj, PatchHandler);                                                

                        deltaSet.Add(changedObj);
                    }
                }
                catch(Exception ex)
                {
                    //For handling the failed operations.
                    IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, originalObj, ex.Message);                    
                    deltaSet.Add(changedObject);
                }
            }

            return deltaSet;
        }



        private DeltaSet<TStructuralType> CreateDetlaSet()
        {
            Type type = typeof(DeltaSet<>).MakeGenericType(_clrType);

            DeltaSet<TStructuralType> deltaSet = Activator.CreateInstance(type, _keys) as DeltaSet<TStructuralType>;
            return deltaSet;
        }

        private IDeltaSetItem HandleFailedOperation(Delta<TStructuralType> changedObj, DataModificationOperationKind operation, TStructuralType originalObj, string errorMessage)
        {
            IDeltaSetItem deltaSetItem = null;
            DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
            dataModificationExceptionType.MessageType = new MessageType();
            dataModificationExceptionType.MessageType.Message = errorMessage;

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
                        deltaSetItem = CreateEntityObjectforFailedOperation(changedObj, originalObj);                        
                        break;
                    }
            }


            deltaSetItem.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;
            deltaSetItem.TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationExceptionType);

            Contract.Assert(deltaSetItem != null);

            return deltaSetItem;
        }

        private IDeltaSetItem CreateEntityObjectforFailedOperation(Delta<TStructuralType> changedObj, TStructuralType originalObj)
        {
            Type type = typeof(Delta<>).MakeGenericType(_clrType);

            Delta<TStructuralType> deltaObject = Activator.CreateInstance(type, _clrType, _clrType.GetProperties().Select(x=>x.Name), null,
                changedObj.InstanceAnnotationsPropertyInfo) as Delta<TStructuralType>;

            SetProperties(originalObj, deltaObject);

            if (deltaObject.InstanceAnnotationsPropertyInfo != null) {

                object instAnnValue;
                changedObj.TryGetPropertyValue(deltaObject.InstanceAnnotationsPropertyInfo.Name, out instAnnValue);
                IODataInstanceAnnotationContainer instanceAnnotations = instAnnValue as IODataInstanceAnnotationContainer;

                if (instanceAnnotations != null)
                {
                    deltaObject.TrySetPropertyValue(deltaObject.InstanceAnnotationsPropertyInfo.Name, instanceAnnotations);
                }
            }

            return deltaObject;
        }

        private void SetProperties(TStructuralType originalObj, Delta<TStructuralType> edmDeltaEntityObject)
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
            changedObj.TryGetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, out annValue);

            IODataInstanceAnnotationContainer instanceAnnotations = annValue as IODataInstanceAnnotationContainer;

            if (instanceAnnotations != null)
            {
                deletedObject.TrySetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, instanceAnnotations);
            }

            deletedObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;

            TryGetContentId(changedObj, _keys, deletedObject);
            
            return deletedObject;
        }

        private static void TryGetContentId(Delta<TStructuralType> changedObj, IList<string> keys, DeltaDeletedEntityObject<TStructuralType> edmDeletedObject)
        {
            bool takeContentId = false;
            for (int i = 0; i < keys.Count; i++)
            {
                object value;
                edmDeletedObject.TryGetPropertyValue(keys[i], out value);

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

        private static void SetPropertyValues(Delta<TStructuralType> changedObj, DeltaDeletedEntityObject<TStructuralType> edmDeletedObject, string property)
        {
            object objectVal;
            if (changedObj.TryGetPropertyValue(property, out objectVal))
            {
                edmDeletedObject.TrySetPropertyValue(property, objectVal);
            }
        }

        internal ICollection<TStructuralType> GetInstance()
        {
            ICollection<TStructuralType> collection = new List<TStructuralType>();

            foreach(Delta<TStructuralType> item in Items)
            {
                collection.Add(item.GetInstance());
            }

            return collection;
        }
    }
}