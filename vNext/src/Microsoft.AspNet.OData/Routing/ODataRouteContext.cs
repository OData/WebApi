using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Core.UriParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Routing
{
    public class ODataRouteContext : RouteContext
    {
        public ODataRouteContext(HttpContext httpContext) : base(httpContext)
        {
        }

        public ODataRouteContext(RouteContext other)
            :this(other.HttpContext)
        {
            //this.IsHandled = other.IsHandled;
            this.RouteData = new RouteData(other.RouteData);
        }

        public ODataPath Path { get; set; }
    }
}
