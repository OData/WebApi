using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    public class DefaultODataRoutingConvention : IODataRoutingConvention
    {
        private static readonly IDictionary<string, string> _actionNameMappings = new Dictionary<string, string>()
        {
            {"GET", "Get"},
            {"POST", "Post"},
            {"PUT", "Put"},
            {"DELETE", "Delete"}
        };

        public ActionDescriptor SelectAction(RouteContext routeContext)
        {
            var odataPath = routeContext.HttpContext.Request.ODataProperties().NewPath;
            var controllerName = string.Empty;
            var methodName = routeContext.HttpContext.Request.Method;
            var routeTemplate = string.Empty;
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

                if (keys.Count == 1)
                {
                    routeTemplate = "{id}";
                }

                var structuralPropertySegment =
                    odataPath.FirstOrDefault((s => s is PropertySegment)) as PropertySegment;
                if (structuralPropertySegment != null)
                {
                    routeTemplate += "/" + structuralPropertySegment.Property.Name;
                }

                var navigationPropertySegment =
                    odataPath.FirstOrDefault(s => s is NavigationPropertySegment) as NavigationPropertySegment;
                if (navigationPropertySegment != null)
                {
                    routeTemplate += "/" + navigationPropertySegment.NavigationProperty.Name;
                }
            }

            if (string.IsNullOrEmpty(routeTemplate))
            {
                routeTemplate = controllerName;
            }
            else
            {
                routeTemplate = controllerName + "/" + routeTemplate;
            }
            
            var services = routeContext.HttpContext.ApplicationServices;
            var provider = services.GetRequiredService<IActionDescriptorsCollectionProvider>();
            var actionDescriptor = provider.ActionDescriptors.Items.SingleOrDefault(d =>
            {
                var c = d as ControllerActionDescriptor;
                return c != null && c.ControllerName == controllerName &&
                    (controllerName == "Metadata" ||
                        ((HttpMethodConstraint)c.ActionConstraints.First()).HttpMethods.Contains(methodName) &&
                         c.AttributeRouteInfo.Template.EndsWith(routeTemplate));
            });

            if (actionDescriptor == null)
            {
                throw new NotSupportedException(string.Format("No action match template '{0}' in '{1}Controller'", routeTemplate, controllerName));
            }

            if (keys.Any())
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