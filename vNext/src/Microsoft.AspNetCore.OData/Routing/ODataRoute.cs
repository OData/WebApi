// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public class ODataRoute : IRouter
    {
        private readonly string _routePrefix;
        private readonly IEdmModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute" /> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="model">The Edm model.</param>
        public ODataRoute(string routePrefix, IEdmModel model)
        {
            _routePrefix = routePrefix;
            _model = model;
        }

        /// <inheritdoc />
        public async Task RouteAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;

            Uri uri;
            PathString remaining;
            if (!request.Path.StartsWithSegments(PathString.FromUriComponent("/" + _routePrefix), out remaining))
            {
                // Fallback to other routes.
                return;
            }

            uri = new Uri(remaining.ToString(), UriKind.Relative);

            context.HttpContext.ODataProperties().Model = _model;
            context.HttpContext.ODataProperties().RoutePrefix = _routePrefix;

            var parser = new ODataUriParser(_model, uri);
            var path = parser.ParsePath();
            context.HttpContext.ODataProperties().NewPath = path;
            context.HttpContext.ODataProperties().Path =
                context.HttpContext.ODataPathHandler().Parse(_model, "http://service-root/", remaining.ToString());
            context.HttpContext.ODataProperties().IsValidODataRequest = true;

            var m = context.HttpContext.RequestServices.GetRequiredService<MvcRouteHandler>();
            await m.RouteAsync(context);
            
        }

        /// <inheritdoc />
        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}