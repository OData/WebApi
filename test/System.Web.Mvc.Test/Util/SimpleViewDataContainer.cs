using System.Web.Mvc;

namespace Microsoft.Web.UnitTestUtil
{
    public class SimpleViewDataContainer : IViewDataContainer
    {
        public SimpleViewDataContainer(ViewDataDictionary viewData)
        {
            ViewData = viewData;
        }

        public ViewDataDictionary ViewData { get; set; }
    }
}
