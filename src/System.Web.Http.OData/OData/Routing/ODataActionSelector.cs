// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing.Conventions;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpActionSelector"/> that uses the server's OData routing conventions to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : ApiControllerActionSelector
    {
        /// <summary>
        /// Selects an action for the <see cref="ApiControllerActionSelector" />.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns>
        /// The selected action.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Response disposed later")]
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            HttpConfiguration configuration = controllerContext.Configuration;
            HttpRequestMessage request = controllerContext.Request;
            ODataPath odataPath = request.GetODataPath();
            if (odataPath == null)
            {
                return base.SelectAction(controllerContext);
            }

            ILookup<string, HttpActionDescriptor> actionMap = GetActionMapping(controllerContext.ControllerDescriptor);
            foreach (IODataRoutingConvention routingConvention in configuration.GetODataRoutingConventions())
            {
                string actionName = routingConvention.SelectAction(odataPath, controllerContext, actionMap);
                if (actionName != null)
                {
                    controllerContext.RouteData.Values.Add(ODataRouteConstants.Action, actionName);
                    return base.SelectAction(controllerContext);
                }
            }

            throw new HttpResponseException(request.CreateErrorResponse(
                HttpStatusCode.NotFound,
                Error.Format(SRResources.NoMatchingResource, controllerContext.Request.RequestUri),
                Error.Format(SRResources.NoRoutingHandlerToSelectAction, odataPath.PathTemplate)));
        }
    }
}
