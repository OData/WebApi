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
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// The handler class for handling users' get, create, delete and updateRelatedObject methods.
    /// This is the handler for data modification where there is a CLR type.
    /// </summary>
    public abstract class ODataAPIHandler<TStructuralType>: IODataAPIHandler where TStructuralType : class
    {
        /// <summary>
        /// Create a new object.
        /// </summary>        
        /// <param name="keyValues">The key-value pair of the object to be created. Optional.</param>
        /// <param name="createdObject">The created object.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryCreate method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out TStructuralType createdObject, out string errorMessage);

        /// <summary>
        /// Get the original object based on key-values.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="originalObject">Object to return.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryGet method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage);

        /// <summary>
        /// Delete the object based on key-value pairs.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryGet method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Add related object.
        /// </summary>
        /// <param name="resource">The object to be added.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the AddRelatedObject method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryAddRelatedObject(TStructuralType resource, out string errorMessage);

        /// <summary>
        /// Get the ODataAPIHandler for the nested type.
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler.</param>
        /// <returns>The type of Nested ODataAPIHandler.</returns>
        public abstract IODataAPIHandler GetNestedHandler(TStructuralType parent, string navigationPropertyName);

        /// <summary>
        /// Apply handlers to the top level resource and the nested resources.
        /// </summary>
        /// <param name="resource">Resource to execute.</param>
        /// <param name="model">The model.</param>
        /// <param name="apiHandlerFactory">API Handler Factory.</param>
        public virtual void DeepInsert(TStructuralType resource, IEdmModel model, ODataAPIHandlerFactory apiHandlerFactory)
        {
            if (resource != null && model != null)
            {
                CopyObjectProperties(resource, model, this, apiHandlerFactory);
            }
        }

        internal static void CopyObjectProperties(object obj, IEdmModel model, IODataAPIHandler apiHandler, ODataAPIHandlerFactory apiHandlerFactory)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            PropertyInfo odataIdContainerProperty = properties.FirstOrDefault(s => s.PropertyType == typeof(ODataIdContainer));

            string edmFullName = type.EdmFullName();
            IEdmSchemaType schemaType = model.FindType(edmFullName);
            IEdmEntityType edmEntityType = schemaType as IEdmEntityType;

            IEnumerable<IEdmStructuralProperty> entityKey = edmEntityType.Key();

            IDictionary<string, object> keys = GetKeys(entityKey, obj, type);

            IODataAPIHandler odataIdContainerHandler = null;

            if (odataIdContainerProperty != null && odataIdContainerProperty.GetValue(obj) is ODataIdContainer container && container != null && apiHandlerFactory != null)
            {
                ODataPath odataPath = GetODataPath(container.ODataId, model);

                if (odataPath != null)
                {
                    odataIdContainerHandler = apiHandlerFactory.GetHandler(odataPath);

                    if (odataIdContainerHandler != null)
                    {
                        keys = odataPath.GetKeys();
                    }
                }
            }

            List<IEdmNavigationProperty> navigationProperties = edmEntityType.NavigationProperties().ToList();

            List<string> navPropNames = new List<string>();

            foreach (IEdmNavigationProperty navProp in navigationProperties)
            {
                navPropNames.Add(navProp.Name);
            }

            bool failedOperation;

            ApplyHandlers(apiHandler, odataIdContainerHandler, obj, keys, navPropNames, out failedOperation);

            // If operation fails, we shouldn't continue with the nested properties.
            if (!failedOperation)
            {
                CopyNestedProperties(obj, type, model, apiHandler, apiHandlerFactory, navPropNames);
            }
        }

        internal static void CopyNestedProperties(object obj, Type type, IEdmModel model, IODataAPIHandler apiHandler, ODataAPIHandlerFactory apiHandlerFactory, List<string> navPropNames)
        {
            foreach (string navPropertName in navPropNames)
            {
                PropertyInfo prop = type.GetProperty(navPropertName);
                var navPropVal = prop.GetValue(obj);

                if (navPropVal == null)
                {
                    continue;
                }

                object parentObj = GetObjectWithoutNavigationProperties(obj, navPropNames);

                object[] nestedHandlerParams = new object[] { parentObj, navPropertName };
                IODataAPIHandler nestedHandler = (IODataAPIHandler)apiHandler.GetType().GetMethod(nameof(GetNestedHandler)).Invoke(apiHandler, nestedHandlerParams);

                if (navPropVal is IEnumerable lst)
                {
                    foreach (var item in lst)
                    {
                        if (item.GetType().IsPrimitive)
                        {
                            break;
                        }

                        CopyObjectProperties(item, model, nestedHandler, apiHandlerFactory);
                    }
                }
                else
                {
                    CopyObjectProperties(navPropVal, model, nestedHandler, apiHandlerFactory);
                }
            }
        }

        // TODO: Error handling
        internal static void ApplyHandlers(IODataAPIHandler odataApiHandler, IODataAPIHandler odataIdContainerHandler, object resource, IDictionary<string, object> keys, List<string> navigationProperties, out bool failedOperation)
        {
            failedOperation = false;
            DataModificationOperationKind operation = DataModificationOperationKind.Insert;

            try
            {
                object[] handlerParams = new object[] { keys, null, null };

                IODataAPIHandler handlerForGet = odataApiHandler;
                ODataAPIResponseStatus getResponse = ODataAPIResponseStatus.NotFound;
                object getObject = null;

                if (odataIdContainerHandler != null)
                {
                    handlerForGet = odataIdContainerHandler;
                    getResponse = (ODataAPIResponseStatus)handlerForGet.GetType().GetMethod(nameof(TryGet)).Invoke(handlerForGet, handlerParams);
                    getObject = handlerParams[1];
                }

                if (getResponse == ODataAPIResponseStatus.Success)
                {
                    operation = DataModificationOperationKind.Link;

                    CopyProperties(getObject, resource, navigationProperties);

                    object[] addRelatedObjectParams = new object[] { getObject, null };

                    ODataAPIResponseStatus addRelatedObjectResponse = (ODataAPIResponseStatus)odataApiHandler.GetType().GetMethod(nameof(TryAddRelatedObject)).Invoke(odataApiHandler, addRelatedObjectParams);

                    if (addRelatedObjectResponse == ODataAPIResponseStatus.Failure)
                    {
                        HandleFailedOperation(resource, operation, addRelatedObjectParams[1].ToString(), navigationProperties);
                        failedOperation = true;
                    }
                }
                else
                {
                    ODataAPIResponseStatus createObjectResponse = (ODataAPIResponseStatus)odataApiHandler.GetType().GetMethod(nameof(TryCreate)).Invoke(odataApiHandler, handlerParams);

                    if (createObjectResponse == ODataAPIResponseStatus.Failure)
                    {
                        HandleFailedOperation(resource, operation, handlerParams[2].ToString(), navigationProperties);
                        failedOperation = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleFailedOperation(resource, operation, ex.Message, navigationProperties);
            }
        }

        private static void HandleFailedOperation(object originalObject, DataModificationOperationKind operation, string errorMessage, List<string> navigationProperties)
        {
            Type type = originalObject.GetType();
            PropertyInfo[] properties = type.GetProperties();
            PropertyInfo oDataInstanceAnnotationContainerPropertyInfo = properties.FirstOrDefault(s => s.PropertyType == typeof(ODataInstanceAnnotationContainer) || s.PropertyType == typeof(IODataInstanceAnnotationContainer));

            if (oDataInstanceAnnotationContainerPropertyInfo != null)
            {
                DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
                dataModificationExceptionType.MessageType = new Org.OData.Core.V1.MessageType { Message = errorMessage };

                IODataInstanceAnnotationContainer odataInstanceAnnotationContainer = oDataInstanceAnnotationContainerPropertyInfo.GetValue(originalObject) as IODataInstanceAnnotationContainer;

                odataInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationExceptionType);
            }
        }

        private static object GetObjectWithoutNavigationProperties(object originalObj, List<string> navPropNames)
        {
            object newObject = Activator.CreateInstance(originalObj.GetType());

            foreach (PropertyInfo prop in originalObj.GetType().GetProperties())
            {
                // Don't copy Navigation Properties. They will be handled in the next level nesting.
                if (!navPropNames.Contains(prop.Name))
                {
                    object resVal = prop.GetValue(originalObj);

                    if (resVal != null)
                    {
                        prop.SetValue(newObject, resVal);
                    }
                }
            }

            return newObject;
        }

        private static void CopyProperties(object originalObject, object newObject, List<string> navPropNames)
        {
            foreach (PropertyInfo prop in originalObject.GetType().GetProperties())
            {
                // Don't copy Navigation Properties. They will be handled in the next level nesting.
                if (!navPropNames.Contains(prop.Name))
                {
                    object resVal = prop.GetValue(originalObject);

                    if (resVal != null)
                    {
                        prop.SetValue(newObject, resVal);
                    }
                }
            }
        }

        private static IDictionary<string, object> GetKeys(IEnumerable<IEdmStructuralProperty> properties, object resource, Type type)
        {
            IDictionary<string, object> keys = new Dictionary<string, object>();

            foreach (IEdmStructuralProperty property in properties)
            {
                PropertyInfo prop = type.GetProperty(property.Name);
                object value = prop.GetValue(resource, null);

                keys.Add(new KeyValuePair<string, object>(property.Name, value));
            }

            return keys;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't need to throw an exception but instead return odataPath as null.")]
        private static ODataPath GetODataPath(string path, IEdmModel model)
        {
            ODataPath odataPath;
            try
            {
                ODataUriParser parser = new ODataUriParser(model, new Uri(path, UriKind.Relative));
                odataPath = parser.ParsePath();
            }
            catch (Exception)
            {
                odataPath = null;
            }

            return odataPath;
        }
    }
}
