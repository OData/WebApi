// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    public class UrlHelper
    {
        private HttpControllerContext _controllerContext;

        public UrlHelper(HttpControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            ControllerContext = controllerContext;
        }

        /// <summary>
        /// Gets the <see cref="HttpControllerContext"/> of the current <see cref="ApiController"/>.
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public HttpControllerContext ControllerContext
        {
            get { return _controllerContext; }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _controllerContext = value;
            }
        }

        public string Route(string routeName, object routeValues)
        {
            return GetHttpRouteHelper(ControllerContext, routeName, routeValues);
        }

        public string Route(string routeName, IDictionary<string, object> routeValues)
        {
            return GetHttpRouteHelper(ControllerContext, routeName, routeValues);
        }

        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public string Link(string routeName, object routeValues)
        {
            string link = Route(routeName, routeValues);
            if (!String.IsNullOrEmpty(link))
            {
                link = new Uri(ControllerContext.Request.RequestUri, link).AbsoluteUri;
            }

            return link;
        }

        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public string Link(string routeName, IDictionary<string, object> routeValues)
        {
            string link = Route(routeName, routeValues);
            if (!String.IsNullOrEmpty(link))
            {
                link = new Uri(ControllerContext.Request.RequestUri, link).AbsoluteUri;
            }

            return link;
        }

        private static string GetHttpRouteHelper(HttpControllerContext controllerContext, string routeName, object routeValues)
        {
            IDictionary<string, object> routeValuesDictionary = HttpRouteCollection.GetTypeProperties(routeValues);
            return GetHttpRouteHelper(controllerContext, routeName, routeValuesDictionary);
        }

        private static string GetHttpRouteHelper(HttpControllerContext controllerContext, string routeName, IDictionary<string, object> routeValues)
        {
            if (routeValues == null)
            {
                // If no route values were passed in at all we have to create a new dictionary
                // so that we can add the extra "httproute" key.
                routeValues = new Dictionary<string, object>();
                routeValues.Add(HttpRoute.HttpRouteKey, true);
            }
            else
            {
                if (!routeValues.ContainsKey(HttpRoute.HttpRouteKey))
                {
                    // Copy the dictionary so that we can add the extra "httproute" key used by all Web API routes to
                    // disambiguate them from other MVC routes.
                    routeValues = new Dictionary<string, object>(routeValues);
                    routeValues.Add(HttpRoute.HttpRouteKey, true);
                }
            }

            IHttpVirtualPathData vpd = controllerContext.Configuration.Routes.GetVirtualPath(
                controllerContext: controllerContext,
                name: routeName,
                values: routeValues);
            if (vpd == null)
            {
                return null;
            }
            return vpd.VirtualPath;
        }
    }
}
