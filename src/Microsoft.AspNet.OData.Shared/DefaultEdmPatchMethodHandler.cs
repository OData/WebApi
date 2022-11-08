//-----------------------------------------------------------------------------
// <copyright file="DefaultEdmODataAPIHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This is the default patch handler for non-CLR types. This class has default get, create and update
    /// methods that are used to patch an original collection when the collection is provided.
    /// </summary>
    internal class DefaultEdmODataAPIHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;
        ICollection<IEdmStructuredObject> originalList;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEdmODataAPIHandler"/> class.
        /// </summary>
        /// <param name="originalList">Original collection of the type which needs to be updated.</param>
        /// <param name="entityType">The Edm entity type of the collection.</param>
        public DefaultEdmODataAPIHandler(ICollection<IEdmStructuredObject> originalList, IEdmEntityType entityType)
        {
            Debug.Assert(entityType != null, "entityType != null");

            this.entityType = entityType;
            this.originalList = originalList ?? new List<IEdmStructuredObject>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            Debug.Assert(keyValues != null, "keyValues != null");

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
        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out IEdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new EdmEntityObject(entityType);
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
                EdmStructuredObject originalObject = GetFilteredItem(keyValues);
                
                if (originalObject != null)
                {
                    originalList.Remove(originalObject);
                }

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName)
        {
            IEdmNavigationProperty navProperty = entityType.NavigationProperties().FirstOrDefault(navProp => navProp.Name == navigationPropertyName);

            if (navProperty == null)
            {
                return null;
            }

            IEdmEntityType nestedEntityType = navProperty.ToEntityType();

            object obj;

            if (parent.TryGetPropertyValue(navigationPropertyName, out obj))
            {
                ICollection<IEdmStructuredObject> nestedList = obj as ICollection<IEdmStructuredObject>;

                return new DefaultEdmODataAPIHandler(nestedList, nestedEntityType);
            }            

            return null;
        }

        private EdmStructuredObject GetFilteredItem(IDictionary<string, object> keyValues)
        {
            //This logic is for filtering the object based on the set of keys,
            //There will only be very few key elements usually, mostly 1, so performance wont be impacted.

            if (originalList == null)
            {
                return null;
            }

            foreach (EdmStructuredObject item in originalList)
            {
                bool isMatch = true;

                foreach (KeyValuePair<string, object> keyValue in keyValues)
                {
                    object value;
                    if (item.TryGetPropertyValue(keyValue.Key, out value))
                    {
                        if (!Equals(value, keyValue.Value))
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

            return null;
        }
    }
}
