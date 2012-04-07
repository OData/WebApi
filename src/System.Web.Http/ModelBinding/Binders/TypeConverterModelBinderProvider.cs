// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding.Binders
{
    // Returns a binder that can perform conversions using a .NET TypeConverter.
    public sealed class TypeConverterModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return null; // no value to convert
            }

            if (!TypeHelper.HasStringConverter(bindingContext.ModelType))
            {
                return null; // this type cannot be converted
            }

            return new TypeConverterModelBinder();
        }
    }
}
