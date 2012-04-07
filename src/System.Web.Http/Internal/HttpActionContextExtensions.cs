// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Internal
{
    internal static class HttpActionContextExtensions
    {
        public static bool TryBindStrongModel<TModel>(this HttpActionContext actionContext, ModelBindingContext parentBindingContext, string propertyName, ModelMetadataProvider metadataProvider, out TModel model)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            ModelBindingContext propertyBindingContext = new ModelBindingContext(parentBindingContext)
            {
                ModelMetadata = metadataProvider.GetMetadataForType(null, typeof(TModel)),
                ModelName = ModelBindingHelper.CreatePropertyModelName(parentBindingContext.ModelName, propertyName)
            };

            IModelBinder binder;
            if (actionContext.TryGetBinder(propertyBindingContext, out binder))
            {
                if (binder.BindModel(actionContext, propertyBindingContext))
                {
                    object untypedModel = propertyBindingContext.Model;
                    model = ModelBindingHelper.CastOrDefault<TModel>(untypedModel);
                    parentBindingContext.ValidationNode.ChildNodes.Add(propertyBindingContext.ValidationNode);
                    return true;
                }
            }

            model = default(TModel);
            return false;
        }
    }
}
