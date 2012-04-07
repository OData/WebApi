// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.Http.Routing
{
    // Represents a segment of a URI that is not a separator. It contains subsegments such as literals and parameters.
    internal sealed class PathContentSegment : PathSegment
    {
        public PathContentSegment(IList<PathSubsegment> subsegments)
        {
            Subsegments = subsegments;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Not changing original algorithm.")]
        public bool IsCatchAll
        {
            get
            {
                // TODO: Verify this is correct. Maybe add an assert.
                return Subsegments.Any<PathSubsegment>(seg => (seg is PathParameterSubsegment) && ((PathParameterSubsegment)seg).IsCatchAll);
            }
        }

        public IList<PathSubsegment> Subsegments { get; private set; }

#if ROUTE_DEBUGGING
        public override string LiteralText
        {
            get
            {
                List<string> s = new List<string>();
                foreach (PathSubsegment subsegment in Subsegments)
                {
                    s.Add(subsegment.LiteralText);
                }
                return String.Join(String.Empty, s.ToArray());
            }
        }

        public override string ToString()
        {
            List<string> s = new List<string>();
            foreach (PathSubsegment subsegment in Subsegments)
            {
                s.Add(subsegment.ToString());
            }
            return "[ " + String.Join(", ", s.ToArray()) + " ]";
        }
#endif
    }
}
