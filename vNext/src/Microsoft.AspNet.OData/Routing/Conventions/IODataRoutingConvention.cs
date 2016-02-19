using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    public interface IODataRoutingConvention
    {
        ActionDescriptor SelectAction(RouteContext routeContext);
    }
}