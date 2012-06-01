// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    public class UrlHelper
    {
        private HttpRequestMessage _request;

        public UrlHelper(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Request = request;
        }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/> of the current <see cref="UrlHelper"/>.
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _request = value;
            }
        }

        public string Route(string routeName, object routeValues)
        {
            return GetHttpRouteHelper(Request, routeName, routeValues);
        }

        public string Route(string routeName, IDictionary<string, object> routeValues)
        {
            return GetHttpRouteHelper(Request, routeName, routeValues);
        }

        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public string Link(string routeName, object routeValues)
        {
            string link = Route(routeName, routeValues);
            if (!String.IsNullOrEmpty(link))
            {
                link = new Uri(Request.RequestUri, link).AbsoluteUri;
            }

            return link;
        }

        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public string Link(string routeName, IDictionary<string, object> routeValues)
        {
            string link = Route(routeName, routeValues);
            if (!String.IsNullOrEmpty(link))
            {
                link = new Uri(Request.RequestUri, link).AbsoluteUri;
            }

            return link;
        }

        private static string GetHttpRouteHelper(HttpRequestMessage request, string routeName, object routeValues)
        {
            HttpRouteValueDictionary routeValuesDictionary = new HttpRouteValueDictionary(routeValues);
            return GetHttpRouteHelper(request, routeName, routeValuesDictionary);
        }

        private static string GetHttpRouteHelper(HttpRequestMessage request, string routeName, IDictionary<string, object> routeValues)
        {
            if (routeValues == null)
            {
                // If no route values were passed in at all we have to create a new dictionary
                // so that we can add the extra "httproute" key.
                routeValues = new HttpRouteValueDictionary();
                routeValues.Add(HttpRoute.HttpRouteKey, true);
            }
            else
            {
                // Copy the dictionary so that we can guarantee that routeValues uses an OrdinalIgnoreCase comparer
                // and to add the extra "httproute" key used by all Web API routes to disambiguate them from other MVC routes.
                routeValues = new HttpRouteValueDictionary(routeValues);
                if (!routeValues.ContainsKey(HttpRoute.HttpRouteKey))
                {
                    routeValues.Add(HttpRoute.HttpRouteKey, true);
                }
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoConfiguration);
            }

            IHttpVirtualPathData vpd = configuration.Routes.GetVirtualPath(
                request: request,
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
