using System;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Mvc.Abstractions;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    public interface IODataRoutingConvention
    {
        ActionDescriptor SelectAction(RouteContext routeContext);
    }
}