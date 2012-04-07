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
        /// Gets the <see cref="ModelBindingContext"/> for this <see cref="HttpActionContext"/>.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <param name="binder">When this method returns, the value associated with the specified binding context, if the context is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns><c>true</c> if <see cref="ModelBindingContext"/> was present; otherwise <c>false</c>.</returns>
        public static bool TryGetBinder(this HttpActionContext actionContext, ModelBindingContext bindingContext, out IModelBinder binder)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (bindingContext == null)
            {
                throw Error.ArgumentNull("bindingContext");
            }

            binder = null;
            ModelBinderProvider providerFromAttr;
            if (ModelBindingHelper.TryGetProviderFromAttributes(bindingContext.ModelType, out providerFromAttr))
            {
                binder = providerFromAttr.GetBinder(actionContext, bindingContext);
            }
            else
            {
                binder = actionContext.ControllerContext.Configuration.Services.GetModelBinderProviders()
                    .Select(p => p.GetBinder(actionContext, bindingContext))
                    .Where(b => b != null)
                    .FirstOrDefault();
            }

            return binder != null;
        }

        /// <summary>
        /// Gets the <see cref="ModelBindingContext"/> for this <see cref="HttpActionContext"/>.
        /// </summary>
        /// <param name="actionContext">The execution context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>The <see cref="ModelBindingContext"/>.</returns>
        public static IModelBinder GetBinder(this HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            IModelBinder binder;
            if (TryGetBinder(actionContext, bindingContext, out binder))
            {
                return binder;
            }

            throw Error.InvalidOperation(SRResources.ModelBinderProviderCollection_BinderForTypeNotFound, bindingContext.ModelType);
        }
    }
}
