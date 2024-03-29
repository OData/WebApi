//-----------------------------------------------------------------------------
// <copyright file="ODataPathRouteConstraint.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public partial class ODataPathRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="routeKey">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>
        /// True if this instance equals a specified route; otherwise, false.
        /// </returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public virtual bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            if (routeDirection == RouteDirection.IncomingRequest)
            {
                ODataPath path = null;
                HttpRequest request = httpContext.Request;

                object oDataPathValue;
                if (values.TryGetValue(ODataRouteConstants.ODataPath, out oDataPathValue))
                {
                    string oDataPath = oDataPathValue as string;
                    // Create request container
                    request.CreateRequestContainer(RouteName);

                    // Check whether the request is a POST targeted at a resource path ending in /$query
                    if (request.IsQueryRequest(oDataPath))
                    {
                        request.TransformQueryRequest();

                        oDataPath = oDataPath.Substring(0, oDataPath.LastIndexOf('/' + ODataRouteConstants.QuerySegment, StringComparison.OrdinalIgnoreCase));
                        values[ODataRouteConstants.ODataPath] = oDataPath;
                    }

                    // We need to call Uri.GetLeftPart(), which returns an encoded Url.
                    // The ODL parser does not like raw values.
                    Uri requestUri = new Uri(request.GetEncodedUrl());
                    string requestLeftPart = requestUri.GetLeftPart(UriPartial.Path);
                    string queryString = request.QueryString.HasValue ? request.QueryString.ToString() : null;

                    path = GetODataPath(oDataPath, requestLeftPart, queryString, () => request.GetRequestContainer());
                }

                if (path != null)
                {
                    // Set all the properties we need for routing, querying, formatting
                    IODataFeature odataFeature = httpContext.ODataFeature();
                    odataFeature.Path = path;
                    odataFeature.RouteName = RouteName;

                    return true;
                }

                // The request doesn't match this route so dispose the request container.
                request.DeleteRequestContainer(true);
                return false;
            }
            else
            {
                // This constraint only applies to incoming request.
                return true;
            }
        }
    }
}
