// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
#if ASPNETWEBAPI
using System.Collections.ObjectModel;
#endif
using System.Diagnostics.Contracts;
using System.Linq;
#if ASPNETWEBAPI
using System.Net.Http;
using System.Web.Http.Controllers;
#else
using System.Web.Routing;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// Route information used to name and order routes for attribute routing. Applies ordering based on the prefix order
    /// and the attribute order first, then applies a default order that registers more specific routes earlier.
    /// </summary>
#if ASPNETWEBAPI
    internal class HttpRouteEntry : IComparable<HttpRouteEntry>
#else
    internal class RouteEntry : IComparable<RouteEntry>
#endif
    {
#if ASPNETWEBAPI
        public IHttpRoute Route { get; set; }

        public HttpParsedRoute ParsedRoute
        {
            get
            {
                HttpRoute route = Route as HttpRoute;
                Contract.Assert(route != null);
                return route.ParsedRoute;
            }
        }

        public IEnumerable<HttpMethod> HttpMethods { get; set; }
        public HashSet<ReflectedHttpActionDescriptor> Actions { get; set; }
#else
        public Route Route { get; set; }
        public ParsedRoute ParsedRoute { get; set; }
#endif
        public string Name { get; set; }
        public string RouteTemplate { get; set; }
        public int Order { get; set; }

#if ASPNETWEBAPI
        public int CompareTo(HttpRouteEntry other)
#else
        public int CompareTo(RouteEntry other)
#endif
        {
            Contract.Assert(other != null);            
                        
            if (Order > other.Order)
            {
                return 1;
            }
            else if (Order < other.Order)
            {
                return -1;
            }

#if ASPNETWEBAPI
            HttpRoute httpRoute1 = Route as HttpRoute;
            HttpRoute httpRoute2 = other.Route as HttpRoute;
#else
            Route httpRoute1 = Route;
            Route httpRoute2 = other.Route;
#endif
            if (httpRoute1 != null && httpRoute2 != null)
            {
                int comparison = Compare(this, other);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            // Compare the route templates alphabetically to ensure the sort is stable and deterministic in almost all cases
            return String.Compare(RouteTemplate, other.RouteTemplate, StringComparison.OrdinalIgnoreCase);
        }

        // Default ordering goes through segments one by one and tries to apply an ordering
#if ASPNETWEBAPI
        private static int Compare(HttpRouteEntry entry1, HttpRouteEntry entry2)
#else
        private static int Compare(RouteEntry entry1, RouteEntry entry2)
#endif
        {
#if ASPNETWEBAPI
            HttpParsedRoute parsedRoute1 = entry1.ParsedRoute;
            HttpParsedRoute parsedRoute2 = entry2.ParsedRoute;
#else
            ParsedRoute parsedRoute1 = entry1.ParsedRoute;
            ParsedRoute parsedRoute2 = entry2.ParsedRoute;
#endif

            IList<PathContentSegment> segments1 = parsedRoute1.PathSegments.OfType<PathContentSegment>().ToArray();
            IList<PathContentSegment> segments2 = parsedRoute2.PathSegments.OfType<PathContentSegment>().ToArray();

            for (int i = 0; i < segments1.Count && i < segments2.Count; i++)
            {
                PathContentSegment segment1 = segments1[i];
                PathContentSegment segment2 = segments2[i];

                int order1 = GetOrder(segment1, entry1.Route.Constraints);
                int order2 = GetOrder(segment2, entry2.Route.Constraints);

                if (order1 > order2)
                {
                    return 1;
                }
                else if (order1 < order2)
                {
                    return -1;
                }
            }

            return 0;
        }

        // Segments have the following order:
        // 1 - Literal segments
        // 2 - Constrained parameter segments / Multi-part segments
        // 3 - Unconstrained parameter segments
        // 4 - Constrained wildcard parameter segments
        // 5 - Unconstrained wildcard parameter segments
        private static int GetOrder(PathContentSegment segment, IDictionary<string, object> constraints)
        {
            if (segment.Subsegments.Count > 1)
            {
                // Multi-part segments should appear after literal segments but before parameter segments
                return 2;
            }

            PathSubsegment subsegment = segment.Subsegments[0];
            // Literal segments always go first
            if (subsegment is PathLiteralSubsegment)
            {
                return 1;
            }
            else
            {
                PathParameterSubsegment parameterSegment = subsegment as PathParameterSubsegment;
                Contract.Assert(parameterSegment != null);
                int order = parameterSegment.IsCatchAll ? 5 : 3;
                
                // If there is a route constraint for the parameter, reduce order by 1
                // Constrained parameters end up with order 2, Constrained catch alls end up with order 4
                if (constraints.ContainsKey(parameterSegment.ParameterName))
                {
                    order--;
                }

                return order;
            }
        }
    }
}
