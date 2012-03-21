using System.Collections.Generic;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation.Validators
{
    /// <summary>
    /// <see cref="ModelValidator"/> for required members.
    /// </summary>
    public class RequiredMemberModelValidator : ModelValidator
    {
        public RequiredMemberModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
            : base(metadata, validatorProviders)
        {
        }

        public override bool IsRequired
        {
            get { return true; }
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            return new ModelValidationResult[0];
        }
    }
}
