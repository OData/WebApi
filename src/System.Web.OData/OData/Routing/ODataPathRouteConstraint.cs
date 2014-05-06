// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public class ODataPathRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="pathHandler">The OData path handler to use for parsing.</param>
        /// <param name="model">The EDM model to use for parsing the path.</param>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        /// <param name="routingConventions">The OData routing conventions to use for selecting the controller name.</param>
        public ODataPathRouteConstraint(IODataPathHandler pathHandler, IEdmModel model, string routeName, IEnumerable<IODataRoutingConvention> routingConventions)
        {
            if (pathHandler == null)
            {
                throw Error.ArgumentNull("pathHandler");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            if (routingConventions == null)
            {
                throw Error.ArgumentNull("routingConventions");
            }

            PathHandler = pathHandler;
            EdmModel = model;
            RouteName = routeName;
            RoutingConventions = new Collection<IODataRoutingConvention>(routingConventions.ToList());
        }

        /// <summary>
        /// Gets the OData path handler to use for parsing.
        /// </summary>
        public IODataPathHandler PathHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the EDM model to use for parsing the path.
        /// </summary>
        public IEdmModel EdmModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the route this constraint is associated with.
        /// </summary>
        public string RouteName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the OData routing conventions to use for selecting the controller name.
        /// </summary>
        public Collection<IODataRoutingConvention> RoutingConventions
        {
            get;
            private set;
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed after being sent.")]
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
                object odataPathRouteValue;
                if (values.TryGetValue(ODataRouteConstants.ODataPath, out odataPathRouteValue))
                {
                    string pathAndQuery = odataPathRouteValue as string;
                    if (pathAndQuery == null)
                    {
                        // No odataPath means the path is empty; this is necessary for service documents
                        pathAndQuery = String.Empty;
                    }
                    else if (!String.IsNullOrEmpty(request.RequestUri.Query))
                    {
                        // Ensure path handler receives the query string as well as the path.
                        pathAndQuery += request.RequestUri.Query;
                    }

                    ODataPath path;
                    try
                    {
                        string serviceRoot = GetServiceRoot(request, route, values);
                        path = PathHandler.Parse(EdmModel, serviceRoot, pathAndQuery);
                    }
                    catch (ODataException)
                    {
                        path = null;
                    }

                    if (path != null)
                    {
                        // Set all the properties we need for routing, querying, formatting
                        request.ODataProperties().Model = EdmModel;
                        request.ODataProperties().PathHandler = PathHandler;
                        request.ODataProperties().Path = path;
                        request.ODataProperties().RouteName = RouteName;
                        request.ODataProperties().RoutingConventions = RoutingConventions;

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
                }
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
        protected virtual string SelectControllerName(ODataPath path, HttpRequestMessage request)
        {
            foreach (IODataRoutingConvention routingConvention in RoutingConventions)
            {
                string controllerName = routingConvention.SelectController(path, request);
                if (controllerName != null)
                {
                    return controllerName;
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Dispose on values doesn't do anything by default.")]
        private string GetServiceRoot(HttpRequestMessage request, IHttpRoute route, IDictionary<string, object> values)
        {
            var newRequest = new HttpRequestMessage(request.Method, request.RequestUri);
            newRequest.SetRouteData(new HttpRouteData(route, new HttpRouteValueDictionary(values)));
            var httpRouteCollection = new HttpRouteCollection { { RouteName, route } };
            newRequest.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var urlHelper = new UrlHelper(newRequest);
            return urlHelper.CreateODataLink(RouteName, PathHandler, new List<ODataPathSegment>());
        }
    }
}
