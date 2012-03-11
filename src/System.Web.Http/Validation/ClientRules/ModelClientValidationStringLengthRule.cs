namespace System.Web.Http.Validation.ClientRules
{
    public class ModelClientValidationStringLengthRule : ModelClientValidationRule
    {
        public ModelClientValidationStringLengthRule(string errorMessage, int minimumLength, int maximumLength)
        {
            ErrorMessage = errorMessage;
            ValidationType = "length";

            if (minimumLength != 0)
            {
                ValidationParameters["min"] = minimumLength;
            }

            if (maximumLength != Int32.MaxValue)
            {
                ValidationParameters["max"] = maximumLength;
            }
        }
    }
}
