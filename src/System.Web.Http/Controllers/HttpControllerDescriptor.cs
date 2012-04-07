// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Internal;

namespace System.Web.Http.Controllers
{
    public class HttpControllerDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private HttpConfiguration _configuration;
        private string _controllerName;
        private Type _controllerType;
        private IHttpControllerActivator _controllerActivator;
        private IHttpActionSelector _actionSelector;
        private IHttpActionInvoker _actionInvoker;
        private IActionValueBinder _actionValueBinder;

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

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpControllerDescriptor()
        {
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

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

        public IHttpControllerActivator HttpControllerActivator
        {
            get { return _controllerActivator; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerActivator = value;
            }
        }

        public IHttpActionSelector HttpActionSelector
        {
            get { return _actionSelector; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionSelector = value;
            }
        }

        public IHttpActionInvoker HttpActionInvoker
        {
            get { return _actionInvoker; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionInvoker = value;
            }
        }

        public IActionValueBinder ActionValueBinder
        {
            get { return _actionValueBinder; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionValueBinder = value;
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
            IHttpController instance = HttpControllerActivator.Create(request, this, ControllerType);
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

        private void Initialize()
        {
            // Look for attribute to provide specialized information for this controller type
            HttpControllerConfigurationAttribute controllerConfig =
                _controllerType.GetCustomAttributes<HttpControllerConfigurationAttribute>(inherit: true).FirstOrDefault();

            // If we find attribute then first ask dependency resolver and if we get null then create it ourselves
            if (controllerConfig != null)
            {
                if (controllerConfig.HttpControllerActivator != null)
                {
                    _controllerActivator = GetService<IHttpControllerActivator>(_configuration, controllerConfig.HttpControllerActivator);
                }

                if (controllerConfig.HttpActionSelector != null)
                {
                    _actionSelector = GetService<IHttpActionSelector>(_configuration, controllerConfig.HttpActionSelector);
                }

                if (controllerConfig.HttpActionInvoker != null)
                {
                    _actionInvoker = GetService<IHttpActionInvoker>(_configuration, controllerConfig.HttpActionInvoker);
                }

                if (controllerConfig.ActionValueBinder != null)
                {
                    _actionValueBinder = GetService<IActionValueBinder>(_configuration, controllerConfig.ActionValueBinder);
                }
            }

            // For everything still null we fall back to the default service list.
            if (_controllerActivator == null)
            {
                _controllerActivator = Configuration.Services.GetHttpControllerActivator();
            }

            if (_actionSelector == null)
            {
                _actionSelector = Configuration.Services.GetActionSelector();
            }

            if (_actionInvoker == null)
            {
                _actionInvoker = Configuration.Services.GetActionInvoker();
            }

            if (_actionValueBinder == null)
            {
                _actionValueBinder = Configuration.Services.GetActionValueBinder();
            }
        }

        /// <summary>
        /// Helper for looking up or activating <see cref="IHttpControllerActivator"/>, <see cref="IHttpActionSelector"/>, 
        /// and <see cref="IHttpActionInvoker"/>. Note that we here use the slow <see cref="M:Activator.CreateInstance"/>
        /// as the instances live for the lifetime of the <see cref="HttpControllerDescriptor"/> instance itself so there is
        /// little benefit in caching a delegate.
        /// </summary>
        /// <typeparam name="TBase">The type of the base.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>A new instance.</returns>
        private static TBase GetService<TBase>(HttpConfiguration configuration, Type serviceType) where TBase : class
        {
            Contract.Assert(configuration != null);
            if (serviceType != null)
            {
                return (TBase)configuration.DependencyResolver.GetService(serviceType)
                    ?? (TBase)Activator.CreateInstance(serviceType);
            }

            return null;
        }
    }
}
