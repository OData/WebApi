using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    public abstract class ModelValidator
    {
        protected ModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            Metadata = metadata;
            ValidatorProviders = validatorProviders;
        }

        protected internal IEnumerable<ModelValidatorProvider> ValidatorProviders { get; private set; }

        public virtual bool IsRequired
        {
            get { return false; }
        }

        protected internal ModelMetadata Metadata { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public static ModelValidator GetModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            return new CompositeModelValidator(metadata, validatorProviders);
        }

        public abstract IEnumerable<ModelValidationResult> Validate(object container);

        private class CompositeModelValidator : ModelValidator
        {
            public CompositeModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
                : base(metadata, validatorProviders)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
            {
                bool propertiesValid = true;

                foreach (ModelMetadata propertyMetadata in Metadata.Properties)
                {
                    foreach (ModelValidator propertyValidator in propertyMetadata.GetValidators(ValidatorProviders))
                    {
                        foreach (ModelValidationResult propertyResult in propertyValidator.Validate(Metadata.Model))
                        {
                            propertiesValid = false;
                            yield return new ModelValidationResult
                            {
                                MemberName = ModelBindingHelper.CreatePropertyModelName(propertyMetadata.PropertyName, propertyResult.MemberName),
                                Message = propertyResult.Message
                            };
                        }
                    }
                }

                if (propertiesValid)
                {
                    foreach (ModelValidator typeValidator in Metadata.GetValidators(ValidatorProviders))
                    {
                        foreach (ModelValidationResult typeResult in typeValidator.Validate(container))
                        {
                            yield return typeResult;
                        }
                    }
                }
            }
        }
    }
}
