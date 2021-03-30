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
    /// Basic Interface for PatchMethodHAndler
    /// </summary>
    public interface IPatchMethodHandler
    {

    }

    /// <summary>
    /// Handler Class to handle users methods for create, delete and update
    /// </summary>
    public abstract class PatchMethodHandler<TStructuralType>: IPatchMethodHandler where TStructuralType : class
    {
        /// <summary>
        /// TryCreate method to create a new object.
        /// </summary>        
        /// <param name="patchObject">The Delta<typeparamref name="TStructuralType"/> object to be patched over original object. Optional to patch</param>
        /// <param name="createdObject">The created object (CLR or Typeless)</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns></returns>
        public abstract PatchStatus TryCreate(Delta<TStructuralType> patchObject, out TStructuralType createdObject, out string errorMessage);

        /// <summary>
        /// TryGet method to which pointer to TryGet method can be assigned to.  This tries to Get based on a keyvalues.
        /// </summary>
        /// <param name="keyValues">Key value pair for the entity keys</param>        
        /// <param name="originalObject">Object to return</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns></returns>
        public abstract PatchStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage);

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
        public abstract IPatchMethodHandler GetNestedPatchHandler(TStructuralType parent, string navigationPropertyName);
    }


    /// <summary>
    /// Enum for Patch Status
    /// </summary>
    public enum PatchStatus
    {
        /// <summary>
        /// Success Status
        /// </summary>
        Success,
        /// <summary>
        /// Failure Status
        /// </summary>
        Failure,
        /// <summary>
        /// Resource Not Found
        /// </summary>
        NotFound
    }

    internal class DefaultPatchHandler<TStructuralType> : PatchMethodHandler<TStructuralType> where TStructuralType :class
    {
        Type _clrType;
        ICollection<TStructuralType> originalList;

        public DefaultPatchHandler(ICollection<TStructuralType> originalList)
        {
            this._clrType = typeof(TStructuralType);
            this.originalList = originalList?? new List<TStructuralType>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = default(TStructuralType);

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
        public override PatchStatus TryCreate(Delta<TStructuralType> patchObject, out TStructuralType createdObject, out string errorMessage)
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
                TStructuralType originalObject = GetFilteredItem(keyValues);
                originalList.Remove(originalObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override IPatchMethodHandler GetNestedPatchHandler(TStructuralType parent, string navigationPropertyName)
        {
            foreach (PropertyInfo property in _clrType.GetProperties())
            {
                if (property.Name == navigationPropertyName)
                {
                    Type type = typeof(DefaultPatchHandler<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]);

                    return Activator.CreateInstance(type, property.GetValue(parent)) as IPatchMethodHandler;
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
