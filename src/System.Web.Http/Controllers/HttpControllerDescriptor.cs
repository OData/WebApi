// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Internal;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Description and configuration for a controller.
    /// </summary>
    public class HttpControllerDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private HttpConfiguration _configuration;

        private string _controllerName;
        private Type _controllerType;

        private MediaTypeFormatterCollection _formatters;
        private ParameterBindingRulesCollection _parameterBindings;

        private ControllerServices _controllerServices;

        private object[] _attrCached;

        public HttpControllerDescriptor(HttpConfiguration configuration, string controllerName, Type controllerType)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (controllerName == null)
            {
                throw Error.ArgumentNull("controllerName");
            }

            if (controllerType == null)
            {
                throw Error.ArgumentNull("controllerType");
            }
            
            _configuration = configuration;
            _controllerName = controllerName;
            _controllerType = controllerType;

            _controllerServices = new ControllerServices(_configuration.Services);
            
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpControllerDescriptor()
        {
        }

        // For unit testing purposes. 
        internal HttpControllerDescriptor(HttpConfiguration configuration)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public virtual ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Global configuration. Controller can override services, so check properties from controller descriptor instead of configuration. 
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _configuration = value;
            }
        }

        public string ControllerName
        {
            get { return _controllerName; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerName = value;
            }
        }

        public Type ControllerType
        {
            get { return _controllerType; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerType = value;
            }
        }

        /// <summary>
        /// Per-controller services. This can override the services found in the global configuration's default services.
        /// </summary>
        public ControllerServices ControllerServices
        {
            get { return _controllerServices; }
        }

        /// <summary>
        /// Get the parameter binding rules for this controller.
        /// To override these to be separate from the global config, set the collection to a new instance.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "protected setter needed for tracers and unit tests")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't let the debugger evaluate a lazy-init property
        public ParameterBindingRulesCollection ParameterBindingRules
        {
            get 
            {
                if (_parameterBindings == null)
                {
                    if (Configuration == null)
                    {
                        return null; // unit tests will call with null configuration
                    }
                    // If this is null, then copy it from the global config.
                    _parameterBindings = new ParameterBindingRulesCollection();
                    foreach (var rule in Configuration.ParameterBindingRules)
                    {
                        _parameterBindings.Add(rule);
                    }
                }
                return _parameterBindings; 
            }  
            protected set
            {
                _parameterBindings = value;
            }
        }

        /// <summary>
        /// Gets the media type formatters.
        /// To override these to be separate from the global config, set the collection to a new instance.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "protected setter needed for tracers and unit tests")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't let the debugger evaluate a lazy-init property
        public MediaTypeFormatterCollection Formatters
        {
            get 
            {
                if (Configuration == null)
                {
                    return null; // unit tests will call with null configuration
                }
                if (_formatters == null)
                {
                    _formatters = new MediaTypeFormatterCollection(Configuration.Formatters);
                }
                return _formatters; 
            }
            protected set
            {
                _formatters = value;
            }
        }

        /// <summary>
        /// Creates a controller instance for the given <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns></returns>
        public virtual IHttpController CreateController(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // Invoke the controller activator
            IHttpControllerActivator activator = ControllerServices.GetHttpControllerActivator();
            IHttpController instance = activator.Create(request, this, ControllerType);
            return instance;
        }

        /// <summary>
        /// Returns the collection of <see cref="IFilter">filters</see> associated with this descriptor's controller.
        /// </summary>
        /// <remarks>The default implementation calls <see cref="GetCustomAttributes{IFilter}()"/>.</remarks>
        /// <returns>A collection of filters associated with this controller.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Filters can be built dynamically")]
        public virtual Collection<IFilter> GetFilters()
        {
            return GetCustomAttributes<IFilter>();
        }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T"/> for this descriptor's controller.
        /// </summary>
        /// <remarks>The default implementation retrieves the matching set of attributes declared on <see cref="ControllerType"/>.</remarks>
        /// <typeparam name="T">Used to filter the collection of attributes. Use a value of <see cref="Object"/> to retrieve all attributes.</typeparam>
        /// <returns>A collection of attributes associated with this controller.</returns>
        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            // Getting custom attributes via reflection is slow. 
            // But iterating over a object[] to pick out specific types is fast. 
            // Furthermore, many different services may call to ask for different attributes, so we have multiple callers. 
            // That means there's not a single cache for the callers, which means there's some value caching here.
            if (_attrCached == null)
            {
                // Even in a race, we'll just ask for the custom attributes twice.
                _attrCached = ControllerType.GetCustomAttributes(inherit: true);
            }

            return new Collection<T>(TypeHelper.OfType<T>(_attrCached));
        }

        // For unit tests for initializing mock objects. Controller may not have a type, so we can't do the normal Initialize() path. 
        internal void Initialize(HttpConfiguration configuration)
        {
            _configuration = configuration;
            _controllerServices = new ControllerServices(_configuration.Services);
            FinishInitialize();
        }

        // Initialize the Controller Descriptor. This invokes all IControllerConfiguration attributes
        // on the controller type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnControllerType(this, ControllerType);
            FinishInitialize();
        }
                
        private void FinishInitialize()
        {
            // If initialization didn't override properties, then set those to point at the global configuration           
            // Be sure to check against fields rather than properties to avoid triggering a lazy copy of the global config.
            if (_formatters == null)
            {
                _formatters = Configuration.Formatters;
            }
            if (_parameterBindings == null)
            {
                _parameterBindings = Configuration.ParameterBindingRules;
            }
        }

        // Helper to invoke any Controller config attributes on this controller type or its base classes.
        private static void InvokeAttributesOnControllerType(HttpControllerDescriptor controllerDescriptor, Type type)
        {
            Contract.Assert(controllerDescriptor != null);

            if (type == null)
            {
                return;
            }
            // Initialize base class before derived classes (same order as ctors).
            InvokeAttributesOnControllerType(controllerDescriptor, type.BaseType);

            // Check for attribute
            object[] attrs = type.GetCustomAttributes(inherit: false);
            foreach (object attr in attrs)
            {
                var init = attr as IControllerConfiguration;
                if (init != null)
                {
                    init.Initialize(controllerDescriptor);
                }
            }
        }
    }
}
