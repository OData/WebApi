// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class KeyValuePairModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            string keyFieldName = ModelBindingHelper.CreatePropertyModelName(bindingContext.ModelName, "key");
            string valueFieldName = ModelBindingHelper.CreatePropertyModelName(bindingContext.ModelName, "value");

            if (bindingContext.ValueProvider.ContainsPrefix(keyFieldName) && bindingContext.ValueProvider.ContainsPrefix(valueFieldName))
            {
                return ModelBindingHelper.GetPossibleBinderInstance(bindingContext.ModelType, typeof(KeyValuePair<,>) /* supported model type */, typeof(KeyValuePairModelBinder<,>) /* binder type */);
            }
            else
            {
                // 'key' or 'value' missing
                return null;
            }
        }
    }
}
