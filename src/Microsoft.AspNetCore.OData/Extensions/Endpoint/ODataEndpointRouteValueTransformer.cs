// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETSTANDARD2_0
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    ///  Provides an abstraction for dynamically manipulating route value to select an OData controller action.
    /// </summary>
    public class ODataEndpointRouteValueTransformer : DynamicRouteValueTransformer
    {
        private IActionSelector _selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEndpointRouteValueTransformer"/> class.
        /// </summary>
        /// <param name="actionSelector">The injected <see cref="IActionSelector"/>.</param>
        public ODataEndpointRouteValueTransformer(IActionSelector actionSelector)
        {
            _selector = actionSelector;
        }

        /// <summary>
        /// Creates a set of transformed route values that will be used to select an action.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
        /// <param name="values">The route values associated with the current match.</param>
        /// <returns>A task which asynchronously returns a set of route values.</returns>
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            if (values == null)
            {
                throw Error.ArgumentNull(nameof(values));
            }

            if (httpContext.ODataFeature().Path != null)
            {
                // Noted: if there's a route mapping with ODataPrefix == null,
                // for example: router.MapODataRoute(routeName: "odata", routePrefix: null, ...)
                // This route will match all requests.
                // Therefore, this route will be a candidate and its tranformer will be called.
                // So, we use the ODataPath setting to verify whether the request is transformed or not.
                // Maybe we have a better solution later.
                return new ValueTask<RouteValueDictionary>(result: null);
            }

            (string routeName, object oDataPathValue) = values.GetODataRouteInfo();
            if (routeName != null)
            {
                HttpRequest request = httpContext.Request;

                // We need to call Uri.GetLeftPart(), which returns an encoded Url.
                // The ODL parser does not like raw values.
                Uri requestUri = new Uri(request.GetEncodedUrl());
                string requestLeftPart = requestUri.GetLeftPart(UriPartial.Path);
                string queryString = request.QueryString.HasValue ? request.QueryString.ToString() : null;

                // Call ODL to parse the Request URI.
                ODataPath path = ODataPathRouteConstraint.GetODataPath(oDataPathValue as string, requestLeftPart, queryString, () => request.CreateRequestContainer(routeName));
                if (path != null)
                {
                    // Set all the properties we need for routing, querying, formatting
                    IODataFeature odataFeature = httpContext.ODataFeature();
                    odataFeature.Path = path;
                    odataFeature.RouteName = routeName;
                    odataFeature.IsEndpointRouting = true; // mark as Endpoint routing

                    // Noted: we inject the ActionSelector and use it to select the best OData action.
                    // In .NET 5 or later, this maybe change.
                    RouteContext routeContext = new RouteContext(httpContext);
                    var condidates = _selector.SelectCandidates(routeContext);
                    var actionDescriptor = _selector.SelectBestCandidate(routeContext, condidates);
                    ControllerActionDescriptor controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
                    if (controllerActionDescriptor != null)
                    {
                        RouteValueDictionary newValues = new RouteValueDictionary();
                        foreach (var item in values)
                        {
                            newValues.Add(item.Key, item.Value);
                        }

                        foreach (var item in routeContext.RouteData.Values)
                        {
                            newValues[item.Key] = item.Value;
                        }

                        newValues["controller"] = controllerActionDescriptor.ControllerName;
                        newValues["action"] = controllerActionDescriptor.ActionName;
                        newValues["odataPath"] = oDataPathValue;

                        // Noted, here's a working around for mulitiple actions in same controller.
                        // For example, we have two "Get" methods in same controller, in order to help "EndpointSelector" 
                        // to select the correct Endpoint, we save the ActionDescriptor value into ODataFeature.
                        odataFeature.ActionDescriptor = controllerActionDescriptor;
                        // Add handler to handle options calls. The routing criteria has been patched to allow endpoint discovery using the correct cors headers
                        if (request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                        {
                            var metadata = actionDescriptor.EndpointMetadata;
                            // For option request can set this as the action will be handled by the cors middleware 
                            var metadataCollection = metadata?.Any() == true
                                ? new EndpointMetadataCollection(metadata)
                                : EndpointMetadataCollection.Empty;
                            // This workaround allows the default cors middleware to read the annotations if the user has them enabling fine-grained cors access control with endpoints
                            var endpoint = new Endpoint(null, metadataCollection, controllerActionDescriptor.ActionName);
                            httpContext.SetEndpoint(endpoint);
                        }

                        return new ValueTask<RouteValueDictionary>(newValues);
                    }
                }
                else
                {
                    // The request doesn't match this route so dispose the request container.
                    request.DeleteRequestContainer(true);
                }
            }

            return new ValueTask<RouteValueDictionary>(result: null);
        }
    }
}
#endif
