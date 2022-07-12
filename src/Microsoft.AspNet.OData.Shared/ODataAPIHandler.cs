//-----------------------------------------------------------------------------
// <copyright file="ODataAPIHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Handler Class to handle users methods for create, delete and update.
    /// This is the handler for data modification where there is a CLR type.
    /// </summary>
    public abstract class ODataAPIHandler<TStructuralType>: IODataAPIHandler where TStructuralType : class
    {
        /// <summary>
        /// TryCreate method to create a new object.
        /// </summary>        
        /// <param name="keyValues">TheKey value pair of the objecct to be created. Optional</param>
        /// <param name="createdObject">The created object (CLR or Typeless)</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns>The status of the TryCreate method <see cref="ODataAPIResponseStatus"/> </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public abstract ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out TStructuralType createdObject, out string errorMessage);

        /// <summary>
        /// TryGet method which tries to get the Origignal object based on a keyvalues.
        /// </summary>
        /// <param name="keyValues">Key value pair for the entity keys</param>        
        /// <param name="originalObject">Object to return</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns>The status of the TryGet method <see cref="ODataAPIResponseStatus"/> </returns>
        public abstract ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage);

        /// <summary>
        /// TryDelete Method which will delete the object based on keyvalue pairs.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="errorMessage"></param>
        /// <returns>The status of the TryGet method <see cref="ODataAPIResponseStatus"/> </returns>
        public abstract ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Get the ODataAPIHandler for the nested type
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler</param>
        /// <returns>The type of Nested ODataAPIHandler</returns>
        public abstract IODataAPIHandler GetNestedHandler(TStructuralType parent, string navigationPropertyName);

        /// <summary>
        /// The parent object
        /// </summary>
        internal TStructuralType ParentObject { get; set; }

        /// <summary>
        /// Apply OdataId for a resource with OdataID container
        /// </summary>
        /// <param name="resource">resource to apply odata id on</param>
        /// <param name="model">The model.</param>
        public virtual void UpdateLinkedObjects(TStructuralType resource, IEdmModel model)
        {
            if (resource != null && model != null)
            {
                this.ParentObject = resource;
                CheckAndApplyODataId(resource, model);
            }
        }

        private ODataPath GetODataPath(string path, IEdmModel model)
        {
            ODataUriParser parser = new ODataUriParser(model, new Uri(path, UriKind.Relative));
            ODataPath odataPath = parser.ParsePath();

            return odataPath;
        }

        private void CheckAndApplyODataId(object obj, IEdmModel model)
        {
            Type type = obj.GetType();
            PropertyInfo property = type.GetProperties().FirstOrDefault(s => s.PropertyType == typeof(IODataIdContainer));
            if (property != null && property.GetValue(obj) is IODataIdContainer container && container != null)
            {
                ODataPath odataPath = GetODataPath(container.ODataId, model);
                object res = ApplyODataIdOnContainer(obj, odataPath);
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

                            CheckAndApplyODataId(item, model);
                        }
                    }
                    else
                    {
                        CheckAndApplyODataId(propVal, model);
                    }
                }
            }
        }

        private object ApplyODataIdOnContainer(object obj, ODataPath odataPath)
        {
            KeySegment keySegment = odataPath.LastOrDefault() as KeySegment;
            Dictionary<string, object> keys = keySegment?.Keys.ToDictionary(x => x.Key, x => x.Value);
            if (keys != null)
            {
                TStructuralType returnedObject;
                string error;
                if (this.ParentObject.Equals(obj))
                {
                    if (this.TryGet(keys, out returnedObject, out error) == ODataAPIResponseStatus.Success)
                    {
                        return returnedObject;
                    }
                    else
                    {
                        if (this.TryCreate(keys, out returnedObject, out error) == ODataAPIResponseStatus.Success)
                        {
                            return returnedObject;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    IODataAPIHandler apiHandlerNested = this.GetNestedHandler(this.ParentObject, odataPath.LastSegment.Identifier);
                    object[] getParams = new object[] { keys, null, null };
                    if (apiHandlerNested.GetType().GetMethod(nameof(TryGet)).Invoke(apiHandlerNested, getParams).Equals(ODataAPIResponseStatus.Success))
                    {
                        return getParams[1];
                    }
                    else
                    {
                        if (apiHandlerNested.GetType().GetMethod(nameof(TryCreate)).Invoke(apiHandlerNested, getParams).Equals(ODataAPIResponseStatus.Success))
                        {
                            return getParams[1];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }
    }
}
