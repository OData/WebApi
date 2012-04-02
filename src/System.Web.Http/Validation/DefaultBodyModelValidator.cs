using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Recursively validate an object. 
    /// </summary>
    public class DefaultBodyModelValidator : IBodyModelValidator
    {
        /// <summary>
        /// Determines whether the <paramref name="model"/> is valid and adds any validation errors to the <paramref name="actionContext"/>'s <see cref="ModelStateDictionary"/>
        /// </summary>
        /// <param name="model">The model to be validated.</param>
        /// <param name="type">The <see cref="Type"/> to use for validation.</param>
        /// <param name="metadataProvider">The <see cref="ModelMetadataProvider"/> used to provide the model metadata.</param>
        /// <param name="actionContext">The <see cref="HttpActionContext"/> within which the model is being validated.</param>
        /// <param name="keyPrefix">The <see cref="string"/> to append to the key for any validation errors.</param>
        /// <returns><c>true</c>if <paramref name="model"/> is valid, <c>false</c> otherwise.</returns>
        public bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (metadataProvider == null)
            {
                throw Error.ArgumentNull("metadataProvider");
            }

            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (model != null && MediaTypeFormatterCollection.IsTypeExcludedFromValidation(model.GetType()))
            {
                // no validation for some DOM like types
                return true;
            }

            IEnumerable<ModelValidatorProvider> validatorProviders = actionContext.GetValidatorProviders();
            // Optimization : avoid validating the object graph if there are no validator providers
            if (validatorProviders == null || !validatorProviders.Any())
            {
                return true;
            }

            ModelMetadata metadata = metadataProvider.GetMetadataForType(() => model, type);
            ValidationContext validationContext = new ValidationContext()
            {
                MetadataProvider = metadataProvider,
                ValidatorProviders = validatorProviders,
                ModelState = actionContext.ModelState,
                Visited = new HashSet<object>()
            };
            return ValidateNodeAndChildren(metadata, validationContext, container: null, prefix: keyPrefix);
        }

        private static bool ValidateNodeAndChildren(ModelMetadata metadata, ValidationContext validationContext, object container, string prefix)
        {            
            object model = metadata.Model;
            bool isValid = true;

            if (model == null)
            {
                return ShallowValidate(metadata, validationContext, container, prefix);
            }

            // Check to avoid infinite recursion. This can happen with cycles in an object graph.
            if (validationContext.Visited.Contains(model))
            {
                return true;
            }
            validationContext.Visited.Add(model);

            // Validate the children first - depth-first traversal
            IEnumerable enumerableModel = TypeHelper.GetAsEnumerable(model);
            if (enumerableModel == null)
            {
                isValid = ValidateProperties(metadata, validationContext, prefix);
            }
            else
            {
                isValid = ValidateElements(enumerableModel, validationContext, prefix);
            }
            if (isValid)
            {
                // Don't bother to validate this node if children failed.
                isValid = ShallowValidate(metadata, validationContext, container, prefix);
            }

            // Pop the object so that it can be validated again in a different path
            validationContext.Visited.Remove(model);

            return isValid;
        }

        private static bool ValidateProperties(ModelMetadata metadata, ValidationContext validationContext, string prefix)
        {
            bool isValid = true;
            foreach (ModelMetadata childMetadata in metadata.Properties)
            {
                string childPrefix = ModelBindingHelper.CreatePropertyModelName(prefix, childMetadata.PropertyName);
                if (!ValidateNodeAndChildren(childMetadata, validationContext, metadata.Model, childPrefix))
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        private static bool ValidateElements(IEnumerable model, ValidationContext validationContext, string prefix)
        {
            bool isValid = true;
            int index = 0;
            Type elementType = GetElementType(model.GetType());

            foreach (object element in model)
            {
                string elementPrefix = ModelBindingHelper.CreateIndexModelName(prefix, index++);
                ModelMetadata elementMetadata = validationContext.MetadataProvider.GetMetadataForType(() => element, elementType);
                if (!ValidateNodeAndChildren(elementMetadata, validationContext, model, elementPrefix))
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        // Validates a single node (not including children)
        // Returns true if validation passes successfully
        private static bool ShallowValidate(ModelMetadata metadata, ValidationContext validationContext, object container, string key)
        {
            bool isValid = true;
            foreach (ModelValidator validator in metadata.GetValidators(validationContext.ValidatorProviders))
            {
                foreach (ModelValidationResult error in validator.Validate(container))
                {
                    validationContext.ModelState.AddModelError(key, error.Message);
                    isValid = false;
                }
            }
            return isValid;
        }

        private static Type GetElementType(Type type)
        {
            Contract.Assert(typeof(IEnumerable).IsAssignableFrom(type));
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            foreach (Type implementedInterface in type.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return implementedInterface.GetGenericArguments()[0];
                }
            }

            return typeof(object);
        }

        private class ValidationContext
        {
            public ModelMetadataProvider MetadataProvider { get; set; }
            public IEnumerable<ModelValidatorProvider> ValidatorProviders { get; set; }
            public ModelStateDictionary ModelState { get; set; }
            public HashSet<object> Visited { get; set; }
        }
    }
}
