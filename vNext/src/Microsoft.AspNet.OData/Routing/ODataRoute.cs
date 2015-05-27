using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.AspNet.OData.Routing
{
    public class ODataRoute : IRouter
    {
        private readonly IODataRoutingConvention _routingConvention;
        private readonly string _routePrefix;
        private readonly IEdmModel _model;
        private readonly IRouter m = new MvcRouteHandler();

        public ODataRoute(string routePrefix, IEdmModel model)
        {
            _routingConvention = new DefaultODataRoutingConvention();
            _routePrefix = routePrefix;
            _model = model;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;

            Uri uri;
            PathString remaining;
            if (!request.Path.StartsWithSegments(PathString.FromUriComponent("/" + _routePrefix), out remaining))
            {
                // Fallback to other routes.
                return;
            }

            uri = new Uri(remaining.ToString(), UriKind.Relative);
            
            context.ODataProperties().Model = _model;
            var parser = new ODataUriParser(_model, uri);
            context.ODataProperties().NewPath = parser.ParsePath();
            context.ODataProperties().IsValidODataRequest = true;

            await m.RouteAsync(context);
            context.IsHandled = true;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}