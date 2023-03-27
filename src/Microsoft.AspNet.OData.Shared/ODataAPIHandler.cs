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
using System.Threading.Tasks;
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
        /// Create a new object asynchronously.
        /// </summary>        
        /// <param name="keyValues">The key-value pair of the object to be created. Optional.</param>
        /// <param name="createdObject">The created object.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>A task representing the status of the TryCreateAsync method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract Task<ODataAPIResponseStatus> TryCreateAsync(IDictionary<string, object> keyValues, out TStructuralType createdObject, out string errorMessage);

        /// <summary>
        /// Get the original object based on key-values.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="originalObject">Object to return.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryGet method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage);

        /// <summary>
        /// Get the original object based on key-values asynchronously.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="originalObject">Object to return.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>A task representing the status of the TryGetAsync method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract Task<ODataAPIResponseStatus> TryGetAsync(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage);

        /// <summary>
        /// Delete the object based on key-value pairs.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryGet method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Delete the object based on key-value pairs asynchronously.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>A task representing the status of the TryDeleteAsync method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract Task<ODataAPIResponseStatus> TryDeleteAsync(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Add related object.
        /// </summary>
        /// <param name="resource">The object to be added.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the AddRelatedObject method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryAddRelatedObject(TStructuralType resource, out string errorMessage);

        /// <summary>
        /// Add related object asynchronously.
        /// </summary>
        /// <param name="resource">The object to be added.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>A task representing the status of the TryAddRelatedObjectAsync method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract Task<ODataAPIResponseStatus> TryAddRelatedObjectAsync(TStructuralType resource, out string errorMessage);
        
        /// <summary>
        /// Get the ODataAPIHandler for the nested type.
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler.</param>
        /// <returns>The type of Nested ODataAPIHandler.</returns>
        public abstract IODataAPIHandler GetNestedHandler(TStructuralType parent, string navigationPropertyName);

        /// <summary>
        /// Get the ODataAPIHandler for the nested type asynchronously.
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler.</param>
        /// <returns>A task representing the type of the nested ODataAPIHandler.</returns>
        public abstract IODataAPIHandler GetNestedHandlerAsync(TStructuralType parent, string navigationPropertyName);

        /// <summary>
        /// Apply handlers to the top level resource and the nested resources.
        /// </summary>
        /// <param name="resource">Resource to execute.</param>
        /// <param name="model">The model.</param>
        /// <param name="apiHandlerFactory">API handler factory.</param>
        public virtual void DeepInsert(TStructuralType resource, IEdmModel model, ODataAPIHandlerFactory apiHandlerFactory)
        {
            if (resource == null || model == null)
            {
                return;
            }

            CopyObjectProperties(resource, model, this, apiHandlerFactory);
        }

        /// <summary>
        /// Apply handlers to an object.
        /// </summary>
        /// <param name="resource">The resource to apply the API handlers</param>
        /// <param name="model">The model.</param>
        /// <param name="apiHandler">The API handler for the resource.</param>
        /// <param name="apiHandlerFactory">API handler factory.</param>
        internal static void CopyObjectProperties(object resource, IEdmModel model, IODataAPIHandler apiHandler, ODataAPIHandlerFactory apiHandlerFactory)
        {
            if (resource == null || model == null || apiHandler == null)
            {
                return;
            }

            Type type = resource.GetType();
            PropertyInfo[] properties = type.GetProperties();
            PropertyInfo odataIdContainerProperty = properties.FirstOrDefault(s => s.PropertyType == typeof(ODataIdContainer));

            string edmFullName = type.EdmFullName();
            IEdmSchemaType schemaType = null;
            IEdmEntityType edmEntityType = null;
            IEnumerable<IEdmStructuralProperty> entityKey = null;
            IEnumerable<IEdmNavigationProperty> navigationProperties = null;

            if (!string.IsNullOrEmpty(edmFullName))
            {
                schemaType = model.FindType(edmFullName);
            }

            if (schemaType != null)
            {
                edmEntityType = schemaType as IEdmEntityType;
            }

            if (edmEntityType != null)
            {
                entityKey = edmEntityType.Key();
                navigationProperties = edmEntityType.NavigationProperties();
            }

            IDictionary<string, object> keys = GetKeys(entityKey, resource, type); // Refactored ApplyHandler. Consider removing this.

            IODataAPIHandler odataIdContainerHandler = null;

            if (odataIdContainerProperty != null && odataIdContainerProperty.GetValue(resource) is ODataIdContainer container && container != null && apiHandlerFactory != null)
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

            List<string> navPropNames = new List<string>();

            if (navigationProperties != null)
            {
                foreach (IEdmNavigationProperty navProp in navigationProperties)
                {
                    navPropNames.Add(navProp.Name);
                }
            }

            bool failedOperation;

            ApplyHandlers(apiHandler, odataIdContainerHandler, resource, keys, navPropNames, out failedOperation);

            // If operation fails, we shouldn't continue with the nested properties.
            if (!failedOperation)
            {
                CopyNestedProperties(resource, type, model, apiHandler, apiHandlerFactory, navPropNames);
            }
        }

        /// <summary>
        /// ApplyHandlers on each nested object.
        /// </summary>
        /// <param name="resource">The resource with the nested properties.</param>
        /// <param name="type">The clr type of the object.</param>
        /// <param name="model">The model.</param>
        /// <param name="apiHandler">The API handler for the object.</param>
        /// <param name="apiHandlerFactory">The API handler factory.</param>
        /// <param name="navPropNames">The property names of all navigation properties in the resource.</param>
        internal static void CopyNestedProperties(object resource, Type type, IEdmModel model, IODataAPIHandler apiHandler, ODataAPIHandlerFactory apiHandlerFactory, List<string> navPropNames)
        {
            foreach (string navPropertName in navPropNames)
            {
                PropertyInfo prop = type.GetProperty(navPropertName);
                var navPropVal = prop.GetValue(resource);

                if (navPropVal == null)
                {
                    continue;
                }

                object parentObj = GetObjectWithoutNavigationPropertyValues(resource, type, navPropNames);

                object[] nestedHandlerParams = new object[] { parentObj, navPropertName };

                MethodInfo nestedHandlerMethodInfo = apiHandler.GetType().GetMethod(nameof(GetNestedHandler));

                if (nestedHandlerMethodInfo == null)
                {
                    return;
                }

                IODataAPIHandler nestedHandler = (IODataAPIHandler)nestedHandlerMethodInfo.Invoke(apiHandler, nestedHandlerParams);

                if (navPropVal is IEnumerable lst)
                {
                    foreach (var item in lst)
                    {
                        CopyObjectProperties(item, model, nestedHandler, apiHandlerFactory);
                    }
                }
                else
                {
                    CopyObjectProperties(navPropVal, model, nestedHandler, apiHandlerFactory);
                }
            }
        }

        /// <summary>
        /// Apply handlers to an object in the payload. Copy properties from the object payload to the object create by the handler.
        /// For TryGet (@odata.id object), copy the properties of the object return from the handler to the object from the payload.
        /// </summary>
        /// <param name="odataApiHandler">The top level handler or the nested handler related to the object.</param>
        /// <param name="odataIdContainerHandler">The handler from the <see cref="ODataIdContainer"/>.</param>
        /// <param name="resource">The object to apply the handlers to.</param>
        /// <param name="keys">Keys in the object.</param>
        /// <param name="navigationProperties">Navigation properties in the object.</param>
        /// <param name="failedOperation">Boolean indicating if the operation fails.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't have a specific exception to catch.")]
        internal static void ApplyHandlers(IODataAPIHandler odataApiHandler, IODataAPIHandler odataIdContainerHandler, object resource, IDictionary<string, object> keys, List<string> navigationProperties, out bool failedOperation)
        {
            failedOperation = false;
            DataModificationOperationKind operation = DataModificationOperationKind.Insert;

            try
            {
                object[] handlerParams = new object[] { keys, null, null };

                ODataAPIResponseStatus responseFromGetRequest = ODataAPIResponseStatus.NotFound;
                object returnedObject = null;

                if (odataIdContainerHandler != null)
                {
                    MethodInfo getMethodinfo = odataIdContainerHandler.GetType().GetMethod(nameof(TryGet));

                    if (getMethodinfo == null)
                    {
                        return;
                    }

                    responseFromGetRequest = (ODataAPIResponseStatus)getMethodinfo.Invoke(odataIdContainerHandler, handlerParams);
                    returnedObject = handlerParams[1];
                }

                if (responseFromGetRequest == ODataAPIResponseStatus.Success)
                {
                    operation = DataModificationOperationKind.Link;

                    CopyProperties(returnedObject, resource, navigationProperties);

                    object[] addRelatedObjectParams = new object[] { returnedObject, null };

                    MethodInfo tryAddRelatedObjectMethodinfo = odataApiHandler.GetType().GetMethod(nameof(TryAddRelatedObject));

                    if (tryAddRelatedObjectMethodinfo == null)
                    {
                        return;
                    }

                    ODataAPIResponseStatus responseFromAddRelatedObject = (ODataAPIResponseStatus)tryAddRelatedObjectMethodinfo.Invoke(odataApiHandler, addRelatedObjectParams);

                    if (responseFromAddRelatedObject != ODataAPIResponseStatus.Success)
                    {
                        HandleFailedOperation(resource, operation, addRelatedObjectParams[1].ToString());
                        failedOperation = true;
                    }
                }
                else
                {
                    MethodInfo tryCreateMethodinfo = odataApiHandler.GetType().GetMethod(nameof(TryCreate));

                    if (tryCreateMethodinfo == null)
                    {
                        return;
                    }

                    ODataAPIResponseStatus responseFromCreateObject = (ODataAPIResponseStatus)tryCreateMethodinfo.Invoke(odataApiHandler, handlerParams);
                    returnedObject = handlerParams[1];

                    if (responseFromCreateObject == ODataAPIResponseStatus.Success)
                    {
                        CopyProperties(resource, returnedObject, navigationProperties);
                    }
                    else {
                        HandleFailedOperation(resource, operation, handlerParams[2].ToString());
                        failedOperation = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleFailedOperation(resource, operation, ex.Message);
                failedOperation = true;
            }
        }

        /// <summary>
        /// Handler a failed operation by creating a <see cref="DataModificationExceptionType"/> and adding it to the object as instance annotation.
        /// </summary>
        /// <param name="originalObject">The object from a failed operation.</param>
        /// <param name="operation">A <see cref="DataModificationOperationKind"/>.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void HandleFailedOperation(object originalObject, DataModificationOperationKind operation, string errorMessage)
        {
            Type type = originalObject.GetType();
            PropertyInfo[] properties = type.GetProperties();
            PropertyInfo oDataInstanceAnnotationContainerPropertyInfo = properties.FirstOrDefault(s => typeof(IODataInstanceAnnotationContainer).IsAssignableFrom(s.PropertyType));

            if (oDataInstanceAnnotationContainerPropertyInfo != null)
            {
                DataModificationExceptionType dataModificationExceptionType = new DataModificationExceptionType(operation);
                dataModificationExceptionType.MessageType = new Org.OData.Core.V1.MessageType { Message = errorMessage };

                IODataInstanceAnnotationContainer odataInstanceAnnotationContainer = oDataInstanceAnnotationContainerPropertyInfo.GetValue(originalObject) as IODataInstanceAnnotationContainer;

                odataInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationExceptionType);
            }
        }

        /// <summary>
        /// Create a new object from the original object without navigation property values.
        /// </summary>
        /// <param name="originalObj">The original object.</param>
        /// <param name="type">The CLR type of the original object.</param>
        /// <param name="navPropNames">Navigation property names.</param>
        /// <returns>An object without navigation property values.</returns>
        private static object GetObjectWithoutNavigationPropertyValues(object originalObj, Type type, List<string> navPropNames)
        {
            object newObject = Activator.CreateInstance(type);

            foreach (PropertyInfo prop in type.GetProperties())
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

            if (properties == null)
            {
                return keys;
            }

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
