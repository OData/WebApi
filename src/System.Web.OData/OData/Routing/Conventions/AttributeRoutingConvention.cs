// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
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

        private IDictionary<ODataPathTemplate, HttpActionDescriptor> _attributeMappings;
        private Func<IDictionary<ODataPathTemplate, HttpActionDescriptor>> _attributeMappingsFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> to be used for parsing the route templates.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use for figuring out all the controllers to 
        /// look for a match.</param>
        public AttributeRoutingConvention(IEdmModel model, HttpConfiguration configuration)
            : this(model, configuration, _defaultPathHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> to be used for parsing the route templates.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use for figuring out all the controllers to 
        /// look for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        public AttributeRoutingConvention(IEdmModel model, HttpConfiguration configuration,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(model, pathTemplateHandler)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            Action<HttpConfiguration> oldInitializer = configuration.Initializer;
            configuration.Initializer = (config) =>
            {
                oldInitializer(config);
                IHttpControllerSelector controllerSelector = config.Services.GetHttpControllerSelector();
                _attributeMappingsFunc =
                    () => BuildAttributeMappings(controllerSelector.GetControllerMapping().Values);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> to be used for parsing the route templates.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        public AttributeRoutingConvention(IEdmModel model, IEnumerable<HttpControllerDescriptor> controllers)
            : this(model, controllers, _defaultPathHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> to be used for parsing the route templates.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        public AttributeRoutingConvention(IEdmModel model, IEnumerable<HttpControllerDescriptor> controllers,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(model, pathTemplateHandler)
        {
            if (controllers == null)
            {
                throw Error.ArgumentNull("controllers");
            }

            _attributeMappingsFunc =
                 () => BuildAttributeMappings(controllers);
        }

        private AttributeRoutingConvention(IEdmModel model, IODataPathTemplateHandler pathTemplateHandler)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            Model = model;
            ODataPathTemplateHandler = pathTemplateHandler;
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> to be used for parsing the route templates.
        /// </summary>
        public IEdmModel Model { get; private set; }

        /// <summary>
        /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
        /// </summary>
        public IODataPathTemplateHandler ODataPathTemplateHandler { get; private set; }

        internal IDictionary<ODataPathTemplate, HttpActionDescriptor> AttributeMappings
        {
            get
            {
                if (_attributeMappingsFunc == null)
                {
                    throw Error.InvalidOperation(SRResources.Object_NotYetInitialized);
                }

                _attributeMappings = _attributeMappings ?? _attributeMappingsFunc();
                return _attributeMappings;
            }
        }

        /// <summary>
        /// Specifies whether OData route attributes on this controller should be mapped.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns><see langword="true"/> if this controller should be included in the map; <see langword="false"/> otherwise.</returns>
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

            object value;
            if (controllerContext.Request.Properties.TryGetValue("AttributeRouteData", out value))
            {
                IDictionary<string, object> attributeRouteData = value as IDictionary<string, object>;
                if (attributeRouteData != null)
                {
                    foreach (var item in attributeRouteData)
                    {
                        routeData.Values.Add(item);
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
                    IEnumerable<HttpActionDescriptor> actions = actionMapping.SelectMany(a => a);
                    string prefix = GetODataRoutePrefix(controller);

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

            return attributeMappings;
        }

        private static bool IsODataController(HttpControllerDescriptor controller)
        {
            return typeof(ODataController).IsAssignableFrom(controller.ControllerType);
        }

        private static string GetODataRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerDescriptor != null);

            string prefix = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>(inherit: false)
                .Select(prefixAttribute => prefixAttribute.Prefix)
                .SingleOrDefault();

            if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
            {
                throw Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix, controllerDescriptor.ControllerType.FullName);
            }

            if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
            {
                prefix = prefix.TrimEnd('/');
            }

            return prefix;
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

            ODataPathTemplate odataPathTemplate = null;

            try
            {
                odataPathTemplate = ODataPathTemplateHandler.ParseTemplate(Model, pathTemplate);
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
