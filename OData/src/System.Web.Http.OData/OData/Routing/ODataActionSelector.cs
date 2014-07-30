// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpActionSelector"/> that uses the server's OData routing conventions to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : IHttpActionSelector
    {
        private const string MessageDetailKey = "MessageDetail";
        private readonly IHttpActionSelector _innerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="innerSelector">The inner controller selector to call.</param>
        public ODataActionSelector(IHttpActionSelector innerSelector)
        {
            if (innerSelector == null)
            {
                throw Error.ArgumentNull("innerSelector");
            }

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
            ODataPath odataPath = request.ODataProperties().Path;
            IEnumerable<IODataRoutingConvention> routingConventions = request.ODataProperties().RoutingConventions;
            IHttpRouteData routeData = controllerContext.RouteData;

            if (odataPath == null || routingConventions == null || routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                return _innerSelector.SelectAction(controllerContext);
            }

            ILookup<string, HttpActionDescriptor> actionMap = _innerSelector.GetActionMapping(controllerContext.ControllerDescriptor);
            foreach (IODataRoutingConvention routingConvention in routingConventions)
            {
                string actionName = routingConvention.SelectAction(odataPath, controllerContext, actionMap);
                if (actionName != null)
                {
                    routeData.Values[ODataRouteConstants.Action] = actionName;
                    return _innerSelector.SelectAction(controllerContext);
                }
            }

            throw new HttpResponseException(CreateErrorResponse(request, HttpStatusCode.NotFound,
                Error.Format(SRResources.NoMatchingResource, controllerContext.Request.RequestUri),
                Error.Format(SRResources.NoRoutingHandlerToSelectAction, odataPath.PathTemplate)));
        }

        private static HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message, string messageDetail)
        {
            HttpError error = new HttpError(message);
            if (request.ShouldIncludeErrorDetail())
            {
                error.Add(MessageDetailKey, messageDetail);
            }

            return request.CreateErrorResponse(statusCode, error);
        }
    }
}
