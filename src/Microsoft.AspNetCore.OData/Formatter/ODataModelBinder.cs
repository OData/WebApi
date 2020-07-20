// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// A model binder for ODataParameterValue values.
    /// </summary>
    /// <remarks>
    /// This class is similar to ODataModelBinderProvider in AspNet. The flow is similar but the
    /// type are dissimilar enough making a common version more complex than separate versions.
    /// </remarks>
    internal class ODataModelBinder : IModelBinder
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw Error.ArgumentNull("bindingContext");
            }
            if (bindingContext.ModelMetadata == null)
            {
                throw Error.Argument("bindingContext", SRResources.ModelBinderUtil_ModelMetadataCannotBeNull);
            }
            var modelTypeInfo = bindingContext.ModelType.GetTypeInfo();
            if (bindingContext.ModelMetadata.IsComplexType && 
                !modelTypeInfo.IsAbstract &&
                modelTypeInfo.GetConstructor(Type.EmptyTypes) != null &&
                bindingContext.ModelMetadata.ModelType != typeof(string))
            {
                return new ODataComplexTypeModelBinder().BindModelAsync(bindingContext);
            }

            ValueProviderResult valueProviderResult = ValueProviderResult.None;
            string modelName = ODataParameterValue.ParameterValuePrefix + bindingContext.ModelName;
            try
            {
                // Look in route data for a ODataParameterValue.
                object valueAsObject = null;
                if (!bindingContext.HttpContext.Request.ODataFeature().RoutingConventionsStore.TryGetValue(modelName, out valueAsObject))
                {
                    bindingContext.ActionContext.RouteData.Values.TryGetValue(modelName, out valueAsObject);
                }

                if (valueAsObject != null)
                {
                    StringValues stringValues = new StringValues(valueAsObject.ToString());
                    valueProviderResult = new ValueProviderResult(stringValues);
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                    ODataParameterValue paramValue = valueAsObject as ODataParameterValue;
                    if (paramValue != null)
                    {
                        HttpRequest request = bindingContext.HttpContext.Request;
                        object model = ConvertTo(paramValue, bindingContext, request.GetRequestContainer());
                        bindingContext.Result = ModelBindingResult.Success(model);
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    // If not in the route data, ask the value provider.
                    valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
                    if (valueProviderResult == ValueProviderResult.None)
                    {
                        valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                    }

                    if (valueProviderResult != ValueProviderResult.None)
                    {
                        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                        object model = ODataModelBinderConverter.ConvertTo(valueProviderResult.FirstValue, bindingContext.ModelType);
                        if (model != null)
                        {
                            bindingContext.Result = ModelBindingResult.Success(model);
                            return Task.CompletedTask;
                        }
                    }
                }

                // No matches, binding failed.
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (ODataException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (ValidationException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, valueProviderResult.FirstValue, ex.Message));
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (FormatException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, valueProviderResult.FirstValue, ex.Message));
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (Exception e)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, e.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
        internal static object ConvertTo(ODataParameterValue parameterValue, ModelBindingContext bindingContext, IServiceProvider requestContainer)
        {
            Contract.Assert(parameterValue != null && parameterValue.EdmType != null);

            object oDataValue = parameterValue.Value;
            if (oDataValue == null || oDataValue is ODataNullValue)
            {
                return null;
            }

            IEdmTypeReference edmTypeReference = parameterValue.EdmType;
            ODataDeserializerContext readContext = BuildDeserializerContext(bindingContext, edmTypeReference);
            return ODataModelBinderConverter.Convert(oDataValue, edmTypeReference, bindingContext.ModelType,
                bindingContext.ModelName, readContext, requestContainer);
        }

        internal static ODataDeserializerContext BuildDeserializerContext(ModelBindingContext bindingContext, IEdmTypeReference edmTypeReference)
        {
            HttpRequest request = bindingContext.HttpContext.Request;
            ODataPath path = request.ODataFeature().Path;
            IEdmModel edmModel = request.GetModel();

            return new ODataDeserializerContext
            {
                Path = path,
                Model = edmModel,
                Request = request,
                ResourceType = bindingContext.ModelType,
                ResourceEdmType = edmTypeReference,
            };
        }
    }

}


