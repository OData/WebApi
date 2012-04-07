// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    // Returns a user-specified binder for a given type.
    public sealed class SimpleModelBinderProvider : ModelBinderProvider
    {
        private readonly Func<IModelBinder> _modelBinderFactory;
        private readonly Type _modelType;

        public SimpleModelBinderProvider(Type modelType, IModelBinder modelBinder)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinder == null)
            {
                throw Error.ArgumentNull("modelBinder");
            }

            _modelType = modelType;
            _modelBinderFactory = () => modelBinder;
        }

        public SimpleModelBinderProvider(Type modelType, Func<IModelBinder> modelBinderFactory)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinderFactory == null)
            {
                throw Error.ArgumentNull("modelBinderFactory");
            }

            _modelType = modelType;
            _modelBinderFactory = modelBinderFactory;
        }

        public Type ModelType
        {
            get { return _modelType; }
        }

        public bool SuppressPrefixCheck { get; set; }

        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (bindingContext.ModelType == ModelType)
            {
                if (SuppressPrefixCheck || bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                {
                    return _modelBinderFactory();
                }
            }

            return null;
        }
    }
}
