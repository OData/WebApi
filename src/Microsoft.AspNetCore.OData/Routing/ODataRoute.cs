// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public partial class ODataRoute : Route
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="target">The target router.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public ODataRoute(IRouter target, string routeName, string routePrefix, ODataPathRouteConstraint routeConstraint, IInlineConstraintResolver resolver)
            : this(target, routeName, routePrefix, (IRouteConstraint)routeConstraint, resolver)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="target">The target router.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public ODataRoute(IRouter target, string routeName, string routePrefix, IRouteConstraint routeConstraint, IInlineConstraintResolver resolver)
            : base(target, routeName, GetRouteTemplate(routePrefix), defaults: null, constraints: null, dataTokens: null, inlineConstraintResolver: resolver)
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
        /// Gets the <see cref="IRouteConstraint"/> on this route.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IRouteConstraint RouteConstraint { get; private set; }

        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            // Fast path link generation where we recognize an OData route of the form "prefix/{*odataPath}".
            // Link generation using HttpRoute.GetVirtualPath can consume up to 30% of processor time
            object odataPathValue;
            if (context.Values.TryGetValue(ODataRouteConstants.ODataPath, out odataPathValue))
            {
                string odataPath = odataPathValue as string;
                if (odataPath != null)
                {
                    // Try to generate an optimized direct link
                    // Otherwise, fall back to the base implementation
                    return CanGenerateDirectLink
                        ? GenerateLinkDirectly(odataPath)
                        : base.GetVirtualPath(context);
                }
            }

            return null;
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal VirtualPathData GenerateLinkDirectly(string odataPath)
        {
            Contract.Assert(odataPath != null);
            Contract.Assert(CanGenerateDirectLink);

            string link = CombinePathSegments(RoutePrefix, odataPath);
            link = UriEncode(link);
            return new VirtualPathData(this, link);
        }
    }
}