// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;

namespace Microsoft.Web.Http.Data
{
    internal static class DataControllerValidation
    {
        internal static bool ValidateObject(object o, List<ValidationResultInfo> validationErrors, HttpActionContext actionContext)
        {
            // create a model validation node for the object
            ModelMetadataProvider metadataProvider = actionContext.GetMetadataProvider();
            string modelStateKey = String.Empty;
            ModelValidationNode validationNode = CreateModelValidationNode(o, metadataProvider, actionContext.ModelState, modelStateKey);
            validationNode.ValidateAllProperties = true;

            // add the node to model state
            ModelState modelState = new ModelState();
            modelState.Value = new ValueProviderResult(o, String.Empty, CultureInfo.CurrentCulture);
            actionContext.ModelState.Add(modelStateKey, modelState);

            // invoke validation
            validationNode.Validate(actionContext);

            if (!actionContext.ModelState.IsValid)
            {
                foreach (var modelStateItem in actionContext.ModelState)
                {
                    foreach (ModelError modelError in modelStateItem.Value.Errors)
                    {
                        validationErrors.Add(new ValidationResultInfo(modelError.ErrorMessage, new string[] { modelStateItem.Key }));
                    }
                }
            }

            return actionContext.ModelState.IsValid;
        }

        private static ModelValidationNode CreateModelValidationNode(object o, ModelMetadataProvider metadataProvider, ModelStateDictionary modelStateDictionary, string modelStateKey)
        {
            ModelMetadata metadata = metadataProvider.GetMetadataForType(() =>
            {
                return o;
            }, o.GetType());
            ModelValidationNode validationNode = new ModelValidationNode(metadata, modelStateKey);

            // for this root node, recursively add all child nodes
            HashSet<object> visited = new HashSet<object>();
            CreateModelValidationNodeRecursive(o, validationNode, metadataProvider, metadata, modelStateDictionary, modelStateKey, visited);

            return validationNode;
        }

        private static void CreateModelValidationNodeRecursive(object o, ModelValidationNode parentNode, ModelMetadataProvider metadataProvider, ModelMetadata metadata, ModelStateDictionary modelStateDictionary, string modelStateKey, HashSet<object> visited)
        {
            if (visited.Contains(o))
            {
                return;
            }
            visited.Add(o);

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(o))
            {
                // append the current property name to the model state path
                string propertyKey = modelStateKey;
                if (propertyKey.Length > 0)
                {
                    propertyKey += ".";
                }
                propertyKey += property.Name;

                // create the node for this property and add to the parent node
                object propertyValue = property.GetValue(o);
                metadata = metadataProvider.GetMetadataForProperty(() =>
                {
                    return propertyValue;
                }, o.GetType(), property.Name);
                ModelValidationNode childNode = new ModelValidationNode(metadata, propertyKey);
                parentNode.ChildNodes.Add(childNode);

                // add the property node to model state
                ModelState modelState = new ModelState();
                modelState.Value = new ValueProviderResult(propertyValue, null, CultureInfo.CurrentCulture);
                modelStateDictionary.Add(propertyKey, modelState);

                if (propertyValue != null)
                {
                    CreateModelValidationNodeRecursive(propertyValue, childNode, metadataProvider, metadata, modelStateDictionary, propertyKey, visited);
                }
            }
        }
    }
}
