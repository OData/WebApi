// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    // Returns a binder that can extract a ValueProviderResult.RawValue and return it directly.
    public sealed class TypeMatchModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            return (TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext) != null)
                       ? new TypeMatchModelBinder()
                       : null /* no match */;
        }
    }
}
