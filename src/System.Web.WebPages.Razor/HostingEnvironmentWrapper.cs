using System.Web.Hosting;

namespace System.Web.WebPages.Razor
{
    internal sealed class HostingEnvironmentWrapper : IHostingEnvironment
    {
        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}
