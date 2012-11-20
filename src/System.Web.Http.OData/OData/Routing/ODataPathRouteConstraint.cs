// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.OData.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public class ODataPathRouteConstraint : IHttpRouteConstraint
    {
        private IODataPathHandler _pathHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="pathHandler">The OData path handler to use for parsing.</param>
        public ODataPathRouteConstraint(IODataPathHandler pathHandler)
        {
            _pathHandler = pathHandler;
        }

        /// <summary>
        /// Determines whether this instance equals a specified route.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>
        /// True if this instance equals a specified route; otherwise, false.
        /// </returns>
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            if (routeDirection == HttpRouteDirection.UriResolution)
            {
                object odataPathRouteValue;
                if (values.TryGetValue(ODataRouteConstants.ODataPath, out odataPathRouteValue))
                {
                    string odataPath = odataPathRouteValue as string;
                    if (odataPath == null)
                    {
                        // No odataPath means the path is empty; this is necessary for service documents
                        odataPath = String.Empty;
                    }

                    ODataPath path = _pathHandler.Parse(odataPath);
                    if (path != null)
                    {
                        request.SetODataPath(path);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                // This constraint only applies to URI resolution
                return true;
            }
        }
    }
}
