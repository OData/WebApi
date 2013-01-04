// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing.Conventions;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpActionSelector"/> that uses the server's OData routing conventions to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : IHttpActionSelector
    {
        private readonly IEnumerable<IODataRoutingConvention> _routingConventions;
        private readonly IHttpActionSelector _innerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="routingConventions">The OData routing conventions to use for OData requests.</param>
        /// <param name="innerSelector">The inner controller selector to call.</param>
        public ODataActionSelector(IEnumerable<IODataRoutingConvention> routingConventions, IHttpActionSelector innerSelector)
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
        /// Returns a map, keyed by action string, of all <see cref="T:System.Web.Http.Controllers.HttpActionDescriptor" /> that the selector can select.  This is primarily called by <see cref="T:System.Web.Http.Description.IApiExplorer" /> to discover all the possible actions in the controller.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>
        /// A map of <see cref="T:System.Web.Http.Controllers.HttpActionDescriptor" /> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="T:System.Web.Http.Controllers.HttpActionDescriptor" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
        {
            return _innerSelector.GetActionMapping(controllerDescriptor);
        }

        /// <summary>
        /// Selects an action for the <see cref="ApiControllerActionSelector" />.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns>
        /// The selected action.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Response disposed later")]
        public HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            HttpRequestMessage request = controllerContext.Request;
            ODataPath odataPath = request.GetODataPath();
            if (odataPath == null)
            {
                return _innerSelector.SelectAction(controllerContext);
            }

            ILookup<string, HttpActionDescriptor> actionMap = _innerSelector.GetActionMapping(controllerContext.ControllerDescriptor);
            foreach (IODataRoutingConvention routingConvention in _routingConventions)
            {
                string actionName = routingConvention.SelectAction(odataPath, controllerContext, actionMap);
                if (actionName != null)
                {
                    controllerContext.RouteData.Values[ODataRouteConstants.Action] = actionName;
                    return _innerSelector.SelectAction(controllerContext);
                }
            }

            throw new HttpResponseException(request.CreateErrorResponse(
                HttpStatusCode.NotFound,
                Error.Format(SRResources.NoMatchingResource, controllerContext.Request.RequestUri),
                Error.Format(SRResources.NoRoutingHandlerToSelectAction, odataPath.PathTemplate)));
        }
    }
}
