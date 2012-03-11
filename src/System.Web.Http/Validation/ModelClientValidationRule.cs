using System.Collections.Generic;

namespace System.Web.Http.Validation
{
    public class ModelClientValidationRule
    {
        private readonly Dictionary<string, object> _validationParameters = new Dictionary<string, object>(StringComparer.Ordinal);
        private string _validationType;

        public string ErrorMessage { get; set; }

        public IDictionary<string, object> ValidationParameters
        {
            get { return _validationParameters; }
        }

        public string ValidationType
        {
            get { return _validationType ?? String.Empty; }
            set { _validationType = value; }
        }
    }
}
