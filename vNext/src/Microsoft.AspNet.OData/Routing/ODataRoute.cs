using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using System;
using System.Threading.Tasks;

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
            var _provider = context.HttpContext.RequestServices;

            Uri uri;
            PathString remaining;
            if (!request.Path.StartsWithSegments(PathString.FromUriComponent("/" + _routePrefix), out remaining))
            {
                // Fallback to MVC routing.
                return;
            }

            uri = new Uri(remaining.ToString(), UriKind.Relative);

            _provider.GetService<ODataProperties>().Model = _model;
            var parser = new ODataUriParser(_model, uri);
            var path = parser.ParsePath();

            var ctx = new ODataRouteContext(context) { Path = path };
            await m.RouteAsync(ctx);
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}