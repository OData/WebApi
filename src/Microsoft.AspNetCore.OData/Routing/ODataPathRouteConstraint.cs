// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Text;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
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
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
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

                    StringBuilder requestLeftPartBuilder = new StringBuilder(request.Scheme);
                    requestLeftPartBuilder.Append("://");
                    requestLeftPartBuilder.Append(request.Host.HasValue ? request.Host.Value : request.Host.ToString());
                    requestLeftPartBuilder.Append(request.Path.HasValue ? request.Path.Value : request.Path.ToString());

                    string queryString = request.QueryString.HasValue ? request.QueryString.ToString() : null;

                    path = GetODataPath(oDataPathValue as string, requestLeftPartBuilder.ToString(), queryString, () => request.CreateRequestContainer(RouteName));
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
