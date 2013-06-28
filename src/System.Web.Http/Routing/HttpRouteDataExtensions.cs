// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    internal static class HttpRouteDataExtensions
    {
        public static ReflectedHttpActionDescriptor[] GetDirectRouteActions(this IHttpRouteData routeData)
        {
            Contract.Assert(routeData != null);

            IHttpRoute route = routeData.Route;
            if (route == null)
            {
                return null;
            }

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
