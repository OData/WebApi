// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    internal class HttpRouteEntry
    {
        private readonly string _name;
        private readonly IHttpRoute _route;

        public HttpRouteEntry(string name, IHttpRoute route)
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

        public IHttpRoute Route
        {
            get { return _route; }
        }
    }
}
