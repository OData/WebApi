using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    public class DynamicViewPage<TModel> : ViewPage<TModel>
    {
        public new dynamic ViewData
        {
            get { return DynamicViewDataDictionary.Wrap(base.ViewData); }
        }
    }
}
