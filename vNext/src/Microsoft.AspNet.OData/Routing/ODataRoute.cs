using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.Routing;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

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

            context.HttpContext.ODataProperties().Model = _model;
            context.HttpContext.ODataProperties().RoutePrefix = _routePrefix;

            var parser = new ODataUriParser(_model, uri);
            var path = parser.ParsePath();
            context.HttpContext.ODataProperties().NewPath = path;
            context.HttpContext.ODataProperties().Path =
                context.HttpContext.ODataPathHandler().Parse(_model, "http://service-root/", remaining.ToString());
            context.HttpContext.ODataProperties().IsValidODataRequest = true;

            await m.RouteAsync(context);
            
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}