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
            CandidateAction[] candidates = routeData.GetDirectRouteCandidates();
            if (candidates != null)
            {
                // Set the controller descriptor for the first action descriptor
                Contract.Assert(candidates.Length > 0);
                Contract.Assert(candidates[0].ActionDescriptor != null);
                HttpControllerDescriptor controllerDescriptor = candidates[0].ActionDescriptor.ControllerDescriptor;

                foreach (CandidateAction candidate in candidates)
                {
                    // Check that all other candidate action descriptors share the same controller descriptor
                    for (int i = 1; i < candidates.Length; i++)
                    {
                        if (candidates[i].ActionDescriptor.ControllerDescriptor != controllerDescriptor)
                        {
                            return null;
                        }
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

        // If routeData is from an attribute route, get the action descriptors, order and precedence that it may match
        // to. Caller still needs to run action selection to pick the specific action.
        // Else return null.
        public static CandidateAction[] GetDirectRouteCandidates(this IHttpRouteData routeData)
        {
            Contract.Assert(routeData != null);
            IEnumerable<IHttpRouteData> subRoutes = routeData.GetSubRoutes();
            if (subRoutes == null)
            {
                // Possible this is being called on a subroute. This can happen after ElevateRouteData. Just chain. 
                return routeData.Route.GetDirectRouteCandidates();
            }

            var list = new List<CandidateAction>();

            foreach (IHttpRouteData subData in subRoutes)
            {
                CandidateAction[] candidates = subData.Route.GetDirectRouteCandidates();
                if (candidates != null)
                {
                    list.AddRange(candidates);
                }
            }
            return list.ToArray();
        }
    }
}