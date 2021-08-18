//-----------------------------------------------------------------------------
// <copyright file="ODataRoute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public partial class ODataRoute
    {
        private static readonly string _escapedHashMark = Uri.EscapeDataString("#");
        private static readonly string _escapedQuestionMark = Uri.EscapeDataString("?");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="pathRouteConstraint">The OData path route constraint.</param>
        private void Initialize(string routePrefix, ODataPathRouteConstraint pathRouteConstraint)
        {
            RoutePrefix = routePrefix;
            PathRouteConstraint = pathRouteConstraint;

            // We can only use our fast-path for link generation if there are no open brackets in the route prefix
            // that need to be replaced. If there are, fall back to the slow path.
            CanGenerateDirectLink = routePrefix == null || routePrefix.IndexOf('{') == -1;
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public string RoutePrefix { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataPathRouteConstraint"/> on this route.
        /// </summary>
        public ODataPathRouteConstraint PathRouteConstraint { get; private set; }

        internal bool CanGenerateDirectLink { get; private set; }

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
