// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
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
