using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.OData.Routing
{
    public class DefaultODataRoutingConvention : IODataRoutingConvention
    {
        public ActionDescriptor SelectAction(ODataRouteContext routeContext)
        {
            var odataPath = routeContext.Path;
            var controllerName = string.Empty;
            var actionName = "Get";
            var keys = new List<KeyValuePair<string, object>>();

            if (odataPath.FirstSegment is MetadataSegment)
            {
                controllerName = "Metadata";
            }
            else
            {
                // TODO: we should use attribute routing to determine controller and action.
                var entitySetSegment = odataPath.FirstSegment as EntitySetSegment;
                if (entitySetSegment != null)
                {
                    controllerName = entitySetSegment.EntitySet.Name;
                }

                var keySegment = odataPath.FirstOrDefault(s => s is KeySegment) as KeySegment;
                if (keySegment != null)
                {
                    keys.AddRange(keySegment.Keys);
                }

                var navigationPropertySegment =
                    odataPath.FirstOrDefault(s => s is NavigationPropertySegment) as NavigationPropertySegment;
                if (navigationPropertySegment != null)
                {
                    actionName += navigationPropertySegment.NavigationProperty.Name;
                }
            }
            
            var services = routeContext.HttpContext.ApplicationServices;
            var provider = services.GetRequiredService<IActionDescriptorsCollectionProvider>();
            var actionDescriptor = provider.ActionDescriptors.Items.SingleOrDefault(d =>
            {
                var c = d as ControllerActionDescriptor;
                return c != null
                && c.ControllerName == controllerName
                && c.Name == actionName
                && c.Parameters.Count == keys.Count;
            });

            if (actionDescriptor != null)
            {
                WriteRouteData(routeContext, actionDescriptor.Parameters, keys);
            }

            return actionDescriptor;
        }

        private void WriteRouteData(RouteContext context, IList<ParameterDescriptor> parameters, IList<KeyValuePair<string, object>> keys)
        {
            for (int i = 0; i < parameters.Count; ++i)
            {
                // TODO: check if parameters match keys.
                context.RouteData.Values[parameters[i].Name] = keys[i].Value;
            }
        }
    }
}