// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public partial class ODataPathRouteConstraint : IHttpRouteConstraint
    {
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
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public virtual bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
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
                ODataPath path = null;

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

                    string requestLeftPart = request.RequestUri.GetLeftPart(UriPartial.Path);
                    string queryString = request.RequestUri.Query;

                    path = GetODataPath(oDataPath, requestLeftPart, queryString, () => request.GetRequestContainer());
                }

                if (path != null)
                {
                    // Set all the properties we need for routing, querying, formatting
                    HttpRequestMessageProperties properties = request.ODataProperties();
                    properties.Path = path;
                    properties.RouteName = RouteName;

                    if (!values.ContainsKey(ODataRouteConstants.Controller))
                    {
                        // Select controller name using the routing conventions
                        string controllerName = SelectControllerName(path, request);
                        if (controllerName != null)
                        {
                            values[ODataRouteConstants.Controller] = controllerName;
                        }
                    }

                    return true;
                }

                // The request doesn't match this route so dispose the request container.
                request.DeleteRequestContainer(true);
                return false;
            }
            else
            {
                // This constraint only applies to URI resolution
                return true;
            }
        }

        /// <summary>
        /// Selects the name of the controller to dispatch the request to.
        /// </summary>
        /// <param name="path">The OData path of the request.</param>
        /// <param name="request">The request.</param>
        /// <returns>The name of the controller to dispatch to, or <c>null</c> if one cannot be resolved.</returns>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        protected virtual string SelectControllerName(ODataPath path, HttpRequestMessage request)
        {
            foreach (IODataRoutingConvention routingConvention in request.GetRoutingConventions())
            {
                string controllerName = routingConvention.SelectController(path, request);
                if (controllerName != null)
                {
                    return controllerName;
                }
            }

            return null;
        }
    }
}
