// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.ModelBinding.Binders
{
    // Returns a user-specified binder for a given open generic type.
    public sealed class GenericModelBinderProvider : ModelBinderProvider
    {
        private readonly Func<Type[], IModelBinder> _modelBinderFactory;
        private readonly Type _modelType;

        public GenericModelBinderProvider(Type modelType, IModelBinder modelBinder)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinder == null)
            {
                throw Error.ArgumentNull("modelBinder");
            }

            ValidateParameters(modelType, null /* modelBinderType */);

            _modelType = modelType;
            _modelBinderFactory = _ => modelBinder;
        }

        public GenericModelBinderProvider(Type modelType, Type modelBinderType)
        {
            // The binder can be a closed type, in which case it will be instantiated directly. If the binder
            // is an open type, the type arguments will be determined at runtime and the corresponding closed
            // type instantiated.

            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinderType == null)
            {
                throw Error.ArgumentNull("modelBinderType");
            }

            ValidateParameters(modelType, modelBinderType);
            bool modelBinderTypeIsOpenGeneric = modelBinderType.IsGenericTypeDefinition;

            _modelType = modelType;
            _modelBinderFactory = typeArguments =>
            {
                Type closedModelBinderType = modelBinderTypeIsOpenGeneric ? modelBinderType.MakeGenericType(typeArguments) : modelBinderType;
                return (IModelBinder)Activator.CreateInstance(closedModelBinderType);
            };
        }

        public GenericModelBinderProvider(Type modelType, Func<Type[], IModelBinder> modelBinderFactory)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinderFactory == null)
            {
                throw Error.ArgumentNull("modelBinderFactory");
            }

            ValidateParameters(modelType, null /* modelBinderType */);

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

            Type[] typeArguments = null;
            if (ModelType.IsInterface)
            {
                Type matchingClosedInterface = TypeHelper.ExtractGenericInterface(bindingContext.ModelType, ModelType);
                if (matchingClosedInterface != null)
                {
                    typeArguments = matchingClosedInterface.GetGenericArguments();
                }
            }
            else
            {
                typeArguments = TypeHelper.GetTypeArgumentsIfMatch(bindingContext.ModelType, ModelType);
            }

            if (typeArguments != null)
            {
                if (SuppressPrefixCheck || bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                {
                    return _modelBinderFactory(typeArguments);
                }
            }

            return null;
        }

        private static void ValidateParameters(Type modelType, Type modelBinderType)
        {
            if (!modelType.IsGenericTypeDefinition)
            {
                throw Error.Argument("modelType", SRResources.GenericModelBinderProvider_ParameterMustSpecifyOpenGenericType, modelType);
            }
            if (modelBinderType != null)
            {
                if (!typeof(IModelBinder).IsAssignableFrom(modelBinderType))
                {
                    throw Error.Argument("modelBinderType", SRResources.Common_TypeMustImplementInterface, modelBinderType, typeof(IModelBinder));
                }
                if (modelBinderType.IsGenericTypeDefinition)
                {
                    if (modelType.GetGenericArguments().Length != modelBinderType.GetGenericArguments().Length)
                    {
                        throw Error.Argument("modelBinderType",
                                             SRResources.GenericModelBinderProvider_TypeArgumentCountMismatch,
                                             modelType,
                                             modelType.GetGenericArguments().Length,
                                             modelBinderType,
                                             modelBinderType.GetGenericArguments().Length);
                    }
                }
            }
        }
    }
}
