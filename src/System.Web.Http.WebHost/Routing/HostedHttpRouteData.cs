// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpRouteData : IHttpRouteData
    {
        private readonly RouteData _routeData;
        private readonly HostedHttpRoute _hostedHttpRoute;

        public HostedHttpRouteData(RouteData routeData)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            _routeData = routeData;
            _hostedHttpRoute = new HostedHttpRoute(_routeData.Route as Route);
        }

        public IHttpRoute Route
        {
            get { return _hostedHttpRoute; }
        }

        public IDictionary<string, object> Values
        {
            get { return _routeData.Values; }
        }

        internal RouteData OriginalRouteData
        {
            get { return _routeData; }
        }
    }
}
