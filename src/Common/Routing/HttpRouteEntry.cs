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
using ParsedRouteType = System.Web.Http.Routing.HttpParsedRoute;
#else
using System.Web.Routing;
using ParsedRouteType = System.Web.Mvc.Routing.ParsedRoute;
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
    internal class HttpRouteEntry
#else
    internal class RouteEntry
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

        public HashSet<ReflectedHttpActionDescriptor> Actions { get; set; }
        public int Order { get; set; }
#else
        public Route Route { get; set; }
#endif
        public string Name { get; set; }
        public string Template { get; set; }

        // Segments have the following order:
        // 1 - Literal segments
        // 2 - Constrained parameter segments / Multi-part segments
        // 3 - Unconstrained parameter segments
        // 4 - Constrained wildcard parameter segments
        // 5 - Unconstrained wildcard parameter segments
        internal static int GetPrecedenceDigit(PathContentSegment segment, IDictionary<string, object> constraints)
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
                if (constraints != null && constraints.ContainsKey(parameterSegment.ParameterName))
                {
                    order--;
                }

                return order;
            }
        }

        public static decimal GetPrecedence(ParsedRouteType parsedRoute, IDictionary<string, object> constraints)
        {
            // Each precedence digit corresponds to one decimal place. For example, 3 segments with precedences 2, 1,
            // and 4 results in a combined precedence of 2.14 (decimal).
            IList<PathContentSegment> segments = parsedRoute.PathSegments.OfType<PathContentSegment>().ToArray();

            decimal precedence = 0;
            uint divisor = 1; // The first digit occupies the one's place.

            for (int i = 0; i < segments.Count; i++)
            {
                PathContentSegment segment = segments[i];

                int digit = GetPrecedenceDigit(segment, constraints);
                Contract.Assert(digit >= 0 && digit < 10);

                precedence = precedence + Decimal.Divide(digit, divisor);

                // The next digit occupies the subsequent place (always after the decimal point and growing to the
                // right).
                divisor *= 10;
            }

            return precedence;
        }
    }
}
