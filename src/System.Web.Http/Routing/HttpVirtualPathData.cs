using System.Web.Http.Common;

namespace System.Web.Http.Routing
{
    public class HttpVirtualPathData : IHttpVirtualPathData
    {
        public HttpVirtualPathData(IHttpRoute route, string virtualPath)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (virtualPath == null)
            {
                throw Error.ArgumentNull("virtualPath");
            }

            Route = route;
            VirtualPath = virtualPath;
        }

        public IHttpRoute Route { get; private set; }

        public string VirtualPath { get; private set; }
    }
}
