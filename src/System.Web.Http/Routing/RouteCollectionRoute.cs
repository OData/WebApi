// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    // A single route that is the composite of multiple "sub routes".  
    // This is used in attribute routing. 
    internal class RouteCollectionRoute : IHttpRoute, IEnumerable<IHttpRoute>
    {
        // Key for accessing SubRoutes on a RouteData.
        // We expose this through the RouteData.Values instead of a derived class because 
        // RouteData can get wrapped in another type, but Values still gets persisted through the wrappers. 
        // Prefix with a \0 to protect against conflicts with user keys. 
        public const string SubRouteDataKey = "MS_SubRoutes";

        private HttpSubRouteCollection _subRoutes;

        private static readonly IDictionary<string, object> _empty = EmptyReadOnlyDictionary<string, object>.Value;
        
        public RouteCollectionRoute()
        {
        }

        // This will enumerate all controllers and action descriptors, which will run those 
        // Initialization hooks, which may try to initialize controller-specific config, which
        // may call back to the initialize hook. So guard against that reentrancy.
        private bool _beingInitialized;

        // deferred hook for initializing the sub routes. The composite route can be added during the middle of 
        // intializing, but then the actual sub routes can get populated after initialization has finished. 
        public HttpSubRouteCollection EnsureInitialized(Func<HttpSubRouteCollection> initializer)
        {
            if (_beingInitialized && _subRoutes == null)
            {
                // Avoid reentrant initialization
                return null;
            }

            try
            {
                _beingInitialized = true;

                _subRoutes = initializer();
                Contract.Assert(_subRoutes != null);                
                return _subRoutes;
            }
            finally
            {
                _beingInitialized = false;
            }
        }

        public HttpSubRouteCollection SubRoutes
        {
            get
            {
                // Caller should have already explicitly called EnsureInitialize. 
                // Avoid lazy initilization from within the route table because the route table
                // is shared resource and init can happen 
                if (_subRoutes == null)
                {
                    string msg = Error.Format(SRResources.Object_NotYetInitialized);
                    throw new InvalidOperationException(msg);
                }

                return _subRoutes;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _subRoutes = value;
            }
        }

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
            get { return null; }
        }

        public HttpMessageHandler Handler
        {
            get
            {
                return null;
            }
        }
                
        // Returns null if no match. 
        // Else, returns a composite route data that encapsulates the possible routes this may match against. 
        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            // Only take 1st match per verb. This is to honor URL constraints.
            // EG, consider this scenario:
            // (1)  GET /controller/{id:int}
            // (2)  GET /controller/{id}
            // (3)  PUT /controller/{value}
            // Matching "/controller/15" should only return routes 1 and 3, not 2. 
            HashSet<HttpMethod> methods = new HashSet<HttpMethod>();

            List<IHttpRouteData> list = new List<IHttpRouteData>();
            foreach (IHttpRoute route in SubRoutes)
            {
                IHttpRouteData match = route.GetRouteData(virtualPathRoot, request);
                if (match != null)
                {
                    // Is this the first verb?
                    HttpMethod verb = match.Route.GetDirectRouteVerb();
                    if (methods.Add(verb))
                    {
                        list.Add(match);
                    }
                }
            }
            if (list.Count == 0)
            {
                return null;  // no matches
            }

            return new RouteCollectionRouteData(this, list.ToArray());
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            // Use GenerationRoute stubs to get placeholders for all the sub routes. 
            return null;
        }

        public IEnumerator<IHttpRoute> GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        // Represents a union of multiple IHttpRouteDatas. 
        private class RouteCollectionRouteData : IHttpRouteData
        {
            public RouteCollectionRouteData(IHttpRoute parent, IHttpRouteData[] subRouteDatas)
            {
                this.Route = parent;
                this.SubRouteDatas = subRouteDatas;
            }

            public IHttpRoute Route { get; private set; }

            public IHttpRouteData[] SubRouteDatas { get; private set; }

            private IDictionary<string, object> _values;

            public IDictionary<string, object> Values
            {
                get
                {
                    // Keys is just a union of the Keys from the sub routes. 
                    // We don't actually use the values, because different subroutes may have conflicting values.
                    // Action selection just needs to know which keys are present. 
                    if (_values == null)
                    {
                        var dict = new HttpRouteValueDictionary();
                        foreach (var data in SubRouteDatas)
                        {
                            foreach (var kv in data.Values)
                            {
                                // Actual value doesn't matter. We just look for the presence of the key. 
                                dict[kv.Key] = String.Empty;
                            }
                        }
                        dict[SubRouteDataKey] = SubRouteDatas;

                        _values = dict;
                    }

                    return _values;
                }
            }
        }        
    }
}