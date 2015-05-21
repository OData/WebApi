using System;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.OData.Routing
{
    public interface IODataRoutingConvention
    {
        ControllerActionDescriptor SelectControllerAction(ODataPath odataPath, RouteContext context);
    }
}