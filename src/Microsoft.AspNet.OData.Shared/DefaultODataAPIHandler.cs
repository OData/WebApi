// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This is the default ODataAPIHandler for CLR type. This calss has default Get, Create and Update
    /// and will do these actions. This will be used when the original collection to be Patched is provided.
    /// </summary>
    /// <typeparam name="TStructuralType"></typeparam>
    internal class DefaultODataAPIHandler<TStructuralType> : ODataAPIHandler<TStructuralType> where TStructuralType :class
    {
        Type _clrType;
        ICollection<TStructuralType> originalList;

        public DefaultODataAPIHandler(ICollection<TStructuralType> originalList)
        {
            this._clrType = typeof(TStructuralType);
            this.originalList = originalList?? new List<TStructuralType>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = default(TStructuralType);

            try
            {
                originalObject = GetFilteredItem(keyValues);

                if (originalObject == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }
            }
            catch (Exception ex)
            {
                status = ODataAPIResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out TStructuralType createdObject, out string errorMessage)
        {
            createdObject = default(TStructuralType);
            errorMessage = string.Empty;

            try
            {
                if(originalList != null)
                {
                    originalList = new List<TStructuralType>();
                }

                createdObject = Activator.CreateInstance(_clrType) as TStructuralType;
                originalList.Add(createdObject);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                TStructuralType originalObject = GetFilteredItem(keyValues);
                originalList.Remove(originalObject);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override IODataAPIHandler GetNestedHandler(TStructuralType parent, string navigationPropertyName)
        {
            foreach (PropertyInfo property in _clrType.GetProperties())
            {
                if (property.Name == navigationPropertyName)
                {
                    Type type = typeof(DefaultODataAPIHandler<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]);

                    return Activator.CreateInstance(type, property.GetValue(parent)) as IODataAPIHandler;
                }
            }

            return null;
        }


        private TStructuralType GetFilteredItem(IDictionary<string, object> keyValues)
        {
            //This logic is for filtering the object based on the set of keys,
            //There will only be very few key elements usually, mostly 1, so performance wont be impacted.

            if(originalList == null || originalList.Count == 0)
            {
                return default(TStructuralType);
            }

            Dictionary<string, PropertyInfo> propertyInfos = new Dictionary<string, PropertyInfo>();

            foreach (string key in keyValues.Keys)
            {
                propertyInfos.Add(key, _clrType.GetProperty(key));
            }

            foreach (TStructuralType item in originalList)
            {
                bool isMatch = true;

                foreach (KeyValuePair<string, object> keyValue in keyValues)
                {
                    if (!Equals(propertyInfos[keyValue.Key].GetValue(item), keyValue.Value))
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
    }
}
