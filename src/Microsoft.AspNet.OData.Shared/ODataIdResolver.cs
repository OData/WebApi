//-----------------------------------------------------------------------------
// <copyright file="ODataIDResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Abstract class for Resolving ODataId
    /// </summary>
    public abstract class ODataIDResolver
    {
        /// <summary>
        /// Apply OdataId for objects with OdataID container
        /// </summary>
        /// <param name="obj">object to apply odata id on</param>
        public virtual void ApplyODataId(object obj)
        {
            if(obj != null)
            {
                CheckAndApplyODataId(obj);
            }
        }

        /// <summary>
        /// Abstract method to Get an object based on KeyValues
        /// </summary>
        /// <param name="name">Name of the object to get</param>
        /// <param name="parent">Parent of the object, if any</param>
        /// <param name="keyValues">KeyValues dictionary</param>
        /// <returns></returns>
        public abstract object GetObject(string name, object parent, Dictionary<string, object> keyValues);

        private void CheckAndApplyODataId(object obj)
        {
            Type type = obj.GetType();

            PropertyInfo property = type.GetProperties().FirstOrDefault(s => s.PropertyType == typeof(ODataIdContainer));

            if (property != null && property.GetValue(obj) is ODataIdContainer container && container != null)
            {
                object res = ApplyODataIdOnContainer(container);

                foreach (PropertyInfo prop in type.GetProperties())
                {
                    object resVal = prop.GetValue(res);

                    if (resVal != null)
                    {
                        prop.SetValue(obj, resVal);
                    }
                }
            }
            else
            {
                foreach (PropertyInfo prop in type.GetProperties().Where(p => !p.PropertyType.IsPrimitive))
                {
                    object propVal = prop.GetValue(obj);
                    if (propVal == null)
                    {
                        continue;
                    }

                    if (propVal is IEnumerable lst)
                    {
                        foreach (object item in lst)
                        {
                            if (item.GetType().IsPrimitive)
                            {
                                break;
                            }

                            CheckAndApplyODataId(item);
                        }
                    }
                    else
                    {
                        CheckAndApplyODataId(propVal);
                    }
                }
            }

        }

        private object ApplyODataIdOnContainer(ODataIdContainer container)
        {
            PathItem[] pathItems = container.ODataIdNavigationPath.GetNavigationPathItems();
            if (pathItems != null)
            {
                int cnt = 0;
                object value = null;

                while (cnt < pathItems.Length)
                {
                    value = GetObject(pathItems[cnt].Name, value, pathItems[cnt].KeyProperties);
                    cnt++;
                }

                return value;
            }

            return null;
        }

    }
}
