using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{    
   
    /// <summary>
    /// Handler Class to handle users methods for create, delete and update
    /// </summary>
    public abstract class TypelessPatchMethodHandler 
    {
        /// <summary>
        /// TryCreate method to create a new object.
        /// </summary>        
        /// <param name="createdObject">The created object (Typeless)</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns></returns>
        public abstract PatchStatus TryCreate(out EdmStructuredObject createdObject, out string errorMessage);

        /// <summary>
        /// TryGet method to which pointer to TryGet method can be assigned to.  This tries to Get based on a keyvalues.
        /// </summary>
        /// <param name="keyValues">Key value pair for the entity keys</param>        
        /// <param name="originalObject">Object to return</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns></returns>
        public abstract PatchStatus TryGet(IDictionary<string, object> keyValues, out EdmStructuredObject originalObject, out string errorMessage);

        /// <summary>
        ///  TryDelete delegate to which the pointer to TryDelete Method can be assigned to, which will delete the object based on keyvalue pairs
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public abstract PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Get the PatchHandler for the nested type
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler</param>
        /// <returns></returns>
        public abstract TypelessPatchMethodHandler GetNestedPatchHandler(EdmStructuredObject parent, string navigationPropertyName);
    }

       
    internal class DefaultTypelessPatchHandler : TypelessPatchMethodHandler
    {
        IEdmEntityType entityType;
        ICollection<EdmStructuredObject> originalList;

        public DefaultTypelessPatchHandler(ICollection<EdmStructuredObject> originalList, IEdmEntityType entityType)
        {
            this.entityType = entityType;
            this.originalList = originalList?? new List<EdmStructuredObject>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out EdmStructuredObject originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = default(EdmStructuredObject);

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
        public override PatchStatus TryCreate(out EdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = default(EdmStructuredObject);
            errorMessage = string.Empty;

            try
            {
                if(originalList == null)
                {
                    originalList = new List<EdmStructuredObject>();
                }

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
                originalList.Remove(originalObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override TypelessPatchMethodHandler GetNestedPatchHandler(EdmStructuredObject parent, string navigationPropertyName)
        {
            IEdmNavigationProperty navProperty = entityType.DeclaredNavigationProperties().FirstOrDefault(x => x.Name == navigationPropertyName);

            if(navProperty == null)
            {
                return null;
            }

            IEdmEntityType nestedEntityType = navProperty.ToEntityType();

            object obj;
            if(parent.TryGetPropertyValue(navigationPropertyName, out obj))
            {
                ICollection<EdmStructuredObject> nestedList = obj as ICollection<EdmStructuredObject>;

                return new DefaultTypelessPatchHandler(nestedList, nestedEntityType);
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

            return default(EdmStructuredObject);
        }
    }
}
