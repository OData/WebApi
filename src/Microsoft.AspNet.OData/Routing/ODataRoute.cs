//-----------------------------------------------------------------------------
// <copyright file="ODataRoute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public partial class ODataRoute : HttpRoute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="pathConstraint">The OData path constraint.</param>
        public ODataRoute(string routePrefix, ODataPathRouteConstraint pathConstraint)
            : this(
                routePrefix, (IHttpRouteConstraint)pathConstraint, defaults: null, constraints: null, dataTokens: null,
                handler: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute" /> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The route constraint.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public ODataRoute(string routePrefix, IHttpRouteConstraint routeConstraint)
            : this(routePrefix, routeConstraint, defaults: null, constraints: null, dataTokens: null, handler: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute" /> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="pathConstraint">The OData path constraint.</param>
        /// <param name="defaults">The default values for the route.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="dataTokens">The data tokens.</param>
        /// <param name="handler">The message handler for the route.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public ODataRoute(
            string routePrefix,
            ODataPathRouteConstraint pathConstraint,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            HttpRouteValueDictionary dataTokens,
            HttpMessageHandler handler)
            : this(routePrefix, (IHttpRouteConstraint)pathConstraint, defaults, constraints, dataTokens, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute" /> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The route constraint.</param>
        /// <param name="defaults">The default values for the route.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="dataTokens">The data tokens.</param>
        /// <param name="handler">The message handler for the route.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public ODataRoute(
            string routePrefix,
            IHttpRouteConstraint routeConstraint,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            HttpRouteValueDictionary dataTokens,
            HttpMessageHandler handler)
            : base(GetRouteTemplate(routePrefix), defaults, constraints, dataTokens, handler)
        {
            RouteConstraint = routeConstraint;
            Initialize(routePrefix, routeConstraint as ODataPathRouteConstraint);

            if (routeConstraint != null)
            {
                Constraints.Add(ODataRouteConstants.ConstraintName, routeConstraint);
            }

            Constraints.Add(ODataRouteConstants.VersionConstraintName, new ODataVersionConstraint());
        }

        /// <summary>
        /// Gets the <see cref="IHttpRouteConstraint"/> on this route.
        /// </summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public IHttpRouteConstraint RouteConstraint { get; private set; }

        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public override IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            // Only perform URL generation if the "httproute" key was specified. This allows these
            // routes to be ignored when a regular MVC app tries to generate URLs. Without this special
            // key an HTTP route used for Web API would normally take over almost all the routes in a
            // typical app.
            if (values != null && values.Keys.Contains(HttpRoute.HttpRouteKey, StringComparer.OrdinalIgnoreCase))
            {
                // Fast path link generation where we recognize an OData route of the form "prefix/{*odataPath}".
                // Link generation using HttpRoute.GetVirtualPath can consume up to 30% of processor time
                object odataPathValue;
                if (values.TryGetValue(ODataRouteConstants.ODataPath, out odataPathValue))
                {
                    string odataPath = odataPathValue as string;
                    if (odataPath != null)
                    {
                        // Try to generate an optimized direct link
                        // Otherwise, fall back to the base implementation
                        return CanGenerateDirectLink
                            ? GenerateLinkDirectly(odataPath)
                            : base.GetVirtualPath(request, values);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Relax the version constraint. The service will allow clients to send both OData V4 and previous max version headers.
        /// Headers for the previous max version will be ignored.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        [Obsolete("The version constraint is relaxed by default")]
        public ODataRoute HasRelaxedODataVersionConstraint()
        {
            return SetODataVersionConstraint(true);
        }

        private ODataRoute SetODataVersionConstraint(bool isRelaxedMatch)
        {
            object constraint;
            if (Constraints.TryGetValue(ODataRouteConstants.VersionConstraintName, out constraint))
            {
                ((ODataVersionConstraint)constraint).IsRelaxedMatch = isRelaxedMatch;
            }
            return this;
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        internal HttpVirtualPathData GenerateLinkDirectly(string odataPath)
        {
            Contract.Assert(odataPath != null);
            Contract.Assert(CanGenerateDirectLink);

            string link = CombinePathSegments(RoutePrefix, odataPath);
            link = UriEncode(link);
            return new HttpVirtualPathData(this, link);
        }
   }
}
