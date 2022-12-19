//-----------------------------------------------------------------------------
// <copyright file="DeltaSetOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
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
        private IList<string> _keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaSet{TStructuralType}"/> class.
        /// </summary>
        /// <param name="keys">List of key names for the type.</param>
        public DeltaSet(IList<string> keys)
        {
            _clrType = typeof(TStructuralType);
            _keys = keys;
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, IDeltaSetItem item)
        {
            Delta<TStructuralType> deltaItem = item as Delta<TStructuralType>;

            //To ensure we dont insert null or a non related type to deltaset
            if (deltaItem == null)
            {
                throw Error.Argument("item", SRResources.ChangedObjectTypeMismatch, item.GetType(), _clrType);
            }

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>.
        /// </summary>
        /// <param name="originalCollection">Original collection of the type which needs to be updated.</param>
        /// /// <returns>DeltaSet response.</returns>
        public DeltaSet<TStructuralType> Patch(ICollection<TStructuralType> originalCollection)
        {
            ODataAPIHandler<TStructuralType> apiHandler = new DefaultODataAPIHandler<TStructuralType>(originalCollection);

            return CopyChangedValues(apiHandler);
        }

        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>.
        /// </summary>
        /// <param name="apiHandlerOfT">API Handler for the entity.</param>
        /// <returns>DeltaSet response.</returns>
        public DeltaSet<TStructuralType> Patch(ODataAPIHandler<TStructuralType> apiHandlerOfT)
        {
            return Patch(apiHandlerOfT, null);
        }

        /// <summary>
        /// Patch for DeltaSet, a collection for Delta<typeparamref name="TStructuralType"/>.
        /// </summary>
        /// <param name="apiHandlerOfT">API Handler for the entity.</param>
        /// <param name="apiHandlerFactory">API Handler Factory.</param>
        /// <returns>DeltaSet response.</returns>
        public DeltaSet<TStructuralType> Patch(ODataAPIHandler<TStructuralType> apiHandlerOfT, ODataAPIHandlerFactory apiHandlerFactory)
        {         
            Debug.Assert(apiHandlerOfT != null, "apiHandlerOfT != null");

            return CopyChangedValues(apiHandlerOfT, apiHandlerFactory);
        }

        /// <summary>
        /// Get the keys and use the keys to find the original object to patch from the collection.
        /// </summary>
        /// <param name="apiHandler">API Handler for the entity.</param>
        /// <param name="apiHandlerFactory">API Handler Factory.</param>
        /// <returns>DeltaSet response.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal DeltaSet<TStructuralType> CopyChangedValues(IODataAPIHandler apiHandler, ODataAPIHandlerFactory apiHandlerFactory = null)
        {
            //Here we are getting the keys and using the keys to find the original object 
            //to patch from the collection.

            ODataAPIHandler<TStructuralType> apiHandlerOfT = apiHandler as ODataAPIHandler<TStructuralType>;

            Debug.Assert(apiHandlerOfT != null, "apiHandlerOfT != null");

            DeltaSet<TStructuralType> deltaSet = CreateDeltaSet();

            foreach (Delta<TStructuralType> changedObj in Items)
            {
                ODataAPIHandler<TStructuralType> handler = apiHandlerOfT;
                bool hasODataId = false;

                if (apiHandlerFactory != null)
                {
                    //{
                    //    "@odata.context":"http://localhost:6285/odata/$metadata#Customers/$delta",
                    //    "value":[
                    //        {
                    //            "@odata.type":"CustomersApiEF.Models.Customer",
                    //            "Id":1,
                    //            "Name":"Customer1",
                    //            "Orders@odata.delta":[
                    //                {
                    //                    "@odata.id":"Customers(3)/Orders(1005)",
                    //                    "Quantity": 1005
                    //                },
                    //                {
                    //                    "Id": 2000,
                    //                    "Price":200,
                    //                    "Quantity":90
                    //                }
                    //            ]
                    //        }
                    //    ]
                    //}

                    // If we have a request payload above and we are handling the changed values in Orders,
                    // The apiHandlerOfT is OrdersAPIHandler with parent as Customers(1).
                    // The object with "@odata.id" will have OrdersAPIHandler with parent as Customers(3).
                    // The object with id 2000 will have OrdersAPIHandler similar to apiHandlerOfT.
                    // The codebelow ensures we use the correct handler.
                    // NOTE: Handler when we have odata.id will always be different than the parent handler.
                    // However this is true if handlers are applied correctly.

                    ODataAPIHandler<TStructuralType> odataPathApiHandler = apiHandlerFactory.GetHandler(changedObj.ODataPath) as ODataAPIHandler<TStructuralType>;

                    if (odataPathApiHandler != null && changedObj.ODataPath.Any())
                    {
                        // Using a 3rd party library to compare objects
                        // https://github.com/GregFinzer/Compare-Net-Objects/wiki/Getting-Started
                        // TODO:
                        // a) Confirm to perf regressions are caused byt his library.
                        // b) Investigate if there is a better library.

                        KellermanSoftware.CompareNetObjects.CompareLogic compare = new KellermanSoftware.CompareNetObjects.CompareLogic();
                        compare.Config.CaseSensitive = false;
                        compare.Config.ComparePrivateProperties = true;
                        compare.Config.ComparePrivateFields = true;

                        KellermanSoftware.CompareNetObjects.ComparisonResult result = compare.Compare(handler, odataPathApiHandler);

                        if (!result.AreEqual)
                        {
                            handler = odataPathApiHandler;
                            hasODataId = true;
                        }
                    }
                }

                DataModificationOperationKind operation = DataModificationOperationKind.Update;

                //Get filtered item based on keys
                TStructuralType original = null;
                string errorMessage = string.Empty;
                string getErrorMessage = string.Empty;

                try
                {
                    Dictionary<string, object> keyValues = new Dictionary<string, object>();
                    keyValues = changedObj.ODataPath.GetKeys();

                    bool containsKeyValue = false;

                    foreach (string key in _keys)
                    {
                        keyValues.TryGetValue(key, out object value);

                        if (!string.IsNullOrEmpty(value.ToString()))
                        {
                            containsKeyValue = true;
                            continue;
                        }

                    }

                    ODataAPIResponseStatus odataAPIResponseStatus;

                    if (containsKeyValue)
                    {
                        odataAPIResponseStatus = handler.TryGet(keyValues, out original, out getErrorMessage);
                    }
                    else
                    {
                        odataAPIResponseStatus = ODataAPIResponseStatus.NotFound;
                    }

                    DeltaDeletedEntityObject<TStructuralType> deletedObj = changedObj as DeltaDeletedEntityObject<TStructuralType>;

                    if (odataAPIResponseStatus == ODataAPIResponseStatus.Failure)
                    {
                        IDeltaSetItem deltaSetItem = changedObj;
                        DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
                        dataModificationExceptionType.MessageType = new MessageType { Message = getErrorMessage };

                        deltaSetItem.TransientInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationExceptionType);
                        deltaSet.Add(deltaSetItem);
                        
                        continue;
                    }

                    if (deletedObj != null)
                    {
                        operation = DataModificationOperationKind.Delete;

                        if (odataAPIResponseStatus == ODataAPIResponseStatus.NotFound)
                        {
                            // Handle Failed Operation - Delete when the object doesn't exist.
                            IDeltaSetItem deltaSetItem = changedObj;
                            DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
                            dataModificationExceptionType.MessageType = new MessageType { Message = "Object to delete not found." };

                            deltaSetItem.TransientInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationExceptionType);
                            deltaSet.Add(deltaSetItem);

                            continue;
                        }

                        // Confirm if we actually need to patch.
                        // changedObj.CopyChangedValues(original, handler, apiHandlerFactory);

                        if (handler.TryDelete(keyValues, out errorMessage) != ODataAPIResponseStatus.Success)
                        {
                            // Handle Failed Operation - Delete
                            IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, errorMessage);
                            deltaSet.Add(changedObject);

                            continue;
                        }

                        deltaSet.Add(deletedObj);
                    }
                    else
                    {
                        if (odataAPIResponseStatus == ODataAPIResponseStatus.NotFound)
                        {
                            operation = DataModificationOperationKind.Insert;

                            if (handler.TryCreate(keyValues, out original, out errorMessage) != ODataAPIResponseStatus.Success)
                            {
                                //Handle a failed Operation - create
                                IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, errorMessage);
                                deltaSet.Add(changedObject);
                                continue;
                            }                            
                        }
                        else if (odataAPIResponseStatus == ODataAPIResponseStatus.Success)
                        {
                            operation = DataModificationOperationKind.Update;

                            if (hasODataId)
                            {
                                ODataAPIResponseStatus linkResponseStatus = apiHandlerOfT.TryAddRelatedObject(original, out errorMessage);

                                if (linkResponseStatus == ODataAPIResponseStatus.Failure)
                                {
                                    IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, errorMessage);
                                    deltaSet.Add(changedObject);
                                }
                            }
                        }
                        else
                        {
                            //Handle a failed operation
                            IDeltaSetItem changedObject = HandleFailedOperation(changedObj, operation, original, getErrorMessage);
                            deltaSet.Add(changedObject);
                            continue;
                        }

                        if (hasODataId)
                        {
                            changedObj.UpdateODataIdObject(original);
                        }

                        // Patch for addition/update. This will call Delta<T> for each item in the collection.
                        // This will work in cases where we use delegates to create objects.
                        changedObj.CopyChangedValues(original, handler, apiHandlerFactory);

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

            // This handles the DataModificationException. It adds the Core.DataModificationException annotation and also copies other instance annotations.
            // The failed operation will be based on the protocol
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

            Debug.Assert(deltaSetItem != null, "deltaSetItem != null");

            return deltaSetItem;
        }

        private IDeltaSetItem CreateEntityObjectForFailedOperation(Delta<TStructuralType> changedObj, TStructuralType originalObj)
        {
            Type type = typeof(Delta<>).MakeGenericType(_clrType);

            Delta<TStructuralType> deltaObject = Activator.CreateInstance(type, _clrType, null, null, false,
                changedObj.InstanceAnnotationsPropertyInfo) as Delta<TStructuralType>;

            SetProperties(originalObj, deltaObject);

            if (deltaObject.InstanceAnnotationsPropertyInfo != null)
            {
                object instanceAnnotationValue;
                changedObj.TryGetPropertyValue(deltaObject.InstanceAnnotationsPropertyInfo.Name, out instanceAnnotationValue);
                if (instanceAnnotationValue != null)
                {
                    IODataInstanceAnnotationContainer instanceAnnotations = instanceAnnotationValue as IODataInstanceAnnotationContainer;

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

            DeltaDeletedEntityObject<TStructuralType> deletedObject = Activator.CreateInstance(type, _clrType, null, null, false,
                changedObj.InstanceAnnotationsPropertyInfo) as DeltaDeletedEntityObject<TStructuralType>;

            foreach (string property in changedObj.GetAllPropertyNames())
            {
                SetPropertyValues(changedObj, deletedObject, property);
            }

            if (changedObj.InstanceAnnotationsPropertyInfo != null)
            {
                object annotationValue;
                if (changedObj.TryGetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, out annotationValue))
                {
                    IODataInstanceAnnotationContainer instanceAnnotations = annotationValue as IODataInstanceAnnotationContainer;

                    if (instanceAnnotations != null)
                    {
                        deletedObject.TrySetPropertyValue(changedObj.InstanceAnnotationsPropertyInfo.Name, instanceAnnotations);
                    }
                }
            }

            deletedObject.TransientInstanceAnnotationContainer = changedObj.TransientInstanceAnnotationContainer;

            ValidateForDeletedEntityId(_keys, deletedObject);
            
            return deletedObject;
        }

        //This is for ODL to work to set id as empty, because if there are missing keys, id wouldnt be set and we need to set it as empty.
        // In a failed create request with no Id, we will create a DeltaDeletedEntityObject in a compensating transaction.
        private static void ValidateForDeletedEntityId(IList<string> keys, DeltaDeletedEntityObject<TStructuralType> edmDeletedObject)
        {
            bool hasNullKeys = false;
            for (int i = 0; i < keys.Count; i++)
            {
                object value;
                edmDeletedObject.TryGetPropertyValue(keys[i], out value);

                if (value == null)
                {
                    hasNullKeys = true;
                    break;
                }
            }

            if (hasNullKeys)
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
