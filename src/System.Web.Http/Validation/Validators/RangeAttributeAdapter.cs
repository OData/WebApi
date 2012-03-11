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
    public class RangeAttributeAdapter : DataAnnotationsModelValidator<RangeAttribute>
    {
        public RangeAttributeAdapter(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, RangeAttribute attribute)
            : base(metadata, validatorProviders, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            string errorMessage = ErrorMessage; // Per Dev10 Bug #923283, need to make sure ErrorMessage is called before Minimum/Maximum
            return new[] { new ModelClientValidationRangeRule(errorMessage, Attribute.Minimum, Attribute.Maximum) };
        }
    }
}
