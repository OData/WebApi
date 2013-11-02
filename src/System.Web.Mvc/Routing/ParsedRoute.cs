// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Web.Mvc.Routing
{
    internal class ParsedRoute
    {
        public ParsedRoute(IList<PathSegment> pathSegments)
        {
            {
                Contract.Assert(pathSegments != null);
                PathSegments = pathSegments;
            }
        }

        public IList<PathSegment> PathSegments { get; private set; }
    }
}