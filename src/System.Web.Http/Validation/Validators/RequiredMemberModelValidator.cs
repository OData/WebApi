using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Common;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;
using System.Web.Http.Validation.ClientRules;

namespace System.Web.Http.Validation.Validators
{
    /// <summary>
    /// <see cref="ModelValidator"/> for required members.
    /// </summary>
    public class RequiredMemberModelValidator : ModelValidator
    {
        private readonly string _memberName;

        public RequiredMemberModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, string memberName)
            : base(metadata, validatorProviders)
        {
            if (memberName == null)
            {
                throw Error.ArgumentNull("memberName");
            }

            _memberName = memberName;
        }

        public override bool IsRequired
        {
            get
            {
                return true;
            }
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            return new ModelValidationResult[0];
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            string message = Error.Format(SRResources.MissingRequiredMember, _memberName);
            return new[] { new ModelClientValidationRequiredRule(message) };
        }
    }
}
