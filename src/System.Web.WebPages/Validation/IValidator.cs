using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace System.Web.WebPages
{
    public interface IValidator
    {
        ModelClientValidationRule ClientValidationRule { get; }
        ValidationResult Validate(ValidationContext validationContext);
    }
}
