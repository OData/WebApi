using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation.ClientRules;

namespace System.Web.Http.Validation.Validators
{
    // [SecuritySafeCritical] to allow derivation from DataAnnotationsModelValidator<T>
    [SecuritySafeCritical]
    public class StringLengthAttributeAdapter : DataAnnotationsModelValidator<StringLengthAttribute>
    {
        public StringLengthAttributeAdapter(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, StringLengthAttribute attribute)
            : base(metadata, validatorProviders, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationStringLengthRule(ErrorMessage, Attribute.MinimumLength, Attribute.MaximumLength) };
        }
    }
}
