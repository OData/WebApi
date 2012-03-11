namespace System.Web.Http.Validation.ClientRules
{
    public class ModelClientValidationRegexRule : ModelClientValidationRule
    {
        public ModelClientValidationRegexRule(string errorMessage, string pattern)
        {
            ErrorMessage = errorMessage;
            ValidationType = "regex";
            ValidationParameters.Add("pattern", pattern);
        }
    }
}
