// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProvider : NameValuePairsValueProvider
    {
        public RouteDataValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(() => GetRoutes(actionContext.ControllerContext.RouteData), culture)
        {
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetRoutes(IHttpRouteData routeData)
        {
            foreach (KeyValuePair<string, object> pair in routeData.Values)
            {
                yield return new KeyValuePair<string, string>(pair.Key, pair.Value.ToString());
            }
        }
    }
}
