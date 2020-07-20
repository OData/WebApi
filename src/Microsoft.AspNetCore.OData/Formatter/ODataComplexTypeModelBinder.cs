// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Formatter
{
    internal class ODataComplexTypeModelBinder : IModelBinder
    {
        private Func<object> _modelCreator;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw Error.ArgumentNull("bindingContext");
            }

            if (bindingContext.ModelMetadata == null)
            {
                throw Error.Argument("bindingContext", SRResources.ModelBinderUtil_ModelMetadataCannotBeNull);
            }

            var propertyData = CanCreateModel(bindingContext);
            if (propertyData == 0)
            {
                return Task.CompletedTask;
            }

            return BindModelCoreAsync(bindingContext, propertyData);
        }
        private async Task BindModelCoreAsync(ModelBindingContext bindingContext, int propertyData)
        {
            // Create model first (if necessary) to avoid reporting errors about properties when activation fails.
            if (bindingContext.Model == null)
            {
                bindingContext.Model = CreateModel(bindingContext);
            }

            var modelMetadata = bindingContext.ModelMetadata;
            var propertyBindingSucceeded = false;
            for (var i = 0; i < modelMetadata.Properties.Count; i++)
            {
                var property = modelMetadata.Properties[i];
                if (!CanBindProperty(bindingContext, property))
                {
                    continue;
                }

                var fieldName = property.BinderModelName ?? property.PropertyName;
                var modelName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, fieldName);
                var result = await BindProperty(bindingContext, property, fieldName, modelName);

                if (result.IsModelSet)
                {
                    propertyBindingSucceeded = true;
                }
            }

            if (!bindingContext.IsTopLevelObject &&
                !propertyBindingSucceeded &&
                propertyData == 1)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
        }

        internal int CanCreateModel(ModelBindingContext bindingContext)
        {
            var isTopLevelObject = bindingContext.IsTopLevelObject;        
            // the creation of top level object (this is also required for ModelBinderAttribute to work.)
            if (!isTopLevelObject)
            {
                return 0;
            }

            // Create the object if:
            // 1. It is a top level model.
            if (isTopLevelObject)
            {
                return 2;
            }
            // 2. Any of the model properties can be bound.
            return CanBindAnyModelProperties(bindingContext);
        }

        private int CanBindAnyModelProperties(ModelBindingContext bindingContext)
        {
            // If there are no properties on the model, there is nothing to bind. We are here means this is not a top
            // level object. So we return false.
            if (bindingContext.ModelMetadata.Properties.Count == 0)
            {
                return 0;
            }

            for (var i = 0; i < bindingContext.ModelMetadata.Properties.Count; i++)
            {
                var propertyMetadata = bindingContext.ModelMetadata.Properties[i];
                if (!CanBindProperty(bindingContext, propertyMetadata))
                {
                    continue;
                }

                var fieldName = propertyMetadata.BinderModelName ?? propertyMetadata.PropertyName;
                var modelName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, fieldName);
                using (bindingContext.EnterNestedScope(
                    modelMetadata: propertyMetadata,
                    fieldName: fieldName,
                    modelName: modelName,
                    model: null))
                {
                    // If any property can be bound from a value provider, then success.
                    if (bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                    {
                        return 2;
                    }
                }
            }
            return 0;
        }

        protected virtual bool CanBindProperty(ModelBindingContext bindingContext, ModelMetadata propertyMetadata)
        {
            var metadataProviderFilter = bindingContext.ModelMetadata.PropertyFilterProvider?.PropertyFilter;
            if (metadataProviderFilter?.Invoke(propertyMetadata) == false)
            {
                return false;
            }

            if (bindingContext.PropertyFilter?.Invoke(propertyMetadata) == false)
            {
                return false;
            }

            if (!propertyMetadata.IsBindingAllowed)
            {
                return false;
            }

            if (!CanUpdatePropertyInternal(propertyMetadata))
            {
                return false;
            }

            return true;
        }

        internal static bool CanUpdatePropertyInternal(ModelMetadata propertyMetadata)
        {
            return !propertyMetadata.IsReadOnly || CanUpdateReadOnlyProperty(propertyMetadata.ModelType);
        }

        private static bool CanUpdateReadOnlyProperty(Type propertyType)
        {
            // properties that are marked readonly.
            if (propertyType.GetTypeInfo().IsValueType)
            {
                return false;
            }

            // Arrays are strange beasts since their contents are mutable but their sizes aren't.
            // Therefore we shouldn't even try to update these. Further reading:
            // http://blogs.msdn.com/ericlippert/archive/2008/09/22/arrays-considered-somewhat-harmful.aspx
            if (propertyType.IsArray)
            {
                return false;
            }

            // Special-case known immutable reference types
            if (propertyType == typeof(string))
            {
                return false;
            }

            return true;
        }

        protected virtual Task BindProperty(ModelBindingContext bindingContext)
        {
            return new ODataModelBinder().BindModelAsync(bindingContext);
        }

        private async Task<ModelBindingResult> BindProperty(
    ModelBindingContext bindingContext,
    ModelMetadata property,
    string fieldName,
    string modelName)
        {
            // Pass complex (including collection) values down so that binding system does not unnecessarily
            // recreate instances or overwrite inner properties that are not bound. No need for this with simple
            // values because they will be overwritten if binding succeeds. Arrays are never reused because they
            // cannot be resized.
            object propertyModel = null;
            if (property.PropertyGetter != null &&
                property.IsComplexType &&
                !property.ModelType.IsArray)
            {
                propertyModel = property.PropertyGetter(bindingContext.Model);
            }

            ModelBindingResult result;
            using (bindingContext.EnterNestedScope(
                modelMetadata: property,
                fieldName: fieldName,
                modelName: modelName,
                model: propertyModel))
            {
                await BindProperty(bindingContext);
                result = bindingContext.Result;              
            }

            if (result.IsModelSet)
            {
                SetProperty(bindingContext, modelName, property, result);
            }
            else if (property.IsBindingRequired)
            {
                var message = property.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(fieldName);
                bindingContext.ModelState.TryAddModelError(modelName, message);
            }

            return result;
        }

        /// <summary>
        /// Creates suitable <see cref="object"/> for given <paramref name="bindingContext"/>.
        /// </summary>
        /// <param name="bindingContext">The <see cref="ModelBindingContext"/>.</param>
        /// <returns>An <see cref="object"/> compatible with <see cref="ModelBindingContext.ModelType"/>.</returns>
        protected virtual object CreateModel(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (_modelCreator == null)
            {
                _modelCreator = Expression
                          .Lambda<Func<object>>(Expression.New(bindingContext.ModelType))
                          .Compile();
            }

            // If model creator throws an exception, we want to propagate it back up the call stack, since the
            // application developer should know that this was an invalid type to try to bind to.
            return _modelCreator();
        }

        protected virtual void SetProperty(
         ModelBindingContext bindingContext,
         string modelName,
         ModelMetadata propertyMetadata,
         ModelBindingResult result)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (modelName == null)
            {
                throw new ArgumentNullException(nameof(modelName));
            }

            if (propertyMetadata == null)
            {
                throw new ArgumentNullException(nameof(propertyMetadata));
            }

            if (!result.IsModelSet)
            {
                // If we don't have a value, don't set it on the model and trounce a pre-initialized value.
                return;
            }

            if (propertyMetadata.IsReadOnly)
            {
                // The property should have already been set when we called BindPropertyAsync, so there's
                // nothing to do here.
                return;
            }

            var value = result.Model;
            try
            {
                propertyMetadata.PropertySetter(bindingContext.Model, value);
            }
            catch (Exception exception)
            {
                AddModelError(exception, modelName, bindingContext);
            }
        }

        private static void AddModelError(
            Exception exception,
            string modelName,
            ModelBindingContext bindingContext)
        {
            var targetInvocationException = exception as TargetInvocationException;
            if (targetInvocationException?.InnerException != null)
            {
                exception = targetInvocationException.InnerException;
            }

            // Do not add an error message if a binding error has already occurred for this property.
            var modelState = bindingContext.ModelState;
            var validationState = modelState.GetFieldValidationState(modelName);
            if (validationState == ModelValidationState.Unvalidated)
            {
                modelState.AddModelError(modelName, exception, bindingContext.ModelMetadata);
            }
        }
    }
}
