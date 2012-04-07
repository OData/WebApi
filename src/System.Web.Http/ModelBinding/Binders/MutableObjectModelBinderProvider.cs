// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class MutableObjectModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (!bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                // no values to bind
                return null;
            }

            // Simple types cannot use this binder
            if (!bindingContext.ModelMetadata.IsComplexType)
            {
                return null;
            }

            if (bindingContext.ModelType == typeof(ComplexModelDto))
            {
                // forbidden type - will cause a stack overflow if we try binding this type
                return null;
            }

            return new MutableObjectModelBinder();
        }
    }
}
