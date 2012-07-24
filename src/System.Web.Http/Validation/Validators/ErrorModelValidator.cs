// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation.Validators
{
    /// <summary>
    /// A <see cref="ModelValidator"/> to represent an error. This validator will always throw an exception regardless of the actual model value.
    /// </summary>
    public class ErrorModelValidator : ModelValidator
    {
        private string _errorMessage;

        public ErrorModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders, string errorMessage) : base(validatorProviders)
        {
            if (errorMessage == null)
            {
                throw Error.ArgumentNull("errorMessage");
            }

            _errorMessage = errorMessage;
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            throw Error.InvalidOperation(_errorMessage);
        }
    }
}
