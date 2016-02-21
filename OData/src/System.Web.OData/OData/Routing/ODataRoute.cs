﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public class ODataRoute : HttpRoute
    {
        private static readonly string _escapedHashMark = Uri.HexEscape('#');
        private static readonly string _escapedQuestionMark = Uri.HexEscape('?');

        private bool _canGenerateDirectLink;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute" /> class.
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
        public ODataRoute(
            string routePrefix,
            IHttpRouteConstraint routeConstraint,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            HttpRouteValueDictionary dataTokens,
            HttpMessageHandler handler)
            : base(GetRouteTemplate(routePrefix), defaults, constraints, dataTokens, handler)
        {
            RoutePrefix = routePrefix;
            PathRouteConstraint = routeConstraint as ODataPathRouteConstraint;
            RouteConstraint = routeConstraint;

            // We can only use our fast-path for link generation if there are no open brackets in the route prefix
            // that need to be replaced. If there are, fall back to the slow path.
            _canGenerateDirectLink = routePrefix == null || routePrefix.IndexOf('{') == -1;

            if (routeConstraint != null)
            {
                Constraints.Add(ODataRouteConstants.ConstraintName, routeConstraint);
            }

            Constraints.Add(ODataRouteConstants.VersionConstraintName, new ODataVersionConstraint());
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public string RoutePrefix { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataPathRouteConstraint"/> on this route.
        /// </summary>
        public ODataPathRouteConstraint PathRouteConstraint { get; private set; }

        /// <summary>
        /// Gets the <see cref="IHttpRouteConstraint"/> on this route.
        /// </summary>
        public IHttpRouteConstraint RouteConstraint { get; private set; }

        internal bool CanGenerateDirectLink
        {
            get
            {
                return _canGenerateDirectLink;
            }
        }

        /// <inheritdoc />
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
                        return _canGenerateDirectLink
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

        internal HttpVirtualPathData GenerateLinkDirectly(string odataPath)
        {
            Contract.Assert(odataPath != null);
            Contract.Assert(_canGenerateDirectLink);

            string link = CombinePathSegments(RoutePrefix, odataPath);
            link = UriEncode(link);
            return new HttpVirtualPathData(this, link);
        }

        private static string GetRouteTemplate(string prefix)
        {
            return String.IsNullOrEmpty(prefix) ?
                ODataRouteConstants.ODataPathTemplate :
                prefix + '/' + ODataRouteConstants.ODataPathTemplate;
        }

        private static string CombinePathSegments(string routePrefix, string odataPath)
        {
            if (String.IsNullOrEmpty(routePrefix))
            {
                return odataPath;
            }
            else
            {
                return String.IsNullOrEmpty(odataPath) ? routePrefix : routePrefix + '/' + odataPath;
            }
        }

        private static string UriEncode(string str)
        {
            Contract.Assert(str != null);

            string escape = Uri.EscapeUriString(str);
            escape = escape.Replace("#", _escapedHashMark);
            escape = escape.Replace("?", _escapedQuestionMark);
            return escape;
        }
    }
}