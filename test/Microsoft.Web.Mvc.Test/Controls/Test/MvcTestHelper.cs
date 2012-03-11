using System.IO;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public static class MvcTestHelper
    {
        public static string GetControlRendering(Control c, bool designMode)
        {
            if (designMode)
            {
                c.Site = new DesignModeSite();
            }
            HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
            c.RenderControl(writer);
            return writer.InnerWriter.ToString();
        }
    }
}
