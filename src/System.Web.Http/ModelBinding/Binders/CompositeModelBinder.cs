// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.ModelBinding.Binders
{
    /// <summary>
    /// This class is an <see cref="IModelBinder"/> that delegates to one of a collection of
    /// <see cref="ModelBinderProvider"/> instances.
    /// </summary>
    /// <remarks>
    /// If no binder is available and the <see cref="ModelBindingContext"/> allows it,
    /// this class tries to find a binder using an empty prefix.
    /// </remarks>
    public class CompositeModelBinder : IModelBinder
    {
        public CompositeModelBinder(IEnumerable<ModelBinderProvider> modelBinderProviders)
            : this(modelBinderProviders.ToArray())
        {
        }
        public CompositeModelBinder(ModelBinderProvider[] modelBinderProviders)
        {
            Providers = modelBinderProviders;
        }

        private ModelBinderProvider[] Providers { get; set; }

        public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            //// REVIEW: from MVC Futures
            ////CheckPropertyFilter(bindingContext);

            ModelBindingContext newBindingContext = CreateNewBindingContext(bindingContext, bindingContext.ModelName);

            IModelBinder binder = GetBinder(actionContext, newBindingContext);
            if (binder == null && !String.IsNullOrEmpty(bindingContext.ModelName)
                && bindingContext.FallbackToEmptyPrefix)
            {
                // fallback to empty prefix?
                newBindingContext = CreateNewBindingContext(bindingContext, String.Empty /* modelName */);
                binder = GetBinder(actionContext, newBindingContext);
            }

            if (binder != null)
            {
                bool boundSuccessfully = binder.BindModel(actionContext, newBindingContext);
                if (boundSuccessfully)
                {
                    // run validation and return the model
                    // If we fell back to an empty prefix above and are dealing with simple types,
                    // propagate the non-blank model name through for user clarity in validation errors.
                    // Complex types will reveal their individual properties as model names and do not require this.
                    if (!newBindingContext.ModelMetadata.IsComplexType && String.IsNullOrEmpty(newBindingContext.ModelName))
                    {
                        newBindingContext.ValidationNode = new Validation.ModelValidationNode(newBindingContext.ModelMetadata, bindingContext.ModelName);
                    }

                    newBindingContext.ValidationNode.Validate(actionContext, null /* parentNode */);
                    bindingContext.Model = newBindingContext.Model;
                    return true;
                }
            }

            return false; // something went wrong
        }

        //// REVIEW: from MVC Futures -- activate when Filters are in
        ////private static void CheckPropertyFilter(ModelBindingContext bindingContext)
        ////{
        ////    if (bindingContext.ModelType.GetProperties().Select(p => p.Name).Any(name => !bindingContext.PropertyFilter(name)))
        ////    {
        ////        throw Error.InvalidOperation(SRResources.ExtensibleModelBinderAdapter_PropertyFilterMustNotBeSet);
        ////    }
        ////}

        private IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(bindingContext != null);

            //// REVIEW: this was in MVC Futures
            ////EnsureNoBindAttribute(bindingContext.ModelType);

            ModelBinderProvider providerFromAttr;
            if (TryGetProviderFromAttributes(bindingContext.ModelType, out providerFromAttr))
            {
                return providerFromAttr.GetBinder(actionContext, bindingContext);
            }

            return (from provider in Providers
                    let binder = provider.GetBinder(actionContext, bindingContext)
                    where binder != null
                    select binder).FirstOrDefault();
        }

        private static ModelBindingContext CreateNewBindingContext(ModelBindingContext oldBindingContext, string modelName)
        {
            ModelBindingContext newBindingContext = new ModelBindingContext
            {
                ModelMetadata = oldBindingContext.ModelMetadata,
                ModelName = modelName,
                ModelState = oldBindingContext.ModelState,
                ValueProvider = oldBindingContext.ValueProvider
            };

            // validation is expensive to create, so copy it over if we can
            if (object.ReferenceEquals(modelName, oldBindingContext.ModelName))
            {
                newBindingContext.ValidationNode = oldBindingContext.ValidationNode;
            }

            return newBindingContext;
        }

        private static bool TryGetProviderFromAttributes(Type modelType, out ModelBinderProvider provider)
        {
            ModelBinderAttribute attr = TypeDescriptorHelper.Get(modelType).GetAttributes().OfType<ModelBinderAttribute>().FirstOrDefault();
            if (attr == null)
            {
                provider = null;
                return false;
            }

            // TODO, 386718, remove the following if statement when the bug is resolved
            if (attr.BinderType == null)
            {
                provider = null;
                return false;
            }

            if (typeof(ModelBinderProvider).IsAssignableFrom(attr.BinderType))
            {
                provider = (ModelBinderProvider)Activator.CreateInstance(attr.BinderType);
            }
            else
            {
                throw Error.InvalidOperation(SRResources.ModelBinderProviderCollection_InvalidBinderType, attr.BinderType.Name, typeof(ModelBinderProvider).Name, typeof(IModelBinder).Name);
            }

            return true;
        }
    }
}
