// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class ArrayModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (!bindingContext.ModelMetadata.IsReadOnly && bindingContext.ModelType.IsArray &&
                bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                Type elementType = bindingContext.ModelType.GetElementType();
                return (IModelBinder)Activator.CreateInstance(typeof(ArrayModelBinder<>).MakeGenericType(elementType));
            }

            return null;
        }
    }
}
