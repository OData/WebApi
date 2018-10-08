using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreODataSample.DynamicModels.Web.Edm;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace AspNetCoreODataSample.DynamicModels.Web.Utils
{
    public class InteriorRoutingConvention : IODataRoutingConvention
    {
        private readonly string ControllerName = "Interior";

        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            var odataPath = routeContext.HttpContext.ODataFeature().Path;

            // check whether an entity set was requested
            if (!(odataPath.Segments.FirstOrDefault() is EntitySetSegment entitySetSegment) || !(entitySetSegment.EntitySet.Type is IEdmCollectionType edmCollectionType))
            {
                return null;
            }

            // ask model provider whether requested entity set is a Interior
            var edmModel = routeContext.HttpContext.Request.GetModel();
            var edmModelProvider = routeContext.HttpContext.RequestServices.GetRequiredService<EdmModelBuilder>();
            if (edmModelProvider.IsInterior(edmModel, edmCollectionType.ElementType.Definition))
            {
                IActionDescriptorCollectionProvider actionCollectionProvider =
                    routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == ControllerName);

                if (odataPath.PathTemplate == "~/entityset/key/navigation")
                {
                    if (routeContext.HttpContext.Request.Method.ToUpperInvariant() == "GET")
                    {
                        NavigationPropertySegment navigationPathSegment = (NavigationPropertySegment)odataPath.Segments.Last();

                        routeContext.RouteData.Values["navigation"] = navigationPathSegment.NavigationProperty.Name;

                        KeySegment keyValueSegment = (KeySegment)odataPath.Segments[1];
                        routeContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Keys.First().Value;

                        return actionDescriptors.Where(c => c.ActionName == "GetNavigation");
                    }
                }

                SelectControllerResult controllerResult = new SelectControllerResult(ControllerName, null);
                IList<IODataRoutingConvention> routingConventions = ODataRoutingConventions.CreateDefault();
                foreach (NavigationSourceRoutingConvention nsRouting in routingConventions.OfType<NavigationSourceRoutingConvention>())
                {
                    string actionName = nsRouting.SelectAction(routeContext, controllerResult, actionDescriptors);
                    if (!String.IsNullOrEmpty(actionName))
                    {
                        return actionDescriptors.Where(
                            c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return null;
        }
    }
}
