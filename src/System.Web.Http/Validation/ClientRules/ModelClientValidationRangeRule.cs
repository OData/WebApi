namespace System.Web.Http.Validation.ClientRules
{
    public class ModelClientValidationRangeRule : ModelClientValidationRule
    {
        public ModelClientValidationRangeRule(string errorMessage, object minValue, object maxValue)
        {
            ErrorMessage = errorMessage;
            ValidationType = "range";
            ValidationParameters["min"] = minValue;
            ValidationParameters["max"] = maxValue;
        }
    }
}
