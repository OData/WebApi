using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Routing
{
    public class ODataRoute : IRouter
    {
        private readonly IODataRoutingConvention _routingConvention;

        public ODataRoute()
        {
            _routingConvention = new DefaultODataRoutingConvention();
        }

        public async Task RouteAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;
            var _provider = context.HttpContext.RequestServices;

            var cp = _provider.GetService<ODataContextProvider>();

            IEdmModel model;
            Uri uri;
            if(!TryGetPathUri(request, cp, out model, out uri))
            {
                //return;
                throw new Exception("route error");
            }

            _provider.GetService<ODataProperties>().Model = model;

            var parser = new ODataUriParser(model, uri);
            var path = parser.ParsePath();

            var actionDescriptor = _routingConvention.SelectControllerAction(path, context);
            await InvokeActionAsync(context, actionDescriptor);
            context.IsHandled = true;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }

        private bool TryGetPathUri(HttpRequest request, ODataContextProvider routePrefix, out IEdmModel model, out Uri uri)
        {
            foreach(var prefix in routePrefix.ContextMap.Keys)
            {
                PathString remaining;
                if (request.Path.StartsWithSegments(PathString.FromUriComponent("/" + prefix), out remaining))
                {
                    uri = new Uri(remaining.ToString(), UriKind.Relative);
                    model = routePrefix.ContextMap[prefix].Model;
                    return true;
                }
            }

            model = null;
            uri = null;
            return false;
        }

        private async Task InvokeActionAsync(RouteContext context, ActionDescriptor actionDescriptor)
        {
            var services = context.HttpContext.RequestServices;
            Debug.Assert(services != null);

            var actionContext = new ActionContext(context.HttpContext, context.RouteData, actionDescriptor);

            var optionsAccessor = services.GetRequiredService<IOptions<MvcOptions>>();
            actionContext.ModelState.MaxAllowedErrors = optionsAccessor.Options.MaxModelValidationErrors;

            var contextAccessor = services.GetRequiredService<IScopedInstance<ActionContext>>();
            contextAccessor.Value = actionContext;
            var invokerFactory = services.GetRequiredService<IActionInvokerFactory>();
            var invoker = invokerFactory.CreateInvoker(actionContext);

            await invoker.InvokeAsync();
        }
    }
}