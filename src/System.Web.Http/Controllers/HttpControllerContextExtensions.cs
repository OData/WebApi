// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Contains information for a single HTTP operation.
    /// </summary>
    internal static class HttpControllerContextExtensions
    {
        // Given an action, update the Controller Context's route data to use that action.
        // If you call action selection again after this method, it should still return the same result. 
        internal static void ElevateRouteData(this HttpControllerContext controllerContext, HttpActionDescriptor actionDescriptorSelected)
        {
            IHttpRouteData routeData = controllerContext.RouteData;

            IEnumerable<IHttpRouteData> multipleRouteData = routeData.GetSubRoutes();
            if (multipleRouteData == null)
            {
                return;
            }

            foreach (IHttpRouteData subData in multipleRouteData)
            {
                CandidateAction[] candidates = subData.Route.GetDirectRouteCandidates();

                if (candidates != null)
                {
                    foreach (CandidateAction candidate in candidates)
                    {
                        if (candidate.ActionDescriptor.Equals(actionDescriptorSelected))
                        {
                            controllerContext.RouteData = subData;
                            return;
                        }
                    }
                }
            }
        }
    }
}