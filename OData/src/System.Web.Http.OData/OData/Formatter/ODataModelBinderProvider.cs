// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData.Properties;
using System.Web.Http.ValueProviders;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// Provides a <see cref="IModelBinder"/> for EDM primitive types.
    /// </summary>
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

            if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(modelType) != null)
            {
                return new ODataModelBinder();
            }

            return null;
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

                ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (value == null)
                {
                    return false;
                }
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

                try
                {
                    string valueString = value.RawValue as string;
                    object model = ConvertTo(valueString, bindingContext.ModelType);
                    bindingContext.Model = model;
                    return true;
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

            internal static object ConvertTo(string valueString, Type type)
            {
                if (valueString == null)
                {
                    return null;
                }

                object value = ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V3);

                bool isNonStandardEdmPrimitive;
                EdmLibHelpers.IsNonstandardEdmPrimitive(type, out isNonStandardEdmPrimitive);

                if (isNonStandardEdmPrimitive)
                {
                    return EdmPrimitiveHelpers.ConvertPrimitiveValue(value, type);
                }
                else
                {
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
