// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Validation;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Extension methods for <see cref="HttpActionContext"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpActionContextExtensions
    {
        /// <summary>
        /// Gets the <see cref="ModelMetadataProvider"/> instance for a given <see cref="HttpActionContext"/>.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <returns>An <see cref="ModelMetadataProvider"/> instance.</returns>
        public static ModelMetadataProvider GetMetadataProvider(this HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();
        }

        /// <summary>
        /// Gets the collection of registered <see cref="ModelValidatorProvider"/> instances.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <returns>A collection of <see cref="ModelValidatorProvider"/> instances.</returns>
        public static IEnumerable<ModelValidatorProvider> GetValidatorProviders(this HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return actionContext.ControllerContext.Configuration.Services.GetModelValidatorProviders();
        }

        /// <summary>
        /// Gets the collection of registered <see cref="ModelValidator"/> instances.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>A collection of registered <see cref="ModelValidator"/> instances.</returns>
        public static IEnumerable<ModelValidator> GetValidators(this HttpActionContext actionContext, ModelMetadata metadata)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            IEnumerable<ModelValidatorProvider> validatorProviders = GetValidatorProviders(actionContext);
            return validatorProviders.SelectMany(provider => provider.GetValidators(metadata, validatorProviders));
        }

        /// <summary>
        /// Attempt to bind against the given ActionContext.
        /// </summary>
        /// <param name="actionContext">The execution context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>True if the bind was successful, else false.</returns>
        public static bool Bind(this HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (bindingContext == null)
            {
                throw Error.ArgumentNull("bindingContext");
            }

            Type modelType = bindingContext.ModelType;
            HttpConfiguration config = actionContext.ControllerContext.Configuration;
            
            IModelBinder binder = null;
            ModelBinderProvider providerFromAttr;
            if (ModelBindingHelper.TryGetProviderFromAttributes(modelType, out providerFromAttr))
            {
                binder = providerFromAttr.GetBinder(config, modelType);
                return binder.BindModel(actionContext, bindingContext);
            }

            foreach (ModelBinderProvider provider in config.Services.GetModelBinderProviders())
            {
                binder = provider.GetBinder(config, modelType);
                if (binder != null)
                {
                    if (binder.BindModel(actionContext, bindingContext))
                    {
                        return true;
                    }
                }
            }

            // Either we couldn't find a binder, or the binder couldn't bind. Distinction is not important.
            return false;
        }
    }
}
