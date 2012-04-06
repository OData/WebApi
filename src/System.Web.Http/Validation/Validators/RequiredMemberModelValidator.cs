// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation.Validators
{
    /// <summary>
    /// <see cref="ModelValidator"/> for required members.
    /// </summary>
    public class RequiredMemberModelValidator : ModelValidator
    {
        public RequiredMemberModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders)
            : base(validatorProviders)
        {
        }

        public override bool IsRequired
        {
            get { return true; }
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            return new ModelValidationResult[0];
        }
    }
}
