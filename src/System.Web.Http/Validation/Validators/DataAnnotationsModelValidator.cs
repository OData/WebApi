// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation.Validators
{
    public class DataAnnotationsModelValidator : ModelValidator
    {
        public DataAnnotationsModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute)
            : base(validatorProviders)
        {
            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            Attribute = attribute;
        }

        protected internal ValidationAttribute Attribute { get; private set; }

        public override bool IsRequired
        {
            get { return Attribute is RequiredAttribute; }
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            // Per the WCF RIA Services team, instance can never be null (if you have
            // no parent, you pass yourself for the "instance" parameter).
            ValidationContext context = new ValidationContext(container ?? metadata.Model, null, null);
            context.DisplayName = metadata.GetDisplayName();

            ValidationResult result = Attribute.GetValidationResult(metadata.Model, context);

            if (result != ValidationResult.Success)
            {
                return new ModelValidationResult[] { new ModelValidationResult { Message = result.ErrorMessage } };
            }

            return new ModelValidationResult[0];
        }
    }
}
