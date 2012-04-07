// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
