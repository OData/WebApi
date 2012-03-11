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
    public class RegularExpressionAttributeAdapter : DataAnnotationsModelValidator<RegularExpressionAttribute>
    {
        public RegularExpressionAttributeAdapter(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, RegularExpressionAttribute attribute)
            : base(metadata, validatorProviders, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRegexRule(ErrorMessage, Attribute.Pattern) };
        }
    }
}
