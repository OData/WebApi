using System;
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
        public ControllerActionDescriptor SelectControllerAction(ODataPath odataPath, RouteContext context)
        {
            var controllerName = string.Empty;
            var actionName = "Get";
            var keyCount = 0;

            if (odataPath.FirstSegment is MetadataSegment)
            {
                controllerName = "Metadata";
            }
            else
            {
                // In the future, we should use attribute routing to determine controller and action.
                var entitySetSegment = odataPath.FirstSegment as EntitySetSegment;
                if (entitySetSegment != null)
                {
                    controllerName = entitySetSegment.EntitySet.Name;
                }

                var keySegment = odataPath.FirstOrDefault(s => s is KeySegment) as KeySegment;
                if (keySegment != null)
                {
                    keyCount = keySegment.Keys.Count();

                    foreach (var kvp in keySegment.Keys)
                    {
                        context.RouteData.Values[kvp.Key] = kvp.Value;
                    }
                }

                var navigationPropertySegment =
                    odataPath.FirstOrDefault(s => s is NavigationPropertySegment) as NavigationPropertySegment;
                if (navigationPropertySegment != null)
                {
                    actionName += navigationPropertySegment.NavigationProperty.Name;
                }
            }
            
            var services = context.HttpContext.ApplicationServices;
            var provider = services.GetRequiredService<IActionDescriptorsCollectionProvider>();
            var actionDescriptor = (ControllerActionDescriptor)provider.ActionDescriptors.Items.SingleOrDefault(d =>
            {
                var c = d as ControllerActionDescriptor;
                return c != null
                && c.ControllerName == controllerName
                && c.Name == actionName
                && c.Parameters.Count == keyCount;
            });

            return actionDescriptor;
        }
    }
}