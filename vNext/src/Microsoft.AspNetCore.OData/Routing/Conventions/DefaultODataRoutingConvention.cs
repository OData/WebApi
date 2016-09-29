// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;

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
            var odataPath = routeContext.HttpContext.Request.ODataFeature().Path;
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

                var operationImportSegment = odataPath.FirstSegment as OperationImportSegment;
                if (operationImportSegment != null)
                {
                    // Handling unbound functions without related entity set
                    controllerName = operationImportSegment.EntitySet != null ?
                        operationImportSegment.EntitySet.Name : routeContext.HttpContext.Request.ODataFeature().RoutePrefix;
                    var edmOperationImport = operationImportSegment.OperationImports.FirstOrDefault();
                    if (edmOperationImport != null)
                    {
                        routeTemplate = edmOperationImport.Name;
                        methodName = edmOperationImport.Name;
                    }

                    foreach (var operationSegmentParameter in operationImportSegment.Parameters)
                    {
                        var keyName = operationSegmentParameter.Name;
                        var convertNode = operationSegmentParameter.Value as ConvertNode;
                        var constantNode = operationSegmentParameter.Value as ConstantNode;
                        object keyValue = null;

                        if (constantNode != null)
                        {
                            keyValue = constantNode.Value;
                        }

                        if (convertNode != null)
                        {
                            keyValue = ((ConstantNode)convertNode.Source).Value;
                        }

                        var newKey = new KeyValuePair<string, object>(keyName, keyValue);
                        keys.Add(newKey);
                    }
                }

                var operationSegment = odataPath.LastSegment as OperationSegment;
                if (operationSegment != null)
                {
                    var edmOperationImport = operationSegment.Operations.FirstOrDefault();
                    if (edmOperationImport != null)
                    {
                        routeTemplate = edmOperationImport.Name;
                        methodName = edmOperationImport.Name;
                    }

                    foreach (var operationSegmentParameter in operationSegment.Parameters)
                    {
                        var keyName = operationSegmentParameter.Name;
                        var keyValue =
                            ((ConstantNode)operationSegmentParameter.Value)
                                .Value;

                        var newKey = new KeyValuePair<string, object>(keyName, keyValue);
                        keys.Add(newKey);
                    }
                }

                var keySegment = odataPath.FirstOrDefault(s => s is KeySegment) as KeySegment;
                if (keySegment != null)
                {
                    keys.AddRange(keySegment.Keys);
                }

                if (keys.Count == 1 && operationImportSegment == null)
                {
                    routeTemplate = "{id}";
                }

                var structuralPropertySegment =
                    odataPath.FirstOrDefault((s => s is PropertySegment)) as PropertySegment;
                if (structuralPropertySegment != null)
                {
                    routeTemplate += "/" + structuralPropertySegment.Property.Name;
                    methodName += structuralPropertySegment.Property.Name;
                }

                var navigationPropertySegment =
                    odataPath.FirstOrDefault(s => s is NavigationPropertySegment) as NavigationPropertySegment;
                if (navigationPropertySegment != null)
                {
                    routeTemplate += "/" + navigationPropertySegment.NavigationProperty.Name;
                }

                var navigationPropertyLinkSegment =
                    odataPath.FirstOrDefault(s => s is NavigationPropertyLinkSegment) as NavigationPropertyLinkSegment;
                if (navigationPropertyLinkSegment != null)
                {
                    routeTemplate += "/" + navigationPropertyLinkSegment.NavigationProperty.Name;
                    methodName = "CreateRef";
                    var navigationKey = new KeyValuePair<string, object>("navigationProperty", navigationPropertyLinkSegment.NavigationProperty.Name);
                    keys.Add(navigationKey);
                    //var navigationSourceKey = new KeyValuePair<string, object>("navigationSource", navigationPropertyLinkSegment.NavigationSource.Name);
                    //keys.Add(navigationSourceKey);
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
            var provider = services.GetRequiredService<IActionDescriptorCollectionProvider>();

            var methodDescriptor = new List<ActionDescriptor>();
            ActionDescriptor actionDescriptor = null;

            // Find all the matching methods
            foreach (var descriptor in provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>())
            {
                if (string.Equals(descriptor.ActionName, methodName, StringComparison.OrdinalIgnoreCase)
                    && descriptor.ControllerName == controllerName)
                {
                    methodDescriptor.Add(descriptor);
                }
            }

            // Now match the parameters
            foreach (var descriptor in methodDescriptor)
            {
                bool matchFound = true;
                if (descriptor.Parameters.Count(d => d.BindingInfo == null) == keys.Count)
                {
                    foreach (var key in keys)
                    {
                        if (descriptor.Parameters.FirstOrDefault(d => d.Name.Equals(key.Key, StringComparison.OrdinalIgnoreCase)) != null)
                        {
                            continue;
                        }
                        matchFound = false;
                        break;
                    }
                }
                else
                {
                    matchFound = false;
                }

                if (!matchFound)
                {
                    continue;
                }

                actionDescriptor = descriptor;
                break;
            }

            if (actionDescriptor == null)
            {
                throw new NotSupportedException(string.Format("No action match template '{0}' in '{1}Controller'", routeTemplate, controllerName));
            }

            if (keys.Any())
            {
                this.WriteRouteData(routeContext, actionDescriptor.Parameters, keys);
            }

            return actionDescriptor;
        }

        private void WriteRouteData(RouteContext context, IList<ParameterDescriptor> parameters, IList<KeyValuePair<string, object>> keys)
        {
            foreach (var key in keys)
            {
                var param = parameters.FirstOrDefault(p => p.Name.Equals(key.Key, StringComparison.OrdinalIgnoreCase));
                if (param == null)
                {
                    continue;
                }

                context.RouteData.Values.Add(param.Name, key.Value);
            }
        }
    }
}