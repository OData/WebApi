// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Routing
{
    public class ODataRouteContext : RouteContext
    {
        public ODataRouteContext(HttpContext httpContext) : base(httpContext)
        {
        }

        public ODataRouteContext(RouteContext other)
            :this(other.HttpContext)
        {
            this.Handler = other.Handler;
            this.RouteData = new RouteData(other.RouteData);
        }

        public ODataPath Path { get; set; }
    }
}
