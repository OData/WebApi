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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.ActionConstraints;

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
            var preflightFor =
                routeContext.HttpContext.Request.Headers["Access-Control-Request-Method"].FirstOrDefault();
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
            
            var services = routeContext.HttpContext.RequestServices;
            var provider = services.GetRequiredService<IActionDescriptorsCollectionProvider>();
            var actionDescriptor = provider.ActionDescriptors.Items.SingleOrDefault(d =>
            {
                var c = d as ControllerActionDescriptor;
                if (c == null)
                {
                    return false;
                }
                if (c.ControllerName != controllerName)
                {
                    return false;
                }
                if (controllerName == "Metadata")
                {
                    return true;
                }
                if (!c.AttributeRouteInfo.Template.EndsWith(routeTemplate))
                {
                    return false;
                }
                var httpMethodConstraint = ((HttpMethodConstraint)c.ActionConstraints.First());
                if (methodName.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    return httpMethodConstraint.HttpMethods.Contains(preflightFor);
                }
                return httpMethodConstraint.HttpMethods.Contains(methodName);
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
            for (int i = 0; i < keys.Count; ++i)
            {
                // TODO: check if parameters match keys.
                context.RouteData.Values[parameters[i].Name] = keys[i].Value;
            }
        }
    }
}