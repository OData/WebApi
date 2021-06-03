using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    internal class DefaultEdmPatchMethodHandler : EdmPatchMethodHandler
    {
        IEdmEntityType entityType;
        ICollection<IEdmStructuredObject> originalList;

        public DefaultEdmPatchMethodHandler(ICollection<IEdmStructuredObject> originalList, IEdmEntityType entityType)
        {
            Contract.Assert(entityType != null);

            this.entityType = entityType;
            this.originalList = originalList?? new List<IEdmStructuredObject>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            Contract.Assert(keyValues != null);

            try
            {
                originalObject = GetFilteredItem(keyValues);

                if (originalObject == null)
                {
                    status = PatchStatus.NotFound;
                }
            }
            catch (Exception ex)
            {
                status = PatchStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override PatchStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new EdmEntityObject(entityType);
                originalList.Add(createdObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                EdmStructuredObject originalObject = GetFilteredItem(keyValues);
                
                if (originalObject != null)
                {
                    originalList.Remove(originalObject);
                }

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override EdmPatchMethodHandler GetNestedPatchHandler(IEdmStructuredObject parent, string navigationPropertyName)
        {
            IEdmNavigationProperty navProperty = entityType.NavigationProperties().FirstOrDefault(navProp => navProp.Name == navigationPropertyName);

            if(navProperty == null)
            {
                return null;
            }

            IEdmEntityType nestedEntityType = navProperty.ToEntityType();

            object obj;
            if(parent.TryGetPropertyValue(navigationPropertyName, out obj))
            {
                ICollection<IEdmStructuredObject> nestedList = obj as ICollection<IEdmStructuredObject>;

                return new DefaultEdmPatchMethodHandler(nestedList, nestedEntityType);
            }            

            return null;
        }


        private EdmStructuredObject GetFilteredItem(IDictionary<string, object> keyValues)
        {
            //This logic is for filtering the object based on the set of keys,
            //There will only be very few key elements usually, mostly 1, so performance wont be impacted.

            if(originalList == null)
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
