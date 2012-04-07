// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProvider : NameValueCollectionValueProvider
    {
        public RouteDataValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(() => GetRoutes(actionContext.ControllerContext.RouteData), culture)
        {
        }

        internal static NameValueCollection GetRoutes(IHttpRouteData routeData)
        {
            //// REVIEW: better way to map KeyValuePairs into NameValueCollection
            NameValueCollection nameValueCollection = new NameValueCollection();
            foreach (KeyValuePair<string, object> pair in routeData.Values)
            {
                nameValueCollection.Add(pair.Key, pair.Value.ToString());
            }

            return nameValueCollection;
        }
    }
}
