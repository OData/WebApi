namespace System.Web.Http.Validation.ClientRules
{
    public class ModelClientValidationRequiredRule : ModelClientValidationRule
    {
        public ModelClientValidationRequiredRule(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ValidationType = "required";
        }
    }
}
