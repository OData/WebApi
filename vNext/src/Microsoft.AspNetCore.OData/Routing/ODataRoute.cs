using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing
{
    public class ODataRoute : IRouter
    {
        private readonly string _routePrefix;
        private readonly IEdmModel _model;

        public ODataRoute(string routePrefix, IEdmModel model)
        {
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

            var m = context.HttpContext.RequestServices.GetRequiredService<MvcRouteHandler>();
            await m.RouteAsync(context);
            
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}