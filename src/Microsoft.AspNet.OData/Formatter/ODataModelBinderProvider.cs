//-----------------------------------------------------------------------------
// <copyright file="ODataModelBinderProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Provides a <see cref="IModelBinder"/> for EDM primitive types.
    /// </summary>
    /// <remarks>
    /// This class is similar to ODataModelBinder in AspNetCore. The flow is similar but the
    /// type are dissimilar enough making a common version more complex than separate versions.
    /// </remarks>
    public class ODataModelBinderProvider : ModelBinderProvider
    {
        /// <inheritdoc />
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            return new ODataModelBinder();
        }

        internal class ODataModelBinder : IModelBinder
        {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw Error.ArgumentNull("bindingContext");
                }

                if (bindingContext.ModelMetadata == null)
                {
                    throw Error.Argument("bindingContext", SRResources.ModelBinderUtil_ModelMetadataCannotBeNull);
                }

                string modelName = ODataParameterValue.ParameterValuePrefix + bindingContext.ModelName;
                ValueProviderResult value = bindingContext.ValueProvider.GetValue(modelName);
                if (value == null)
                {
                    value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                    if (value == null)
                    {
                        return false;
                    }
                }

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

                try
                {
                    ODataParameterValue paramValue = value.RawValue as ODataParameterValue;
                    if (paramValue != null)
                    {
                        bindingContext.Model = ConvertTo(paramValue, actionContext, bindingContext,
                            actionContext.Request.GetRequestContainer());
                        return true;
                    }

                    string valueString = value.RawValue as string;
                    if (valueString != null)
                    {
                        bindingContext.Model = ODataModelBinderConverter.ConvertTo(valueString, bindingContext.ModelType);
                        return true;
                    }

                    return false;
                }
                catch (ODataException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                    return false;
                }
                catch (ValidationException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, value.RawValue, ex.Message));
                    return false;
                }
                catch (FormatException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, value.RawValue, ex.Message));
                    return false;
                }
                catch (Exception e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, e);
                    return false;
                }
            }

            internal static object ConvertTo(ODataParameterValue parameterValue, HttpActionContext actionContext, ModelBindingContext bindingContext,
                IServiceProvider requestContainer)
            {
                Contract.Assert(parameterValue != null && parameterValue.EdmType != null);

                object oDataValue = parameterValue.Value;
                if (oDataValue == null || oDataValue is ODataNullValue)
                {
                    return null;
                }

                IEdmTypeReference edmTypeReference = parameterValue.EdmType;
                ODataDeserializerContext readContext = BuildDeserializerContext(actionContext, bindingContext, edmTypeReference);
                return ODataModelBinderConverter.Convert(oDataValue, edmTypeReference, bindingContext.ModelType,
                    bindingContext.ModelName, readContext, requestContainer);
            }

            internal static ODataDeserializerContext BuildDeserializerContext(HttpActionContext actionContext,
                ModelBindingContext bindingContext, IEdmTypeReference edmTypeReference)
            {
                HttpRequestMessage request = actionContext.Request;
                ODataPath path = request.ODataProperties().Path;
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
}
