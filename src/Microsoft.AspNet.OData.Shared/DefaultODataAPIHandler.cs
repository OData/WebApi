//-----------------------------------------------------------------------------
// <copyright file="DefaultODataAPIHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This is the default ODataAPIHandler for CLR types. This class has default get, create and update
    /// methods that are used to patch an original collection when the collection is provided.
    /// </summary>
    /// <typeparam name="TStructuralType"></typeparam>
    internal class DefaultODataAPIHandler<TStructuralType> : ODataAPIHandler<TStructuralType> where TStructuralType :class
    {
        Type clrType;
        ICollection<TStructuralType> originalList;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataAPIHandler{TStructuralType}"/> class.
        /// </summary>
        /// <param name="originalList">Original collection of the type which needs to be updated.</param>
        public DefaultODataAPIHandler(ICollection<TStructuralType> originalList)
        {
            this.clrType = typeof(TStructuralType);
            this.originalList = originalList ?? new List<TStructuralType>();
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
                if (clrType.GetConstructor(Type.EmptyTypes) != null)
                {
                    createdObject = Activator.CreateInstance(clrType) as TStructuralType;
                    originalList.Add(createdObject);

                    return ODataAPIResponseStatus.Success;
                }
                else
                {
                    errorMessage = "Type has not parameterless constructor";
                    return ODataAPIResponseStatus.Failure;
                }
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
            foreach (PropertyInfo property in clrType.GetProperties())
            {
                if (property.Name == navigationPropertyName)
                {
                    Type type = typeof(DefaultODataAPIHandler<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]);

                    return Activator.CreateInstance(type, property.GetValue(parent)) as IODataAPIHandler;
                }
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override ODataAPIResponseStatus TryAddRelatedObject(TStructuralType resource, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                originalList.Add(resource);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        /// <summary>
        /// Filter the object based on the set of keys.
        /// </summary>
        /// <param name="keyValues">Key-value pairs for the object keys.</param>
        /// <returns>The filtered object.</returns>
        /// <remarks>There will only be very few key elements usually, mostly 1, so performance wont be impacted.</remarks>
        private TStructuralType GetFilteredItem(IDictionary<string, object> keyValues)
        {
            if (originalList.Count == 0)
            {
                return default(TStructuralType);
            }

            foreach (TStructuralType item in originalList)
            {
                bool isMatch = true;
                foreach (KeyValuePair<string, object> keyValue in keyValues)
                {
                    PropertyInfo propertyInfo = clrType.GetProperty(keyValue.Key);

                    if (!Equals(propertyInfo.GetValue(item), keyValue.Value))
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
