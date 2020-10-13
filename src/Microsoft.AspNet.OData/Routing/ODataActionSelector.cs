﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.AspNet.OData.Routing
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
            IEnumerable<IODataRoutingConvention> routingConventions = request.GetRoutingConventions();
            IHttpRouteData routeData = controllerContext.RouteData;

            if (odataPath == null || routingConventions == null || routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                return _innerSelector.SelectAction(controllerContext);
            }

            ILookup<string, HttpActionDescriptor> actionMap = _innerSelector.GetActionMapping(controllerContext.ControllerDescriptor);

            foreach (IODataRoutingConvention routingConvention in routingConventions)
            {
                string actionName = routingConvention.SelectAction(
                    odataPath,
                    controllerContext,
                    actionMap);

                if (actionName != null)
                {
                    routeData.Values[ODataRouteConstants.Action] = actionName;
                    var action = _innerSelector.SelectAction(controllerContext);
                    if (ActionParametersMatchRequest(action, controllerContext))
                    {
                        return action;
                    }
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

        private static bool ActionParametersMatchRequest(HttpActionDescriptor action, HttpControllerContext context)
        {
            var parameters = action.GetParameters();
            var routeData = context.RouteData;
            var routingConventionsStore = context.Request.ODataProperties().RoutingConventionsStore;

            int keyCount = 0;
            if (routingConventionsStore.ContainsKey(ODataRouteConstants.KeyCount))
            {
                keyCount = (int)routingConventionsStore[ODataRouteConstants.KeyCount];
            }

            // navigationProperty is optional in some cases, therefore an action
            // should not be rejected simply because it does not declare a navigationProperty parameter
            if (routeData.Values.ContainsKey(ODataRouteConstants.NavigationProperty))
            {
                keyCount -= 1;
            }

            if (parameters.Count < keyCount)
            {
                return false;
            }

            return true;
        }
    }
}
