using System.ComponentModel;
using System.Web.WebPages.Razor;

namespace Microsoft.Web.WebPages.OAuth
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        public static void Start()
        {
            WebPageRazorHost.AddGlobalImport("DotNetOpenAuth.AspNet");
            WebPageRazorHost.AddGlobalImport("Microsoft.Web.WebPages.DotNetOpenAuth");
        }
    }
}