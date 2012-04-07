// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class ComplexModelDtoModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext, typeof(ComplexModelDto), false /* allowNullModel */);

            ComplexModelDto dto = (ComplexModelDto)bindingContext.Model;
            foreach (ModelMetadata propertyMetadata in dto.PropertyMetadata)
            {
                ModelBindingContext propertyBindingContext = new ModelBindingContext(bindingContext)
                {
                    ModelMetadata = propertyMetadata,
                    ModelName = ModelBindingHelper.CreatePropertyModelName(bindingContext.ModelName, propertyMetadata.PropertyName)
                };

                // bind and propagate the values
                IModelBinder propertyBinder;
                if (actionContext.TryGetBinder(propertyBindingContext, out propertyBinder))
                {
                    if (propertyBinder.BindModel(actionContext, propertyBindingContext))
                    {
                        dto.Results[propertyMetadata] = new ComplexModelDtoResult(propertyBindingContext.Model, propertyBindingContext.ValidationNode);
                    }
                    else
                    {
                        dto.Results[propertyMetadata] = null;
                    }
                }
            }

            return true;
        }
    }
}
