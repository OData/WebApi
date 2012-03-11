using System.Net.Http.Formatting;
using System.Web.Http.Common;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// This <see cref="IFormatterLogger"/> logs formatter errors to the provided <see cref="ModelStateDictionary"/>.
    /// </summary>
    public class ModelStateFormatterLogger : IFormatterLogger
    {
        private readonly ModelStateDictionary _modelState;

        public ModelStateFormatterLogger(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw Error.ArgumentNull("modelState");
            }

            _modelState = modelState;
        }

        public void LogError(string errorPath, string errorMessage)
        {
            if (errorPath == null)
            {
                throw Error.ArgumentNull("errorPath");
            }
            if (errorMessage == null)
            {
                throw Error.ArgumentNull("errorMessage");
            }

            _modelState.AddModelError(errorPath, errorMessage);
        }
    }
}
