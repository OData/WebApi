// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpControllerSelector"/> that uses the server's OData routing conventions to select a controller for OData requests.
    /// </summary>
    public class ODataControllerSelector : IHttpControllerSelector
    {
        private readonly IEnumerable<IODataRoutingConvention> _routingConventions;
        private readonly IHttpControllerSelector _innerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataControllerSelector" /> class.
        /// </summary>
        /// <param name="routingConventions">The OData routing conventions to use for OData requests.</param>
        /// <param name="innerSelector">The inner controller selector to call.</param>
        public ODataControllerSelector(IEnumerable<IODataRoutingConvention> routingConventions, IHttpControllerSelector innerSelector)
        {
            if (routingConventions == null)
            {
                throw Error.ArgumentNull("routingConventions");
            }

            if (innerSelector == null)
            {
                throw Error.ArgumentNull("innerSelector");
            }

            _routingConventions = routingConventions;
            _innerSelector = innerSelector;
        }

        /// <summary>
        /// Returns a map, keyed by controller string, of all <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> that the selector can select.  This is primarily called by <see cref="T:System.Web.Http.Description.IApiExplorer" /> to discover all the possible controllers in the system.
        /// </summary>
        /// <returns>
        /// A map of all <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" />.
        /// </returns>
        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _innerSelector.GetControllerMapping();
        }

        /// <summary>
        /// Selects a <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> for the given <see cref="T:System.Net.Http.HttpRequestMessage" />.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>
        /// The <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> instance for the given <see cref="T:System.Net.Http.HttpRequestMessage" />.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Response disposed later")]
        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return _innerSelector.SelectController(request);
            }

            ODataPath odataPath = request.GetODataPath();
            if (odataPath == null)
            {
                return _innerSelector.SelectController(request);
            }

            foreach (IODataRoutingConvention routingConvention in _routingConventions)
            {
                string controllerName = routingConvention.SelectController(odataPath, request);
                if (controllerName != null)
                {
                    routeData.Values.Add(ODataRouteConstants.Controller, controllerName);
                    return _innerSelector.SelectController(request);
                }
            }

            throw new HttpResponseException(request.CreateErrorResponse(
                HttpStatusCode.NotFound,
                Error.Format(SRResources.NoMatchingResource, request.RequestUri),
                Error.Format(SRResources.NoRoutingHandlerToSelectController, odataPath.PathTemplate)));
        }
    }
}
