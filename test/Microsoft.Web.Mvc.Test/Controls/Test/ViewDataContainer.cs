using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class ViewDataContainer : Control, IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }
    }
}
