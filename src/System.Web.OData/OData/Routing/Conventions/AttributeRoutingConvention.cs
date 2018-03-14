﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using System.Web.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public class AttributeRoutingConvention : IODataRoutingConvention
    {
        private static readonly DefaultODataPathHandler _defaultPathHandler = new DefaultODataPathHandler();

        private readonly string _routeName;

        private IDictionary<ODataPathTemplate, HttpActionDescriptor> _attributeMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        public AttributeRoutingConvention(string routeName, HttpConfiguration configuration)
            : this(routeName, configuration, _defaultPathHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        public AttributeRoutingConvention(string routeName, HttpConfiguration configuration,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(routeName, pathTemplateHandler)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            IODataPathHandler pathHandler = pathTemplateHandler as IODataPathHandler;
            // if settings is not on local, use the global configuration settings.
            if (pathHandler != null && pathHandler.UrlKeyDelimiter == null)
            {
                ODataUrlKeyDelimiter urlKeyDelimiter = configuration.GetUrlKeyDelimiter();
                pathHandler.UrlKeyDelimiter = urlKeyDelimiter;
            }

            Action<HttpConfiguration> oldInitializer = configuration.Initializer;
            bool initialized = false;
            configuration.Initializer = (config) =>
            {
                if (!initialized)
                {
                    initialized = true;
                    oldInitializer(config);
                    IHttpControllerSelector controllerSelector = config.Services.GetHttpControllerSelector();
                    _attributeMappings = BuildAttributeMappings(controllerSelector.GetControllerMapping().Values);
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        public AttributeRoutingConvention(string routeName,
            IEnumerable<HttpControllerDescriptor> controllers)
            : this(routeName, controllers, _defaultPathHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "See note on <see cref=\"ShouldMapController()\"> method.")]
        public AttributeRoutingConvention(string routeName,
            IEnumerable<HttpControllerDescriptor> controllers,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(routeName, pathTemplateHandler)
        {
            if (controllers == null)
            {
                throw Error.ArgumentNull("controllers");
            }

            _attributeMappings = BuildAttributeMappings(controllers);
        }

        private AttributeRoutingConvention(string routeName, IODataPathTemplateHandler pathTemplateHandler)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            _routeName = routeName;
            ODataPathTemplateHandler = pathTemplateHandler;
        }

        /// <summary>
        /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
        /// </summary>
        public IODataPathTemplateHandler ODataPathTemplateHandler { get; private set; }

        internal IDictionary<ODataPathTemplate, HttpActionDescriptor> AttributeMappings
        {
            get
            {
                if (_attributeMappings == null)
                {
                    // Will throw an InvalidOperationException if this class is constructed with an HttpConfiguration
                    // but EnsureInitialized() hasn't been called yet.
                    throw Error.InvalidOperation(SRResources.Object_NotYetInitialized);
                }

                return _attributeMappings;
            }
        }

        /// <summary>
        /// Specifies whether OData route attributes on this controller should be mapped.
        /// This method will execute before the derived type's instance constructor executes. Derived types must
        /// be aware of this and should plan accordingly. For example, the logic in ShouldMapController() should be simple
        /// enough so as not to depend on the "this" pointer referencing a fully constructed object.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns><c>true</c> if this controller should be included in the map; <c>false</c> otherwise.</returns>
        public virtual bool ShouldMapController(HttpControllerDescriptor controller)
        {
            return true;
        }

        /// <inheritdoc />
        public string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (KeyValuePair<ODataPathTemplate, HttpActionDescriptor> attributeMapping in AttributeMappings)
            {
                ODataPathTemplate template = attributeMapping.Key;
                HttpActionDescriptor action = attributeMapping.Value;

                if (action.SupportedHttpMethods.Contains(request.Method) && template.TryMatch(odataPath, values))
                {
                    values["action"] = action.ActionName;
                    request.Properties["AttributeRouteData"] = values;

                    return action.ControllerDescriptor.ControllerName;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
        {
            var routeData = controllerContext.Request.GetRouteData();
            var routingConventionsStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;

            object value;
            if (controllerContext.Request.Properties.TryGetValue("AttributeRouteData", out value))
            {
                IDictionary<string, object> attributeRouteData = value as IDictionary<string, object>;
                if (attributeRouteData != null)
                {
                    foreach (var item in attributeRouteData)
                    {
                        if (item.Key.StartsWith(ODataParameterValue.ParameterValuePrefix, StringComparison.Ordinal) &&
                            item.Value is ODataParameterValue)
                        {
                            routingConventionsStore.Add(item);
                        }
                        else
                        {
                            routeData.Values.Add(item);
                        }
                    }

                    return attributeRouteData["action"] as string;
                }
            }

            return null;
        }

        private IDictionary<ODataPathTemplate, HttpActionDescriptor> BuildAttributeMappings(IEnumerable<HttpControllerDescriptor> controllers)
        {
            Dictionary<ODataPathTemplate, HttpActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, HttpActionDescriptor>();

            foreach (HttpControllerDescriptor controller in controllers)
            {
                if (IsODataController(controller) && ShouldMapController(controller))
                {
                    IHttpActionSelector actionSelector = controller.Configuration.Services.GetActionSelector();
                    ILookup<string, HttpActionDescriptor> actionMapping = actionSelector.GetActionMapping(controller);
                    HttpActionDescriptor[] actions = actionMapping.SelectMany(a => a).ToArray();

                    foreach (string prefix in GetODataRoutePrefixes(controller))
                    {
                        foreach (HttpActionDescriptor action in actions)
                        {
                            IEnumerable<ODataPathTemplate> pathTemplates = GetODataPathTemplates(prefix, action);
                            foreach (ODataPathTemplate pathTemplate in pathTemplates)
                            {
                                attributeMappings.Add(pathTemplate, action);
                            }
                        }
                    }
                }
            }

            return attributeMappings;
        }

        private static bool IsODataController(HttpControllerDescriptor controller)
        {
            return typeof(ODataController).IsAssignableFrom(controller.ControllerType);
        }

        private static IEnumerable<string> GetODataRoutePrefixes(HttpControllerDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerDescriptor != null);

            var prefixAttributes = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>(inherit: false);
            if (prefixAttributes.Count == 0)
            {
                yield return null;
            }
            else
            {
                foreach (ODataRoutePrefixAttribute prefixAttribute in prefixAttributes)
                {
                    string prefix = prefixAttribute.Prefix;

                    if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix, controllerDescriptor.ControllerType.FullName);
                    }

                    if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        prefix = prefix.TrimEnd('/');
                    }

                    yield return prefix;
                }
            }
        }

        private IEnumerable<ODataPathTemplate> GetODataPathTemplates(string prefix, HttpActionDescriptor action)
        {
            Contract.Assert(action != null);

            IEnumerable<ODataRouteAttribute> routeAttributes = action.GetCustomAttributes<ODataRouteAttribute>(inherit: false);
            return
                routeAttributes
                .Select(route => GetODataPathTemplate(prefix, route.PathTemplate, action))
                .Where(template => template != null);
        }

        private ODataPathTemplate GetODataPathTemplate(string prefix, string pathTemplate, HttpActionDescriptor action)
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
                // We are NOT in a request but establishing the attribute routing convention.
                // So use the root container rather than the request container.
                odataPathTemplate = ODataPathTemplateHandler.ParseTemplate(pathTemplate,
                    action.Configuration.GetODataRootContainer(_routeName));
            }
            catch (ODataException e)
            {
                throw Error.InvalidOperation(SRResources.InvalidODataRouteOnAction, pathTemplate, action.ActionName,
                    action.ControllerDescriptor.ControllerName, e.Message);
            }

            return odataPathTemplate;
        }
    }
}
