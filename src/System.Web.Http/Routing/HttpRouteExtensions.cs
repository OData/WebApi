// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    internal static class HttpRouteExtensions
    {
        // If route is a direct route, get the http method for its actions.
        // Else return null.
        public static HttpMethod GetDirectRouteVerb(this IHttpRoute route)
        {
            ReflectedHttpActionDescriptor[] ads = route.GetDirectRouteActions();
            if (ads != null)
            {
                // All action descriptors on this route have the same method, so just pull the first. 
                return ads[0].SupportedHttpMethods[0];
            }
            return null;
        }

        // If route is a direct route, get the action descriptors it may map to.
        public static ReflectedHttpActionDescriptor[] GetDirectRouteActions(this IHttpRoute route)
        {
            Contract.Assert(route != null);

            IDictionary<string, object> dataTokens = route.DataTokens;
            if (dataTokens == null)
            {
                return null;
            }

            ReflectedHttpActionDescriptor[] directRouteActions;
            if (dataTokens.TryGetValue<ReflectedHttpActionDescriptor[]>(RouteKeys.ActionsDataTokenKey, out directRouteActions))
            {
                if (directRouteActions != null && directRouteActions.Length > 0)
                {
                    return directRouteActions;
                }
            }

            return null;
        }
    }
}
