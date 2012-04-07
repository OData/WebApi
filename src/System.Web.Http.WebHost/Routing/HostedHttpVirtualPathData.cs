// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpVirtualPathData : IHttpVirtualPathData
    {
        private readonly VirtualPathData _virtualPath;
        private readonly HostedHttpRoute _hostedHttpRoute;

        public HostedHttpVirtualPathData(VirtualPathData virtualPath)
        {
            if (virtualPath == null)
            {
                throw Error.ArgumentNull("route");
            }

            _virtualPath = virtualPath;
            _hostedHttpRoute = new HostedHttpRoute(_virtualPath.Route as Route);
        }

        public IHttpRoute Route
        {
            get { return _hostedHttpRoute; }
        }

        public string VirtualPath
        {
            get { return _virtualPath.VirtualPath; }
        }
    }
}
