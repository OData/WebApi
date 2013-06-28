// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Routing;

namespace System.Web.Routing
{
    public class DirectRouteTestHelpers
    {
        public static Route[] BuildDirectRouteStubsFrom<T>(Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            var attributes = method.GetCustomAttributes(false).OfType<IDirectRouteInfoProvider>();

            return attributes.Select(attr =>
                {
                    var route = new Route(attr.RouteTemplate, routeHandler: null);
                    route.SetTargetActionMethod(method);
                    return route;
                }).ToArray();
        }
    }
}