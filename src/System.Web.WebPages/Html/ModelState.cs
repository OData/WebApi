using System.Collections.Generic;

namespace System.Web.WebPages.Html
{
    public class ModelState
    {
        private List<string> _errors = new List<string>();

        public IList<string> Errors
        {
            get { return _errors; }
        }

        public object Value { get; set; }
    }
}
