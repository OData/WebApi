using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    [Serializable]
    public class ModelState
    {
        private ModelErrorCollection _errors = new ModelErrorCollection();

        public ValueProviderResult Value { get; set; }

        public ModelErrorCollection Errors
        {
            get { return _errors; }
        }
    }
}
