// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Adapts a named direct route into a top-level entry in the route table that can only be used
    /// for generating a link (GetVirtualPath). We use these because the subroutes produced by direct
    /// routing, don't go into the main collection and so can't be matched by name.
    /// </summary>
    internal class GenerationRoute : RouteBase
    {
        public GenerationRoute(string name, Route route)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw Error.ParameterCannotBeNullOrEmpty("name");
            }
            
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            Name = name;
            Route = route;
        }

        public string Name { get; private set; }

        public Route Route { get; private set; }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return Route.GetVirtualPath(requestContext, values);
        }
    }
}
