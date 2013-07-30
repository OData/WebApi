// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    internal static class HttpRouteDataExtensions
    {
        // If routeData is from an attribute route, get the controller that can handle it. 
        // Else return null.
        public static HttpControllerDescriptor GetDirectRouteController(this IHttpRouteData routeData)
        {
            ReflectedHttpActionDescriptor[] directRouteActions = routeData.GetDirectRouteActions();
            if (directRouteActions != null)
            {
                // Set the controller descriptor for the first action descriptor
                Contract.Assert(directRouteActions.Length > 0);
                HttpControllerDescriptor controllerDescriptor = directRouteActions[0].ControllerDescriptor;

                // Check that all other action descriptors share the same controller descriptor
                for (int i = 1; i < directRouteActions.Length; i++)
                {
                    if (directRouteActions[i].ControllerDescriptor != controllerDescriptor)
                    {
                        return null;
                    }
                }

                return controllerDescriptor;
            }

            return null;
        }

        public static IEnumerable<IHttpRouteData> GetSubRoutes(this IHttpRouteData routeData)
        {
            IHttpRouteData[] subRoutes = null;
            if (routeData.Values.TryGetValue(RouteCollectionRoute.SubRouteDataKey, out subRoutes))
            {
                return subRoutes;
            }
            return null;
        }

        // If routeData is from an attribute route, get the action descriptors that it may match to.
        // Caller still needs to run action selection to pick the specific action.
        // Else return null.
        public static ReflectedHttpActionDescriptor[] GetDirectRouteActions(this IHttpRouteData routeData)
        {
            IEnumerable<IHttpRouteData> subRoutes = routeData.GetSubRoutes();
            if (subRoutes == null)
            {
                // Possible this is being called on a subroute. This can happen after ElevateRouteData. Just chain. 
                return routeData.Route.GetDirectRouteActions();
            }

            var list = new List<ReflectedHttpActionDescriptor>();

            foreach (IHttpRouteData subData in subRoutes)
            {
                ReflectedHttpActionDescriptor[] actionDescriptors = subData.Route.GetDirectRouteActions();
                if (actionDescriptors != null)
                {
                    list.AddRange(actionDescriptors);
                }
            }
            return list.ToArray();
        }
    }
}