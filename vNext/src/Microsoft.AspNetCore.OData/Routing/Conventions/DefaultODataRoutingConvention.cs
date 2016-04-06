using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Core.UriParser.Semantic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
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

				// TODO: Move all these out into separate processor classes for each type
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

				var operationSegment =
					odataPath.FirstOrDefault(s => s is OperationSegment) as OperationSegment;
				if (operationSegment != null)
				{
					routeTemplate += "/" + operationSegment.Operations.First().Name;
					routeTemplate = ApplyParameters(operationSegment.Parameters, keys, routeTemplate);
				}

				var operationImportSegment =
					odataPath.FirstOrDefault(s => s is OperationImportSegment) as OperationImportSegment;
				if (operationImportSegment != null)
				{
					controllerName = "*";
					routeTemplate = "/" + operationImportSegment.OperationImports.First().Name;
					routeTemplate = ApplyParameters(operationImportSegment.Parameters, keys, routeTemplate);
				}
			}

			if (string.IsNullOrEmpty(routeTemplate))
			{
				routeTemplate = controllerName;
			}
			else
			{
				routeTemplate = controllerName + "/" + routeTemplate.TrimStart('/');
			}

			routeTemplate = routeTemplate.TrimStart('*');

			var services = routeContext.HttpContext.RequestServices;
			var provider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
			var actionDescriptor = provider.ActionDescriptors.Items.SingleOrDefault(d =>
			{
				var c = d as ControllerActionDescriptor;
				var isUs = false;
				//if (d.DisplayName == "ODataSample.Web.Controllers.ProductsController.PostName (ODataSample.Web)")
				//{
				//	isUs = true;
				//}
				if (c == null)
				{
					return false;
				}
				if (c.ControllerName != controllerName && controllerName != "*")
				{
					return false;
				}
				if (controllerName == "Metadata")
				{
					return true;
				}
				if (c.AttributeRouteInfo == null)
				{
					return false;
				}
				if (!c.AttributeRouteInfo.Template.EndsWith(routeTemplate))
				{
					//if (isUs)
					//{
					//	throw new Exception(c.AttributeRouteInfo.Template);
					//}
					return false;
				}
				// If we find no action constraints, this isn't our method
				if (c.ActionConstraints == null || !c.ActionConstraints.Any())
				{
					return false;
				}
				// TODO: If this is a OperationSegment or an OperationImportSegment then check the return types match
				var httpMethodConstraint = ((HttpMethodActionConstraint)c.ActionConstraints.First());
				if (methodName.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
				{
					return httpMethodConstraint.HttpMethods.Contains(preflightFor);
				}
				return httpMethodConstraint.HttpMethods.Contains(methodName);
			});

			if (actionDescriptor == null)
			{
				throw new NotSupportedException($"No action match template '{routeTemplate}' in '{controllerName}Controller'");
			}

			if (keys.Any())
			{
				WriteRouteData(routeContext, actionDescriptor.Parameters, keys);
			}

			return actionDescriptor;
		}

	    private static string ApplyParameters(IEnumerable<OperationSegmentParameter> segmentParameters, List<KeyValuePair<string, object>> keys, string routeTemplate)
	    {
		    var parameters = new List<string>();
		    if (segmentParameters != null && segmentParameters.Any())
		    {
			    foreach (var param in segmentParameters)
			    {
				    parameters.Add(string.Format("{0}={{{0}}}", param.Name));
					var value = ResolveValue(param.Value as SingleValueNode);
					keys.Add(new KeyValuePair<string, object>(param.Name, value));
			    }
			    routeTemplate += "(" + string.Join(",", parameters) + ")";
		    }
		    return routeTemplate;
	    }

	    private static object ResolveValue(SingleValueNode node)
	    {
			var constantNode = node as ConstantNode;
			if (constantNode != null)
			{
				return constantNode.Value;
			}
			var convertNode = node as ConvertNode;
			if (convertNode != null)
			{
				return ResolveValue(convertNode.Source);
			}
		    return null;
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