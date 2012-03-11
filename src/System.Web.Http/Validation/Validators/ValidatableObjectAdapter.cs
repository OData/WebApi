using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;

namespace System.Web.Http.Validation.Validators
{
    public class ValidatableObjectAdapter : ModelValidator
    {
        public ValidatableObjectAdapter(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
            : base(metadata, validatorProviders)
        {
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            // NOTE: Container is never used here, because IValidatableObject doesn't give you
            // any way to get access to your container.

            object model = Metadata.Model;
            if (model == null)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }

            IValidatableObject validatable = model as IValidatableObject;
            if (validatable == null)
            {
                throw Error.InvalidOperation(SRResources.ValidatableObjectAdapter_IncompatibleType, model.GetType());
            }

            ValidationContext validationContext = new ValidationContext(validatable, null, null);
            return ConvertResults(validatable.Validate(validationContext));
        }

        private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
        {
            foreach (ValidationResult result in results)
            {
                if (result != ValidationResult.Success)
                {
                    if (result.MemberNames == null || !result.MemberNames.Any())
                    {
                        yield return new ModelValidationResult { Message = result.ErrorMessage };
                    }
                    else
                    {
                        foreach (string memberName in result.MemberNames)
                        {
                            yield return new ModelValidationResult { Message = result.ErrorMessage, MemberName = memberName };
                        }
                    }
                }
            }
        }
    }
}
