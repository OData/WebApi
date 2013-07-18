// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    internal static class HttpRouteExtensions
    {
        public static HttpControllerDescriptor GetDirectRouteController(this IHttpRoute route)
        {
            Contract.Assert(route != null);

            ReflectedHttpActionDescriptor[] directRouteActions = route.GetDirectRouteActions();
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
