// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    // Returns a binder that can bind ComplexModelDto objects.
    public sealed class ComplexModelDtoModelBinderProvider : ModelBinderProvider
    {
        // This is really just a simple binder.
        private static readonly SimpleModelBinderProvider _underlyingProvider = GetUnderlyingProvider();

        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            return _underlyingProvider.GetBinder(actionContext, bindingContext);
        }

        private static SimpleModelBinderProvider GetUnderlyingProvider()
        {
            return new SimpleModelBinderProvider(typeof(ComplexModelDto), new ComplexModelDtoModelBinder())
            {
                SuppressPrefixCheck = true
            };
        }
    }
}
