using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{    
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
