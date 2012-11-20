// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing.Conventions;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpControllerSelector"/> that uses the server's OData routing conventions to select a controller for OData requests.
    /// </summary>
    public class ODataControllerSelector : DefaultHttpControllerSelector
    {
        private IList<IODataRoutingConvention> _routingConventions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataControllerSelector" /> class.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public ODataControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
            _routingConventions = configuration.GetODataRoutingConventions();
        }

        /// <summary>
        /// Gets the name of the controller for the specified <see cref="HttpRequestMessage" />.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>
        /// The name of the controller for the specified <see cref="HttpRequestMessage" />.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Response disposed later")]
        public override string GetControllerName(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ODataPath odataPath = request.GetODataPath();
            if (odataPath == null)
            {
                return base.GetControllerName(request);
            }

            foreach (IODataRoutingConvention routingConvention in _routingConventions)
            {
                string controllerName = routingConvention.SelectController(odataPath, request);
                if (controllerName != null)
                {
                    return controllerName;
                }
            }

            throw new HttpResponseException(request.CreateErrorResponse(
                HttpStatusCode.NotFound,
                Error.Format(SRResources.NoMatchingResource, request.RequestUri),
                Error.Format(SRResources.NoRoutingHandlerToSelectController, odataPath.PathTemplate)));
        }
    }
}
