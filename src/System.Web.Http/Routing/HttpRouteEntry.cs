// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Route information used to name and order routes for attribute routing. Applies ordering based on the prefix order
    /// and the attribute order first, then applies a default order that registers more specific routes earlier.
    /// </summary>
    internal class HttpRouteEntry : IComparable<HttpRouteEntry>
    {
        public IHttpRoute Route { get; set; }
        public string Name { get; set; }
        public string RouteTemplate { get; set; }
        public int PrefixOrder { get; set; }
        public int Order { get; set; }

        public int CompareTo(HttpRouteEntry other)
        {
            Contract.Assert(other != null);

            // Order by prefixes first
            if (PrefixOrder > other.PrefixOrder)
            {
                return 1;
            }
            else if (PrefixOrder < other.PrefixOrder)
            {
                return -1;
            }

            // Then order by the attribute order
            if (Order > other.Order)
            {
                return 1;
            }
            else if (Order < other.Order)
            {
                return -1;
            }

            HttpRoute httpRoute1 = Route as HttpRoute;
            HttpRoute httpRoute2 = other.Route as HttpRoute;
            if (httpRoute1 != null && httpRoute2 != null)
            {
                int comparison = Compare(httpRoute1, httpRoute2);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            // Compare the route templates alphabetically to ensure the sort is stable and deterministic in almost all cases
            return String.Compare(RouteTemplate, other.RouteTemplate, StringComparison.OrdinalIgnoreCase);
        }

        // Default ordering goes through segments one by one and tries to apply an ordering
        private static int Compare(HttpRoute httpRoute1, HttpRoute httpRoute2)
        {
            HttpParsedRoute parsedRoute1 = httpRoute1.ParsedRoute;
            HttpParsedRoute parsedRoute2 = httpRoute2.ParsedRoute;

            IList<PathContentSegment> segments1 = parsedRoute1.PathSegments.OfType<PathContentSegment>().ToArray();
            IList<PathContentSegment> segments2 = parsedRoute2.PathSegments.OfType<PathContentSegment>().ToArray();

            for (int i = 0; i < segments1.Count && i < segments2.Count; i++)
            {
                PathContentSegment segment1 = segments1[i];
                PathContentSegment segment2 = segments2[i];

                int order1 = GetOrder(segment1, httpRoute1.Constraints);
                int order2 = GetOrder(segment2, httpRoute2.Constraints);

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
