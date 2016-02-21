using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    public interface IODataRoutingConvention
    {
        ActionDescriptor SelectAction(RouteContext routeContext);
    }
}