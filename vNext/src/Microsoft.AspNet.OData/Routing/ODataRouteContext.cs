using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
            //this.IsHandled = other.IsHandled;
            this.RouteData = new RouteData(other.RouteData);
        }

        public ODataPath Path { get; set; }
    }
}
