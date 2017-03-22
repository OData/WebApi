// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public class AttributeRoutingConvention : IODataRoutingConvention
    {
        private IDictionary<ODataPathTemplate, ControllerActionDescriptor> _attributeMappings;

        public AttributeRoutingConvention()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="pathTemplateHandler"></param>
        /// <param name="actionCollectionProvider"></param>
        public AttributeRoutingConvention(IODataPathTemplateHandler pathTemplateHandler, IActionDescriptorCollectionProvider actionCollectionProvider)
        {
            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            if (actionCollectionProvider == null)
            {
                throw Error.ArgumentNull("actionCollectionProvider");
            }

            ODataPathTemplateHandler = pathTemplateHandler;
            ActionDescriptorCollectionProvider = actionCollectionProvider;
        }

        /// <summary>
        /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
        /// </summary>
        public IODataPathTemplateHandler ODataPathTemplateHandler { get; private set; }

        /// <summary>
        /// Gets the <see cref="IActionDescriptorCollectionProvider"/> to be used for collecting the controllers.
        /// </summary>
        public IActionDescriptorCollectionProvider ActionDescriptorCollectionProvider { get; private set; }

        /// <summary>
        /// Specifies whether OData route attributes on this controller should be mapped.
        /// This method will execute before the derived type's instance constructor executes. Derived types must
        /// be aware of this and should plan accordingly. For example, the logic in ShouldMapController() should be simple
        /// enough so as not to depend on the "this" pointer referencing a fully constructed object.
        /// </summary>
        /// <param name="controllerActionDescriptor">The controller and action descriptor.</param>
        /// <returns><c>true</c> if this controller should be included in the map; <c>false</c> otherwise.</returns>
        public virtual bool ShouldMapController(ControllerActionDescriptor controllerActionDescriptor)
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual ActionDescriptor SelectAction(RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            if (_attributeMappings == null)
            {
                if (ActionDescriptorCollectionProvider == null)
                {
                    ActionDescriptorCollectionProvider =
                        routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>
                            ();
                }

                if (ODataPathTemplateHandler == null)
                {
                    ODataPathTemplateHandler =
                        routeContext.HttpContext.RequestServices.GetRequiredService<IODataPathTemplateHandler>();
                }

                IEdmModel model = routeContext.HttpContext.ODataFeature().Model;

                IEnumerable<ControllerActionDescriptor> actionDescriptors =
                    ActionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();

                _attributeMappings = BuildAttributeMappings(actionDescriptors, model);
            }

            HttpRequest request = routeContext.HttpContext.Request;
            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            Dictionary<string, object> values = new Dictionary<string, object>();

            var routeData = routeContext.RouteData;
            var routingConventionsStore = request.ODataFeature().RoutingConventionsStore;

            foreach (KeyValuePair<ODataPathTemplate, ControllerActionDescriptor> attributeMapping in _attributeMappings)
            {
                ODataPathTemplate template = attributeMapping.Key;
                ControllerActionDescriptor actionDescriptor = attributeMapping.Value;

                if (IsHttpMethodMatch(actionDescriptor, request.Method) && template.TryMatch(odataPath, values))
                {
                    foreach (var item in values)
                    {
                        if (item.Key.StartsWith(ODataParameterValue.ParameterValuePrefix, StringComparison.Ordinal) &&
                            item.Value is ODataParameterValue)
                        {
                            routingConventionsStore.Add(item);
                        }
                        else
                        {
                            routeData.Values.Add(item.Key, item.Value);
                        }
                    }

                    return actionDescriptor;
                }
            }

            return null;
        }

        private static bool IsHttpMethodMatch(ControllerActionDescriptor descriptor, string httpMethod)
        {
            return descriptor.ActionConstraints?
                            .Where(x => x.GetType() == typeof(HttpMethodActionConstraint))
                            .SelectMany(h => (h as HttpMethodActionConstraint).HttpMethods)
                            .Any(h => h == httpMethod) ?? httpMethod == ODataRouteConstants.HttpGet;
        }

        private static IEnumerable<string> GetODataRoutePrefixes(ControllerActionDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerDescriptor != null);

            ODataRouteAttribute[] prefixAttributes = controllerDescriptor.ControllerTypeInfo.GetCustomAttributes<ODataRouteAttribute>(inherit: false).ToArray();
            if (!prefixAttributes.Any())
            {
                yield return null;
            }
            else
            {
                foreach (ODataRouteAttribute prefixAttribute in prefixAttributes)
                {
                    string prefix = prefixAttribute.PathTemplate;

                    if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix, controllerDescriptor.ControllerTypeInfo.FullName);
                    }

                    if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        prefix = prefix.TrimEnd('/');
                    }

                    yield return prefix;
                }
            }
        }

        private IEnumerable<ODataPathTemplate> GetODataPathTemplates(string prefix, ControllerActionDescriptor descriptor, IEdmModel mode)
        {
            Contract.Assert(descriptor != null);

            IEnumerable<ODataRouteAttribute> routeAttributes = descriptor.MethodInfo.GetCustomAttributes<ODataRouteAttribute>(inherit: false);
            return
                routeAttributes
                .Select(route => GetODataPathTemplate(prefix, route.PathTemplate, descriptor, mode))
                .Where(template => template != null);
        }

        private ODataPathTemplate GetODataPathTemplate(string prefix, string pathTemplate, ControllerActionDescriptor action, IEdmModel model)
        {
            if (prefix != null && !pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                if (String.IsNullOrEmpty(pathTemplate))
                {
                    pathTemplate = prefix;
                }
                else if (pathTemplate.StartsWith("(", StringComparison.Ordinal))
                {
                    // We don't need '/' when the pathTemplate starts with a key segment.
                    pathTemplate = prefix + pathTemplate;
                }
                else
                {
                    pathTemplate = prefix + "/" + pathTemplate;
                }
            }

            if (pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                pathTemplate = pathTemplate.Substring(1);
            }

            ODataPathTemplate odataPathTemplate;

            try
            {
                //IEdmModel model = new EdmModel(); // TODO:
                odataPathTemplate = ODataPathTemplateHandler.ParseTemplate(model, pathTemplate);
            }
            catch (ODataException e)
            {
                /*
                throw Error.InvalidOperation(SRResources.InvalidODataRouteOnAction, pathTemplate, action.ActionName,
                    action.ControllerDescriptor.ControllerName, e.Message);*/
                // TODO: 
                return null;
            }

            return odataPathTemplate;
        }

        // TODO: do we need to use the [FromServices]
        //private IDictionary<ODataPathTemplate, ActionDescriptor> BuildAttributeMappings([FromServices] IActionDescriptorCollectionProvider actionCollectionProvider)
        private IDictionary<ODataPathTemplate, ControllerActionDescriptor> BuildAttributeMappings(IEnumerable<ControllerActionDescriptor> actionDescriptors,
            IEdmModel model)
        {
            Dictionary<ODataPathTemplate, ControllerActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, ControllerActionDescriptor>();

            foreach (ControllerActionDescriptor actionDescriptor in actionDescriptors)
            {
                if (!ShouldMapController(actionDescriptor))
                {
                    continue;
                }

                foreach (string prefix in GetODataRoutePrefixes(actionDescriptor))
                {
                    IEnumerable<ODataPathTemplate> pathTemplates = GetODataPathTemplates(prefix, actionDescriptor, model);
                    foreach (ODataPathTemplate pathTemplate in pathTemplates)
                    {
                        attributeMappings.Add(pathTemplate, actionDescriptor);
                    }
                }
            }

            return attributeMappings;
        }
    }
}
