using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing
{
    public class ODataRoute : IRouter
    {
        private readonly IODataRoutingConvention _routingConvention;
        public readonly string RoutePrefix;
        private readonly IEdmModel _model;
        private readonly IRouter m = new MvcRouteHandler();

		public static ODataRoute Instance { get; internal set; }

		public ODataRoute(string routePrefix, IEdmModel model)
        {
            _routingConvention = new DefaultODataRoutingConvention();
            RoutePrefix = routePrefix;
            _model = model;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;
			Uri uri;
            PathString remaining;
            if (!request.Path.StartsWithSegments(PathString.FromUriComponent("/" + RoutePrefix), out remaining))
            {
                // Fallback to other routes.
                return;
            }

            uri = new Uri(remaining.ToString(), UriKind.Relative);
            
            context.HttpContext.ODataProperties().Model = _model;
            var parser = new ODataUriParser(_model, uri);
            var path = parser.ParsePath();
            context.HttpContext.ODataProperties().NewPath = path;
            context.HttpContext.ODataProperties().Path =
                context.HttpContext.ODataPathHandler().Parse(_model, "http://service-root/", remaining.ToString());
            context.HttpContext.ODataProperties().IsValidODataRequest = true;
            var ase = context.HttpContext.RequestServices.GetRequiredService<IActionSelector>();
            await m.RouteAsync(context);
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}