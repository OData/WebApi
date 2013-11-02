// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if ASPNETWEBAPI
using TRoute = System.Web.Http.Routing.IHttpRoute;
#else
using TRoute = System.Web.Routing.Route;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    internal class RouteEntry
    {
        private readonly string _name;
        private readonly TRoute _route;

        public RouteEntry(string name, TRoute route)
        {
            if (route == null)
            {
                throw new ArgumentNullException("route");
            }

            _name = name;
            _route = route;
        }

        public string Name
        {
            get { return _name; }
        }

        public TRoute Route
        {
            get { return _route; }
        }
    }
}
