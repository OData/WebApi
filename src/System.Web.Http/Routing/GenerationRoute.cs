// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Route that generates a virtual path, but does not claim any routes. 
    /// This can be used with RouteCollectionRoute to provide generation by names. 
    /// Delegates to an inner route to do actual generation.
    /// </summary>
    /// <remarks>
    /// Corresponds to the MVC implementation of attribute routing in System.Web.Mvc.Routing.GenerationRoute.
    /// </remarks>
    internal class GenerationRoute : IHttpRoute
    {
        private readonly IHttpRoute _innerRoute;

        public GenerationRoute(IHttpRoute innerRoute)
        {
            if (innerRoute == null)
            {
                throw new ArgumentNullException("innerRoute");
            }

            _innerRoute = innerRoute;
        }

        private static readonly IDictionary<string, object> _empty = EmptyReadOnlyDictionary<string, object>.Value;

        public string RouteTemplate
        {
            get { return String.Empty; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return _empty; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return _empty; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return _empty; }
        }

        public HttpMessageHandler Handler
        {
            get { return null; }
        }

        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            // Claims no routes
            return null;
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            return _innerRoute.GetVirtualPath(request, values);
        }
    }
}