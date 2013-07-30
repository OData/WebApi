// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    // Route that generates a virtual path, but does not claim any routes. 
    // This can be used with RouteCollectionRoute to provide generation by names. 
    // Delegates to an inner route to do actual generation.
    internal class GenerateRoute : IHttpRoute
    {
        private readonly IHttpRoute _innerRoute;

        public GenerateRoute(IHttpRoute inner)
        {
            _innerRoute = inner;
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