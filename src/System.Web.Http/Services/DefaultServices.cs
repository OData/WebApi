// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.Services
{
    /// <summary>
    ///     <para>
    ///         Represents a container for service instances used by the <see cref="HttpConfiguration"/>. Note that
    ///         this container only supports known types, and methods to get or set arbitrary service types will
    ///         throw <see cref="ArgumentException"/> when called. For creation of arbitrary types, please use
    ///         <see cref="IDependencyResolver"/> instead. The supported types for this container are:
    ///     </para>
    ///     <list type="bullet">
    ///         <item><see cref="IActionValueBinder"/></item>
    ///         <item><see cref="IApiExplorer"/></item>
    ///         <item><see cref="IAssembliesResolver"/></item>
    ///         <item><see cref="IBodyModelValidator"/></item>
    ///         <item><see cref="IContentNegotiator"/></item>
    ///         <item><see cref="IDocumentationProvider"/></item>
    ///         <item><see cref="IFilterProvider"/></item>
    ///         <item><see cref="IHttpActionInvoker"/></item>
    ///         <item><see cref="IHttpActionSelector"/></item>
    ///         <item><see cref="IHttpControllerActivator"/></item>
    ///         <item><see cref="IHttpControllerSelector"/></item>
    ///         <item><see cref="IHttpControllerTypeResolver"/></item>
    ///         <item><see cref="ITraceManager"/></item>
    ///         <item><see cref="ITraceWriter"/></item>
    ///         <item><see cref="ModelBinderProvider"/></item>
    ///         <item><see cref="ModelMetadataProvider"/></item>
    ///         <item><see cref="ModelValidatorProvider"/></item>
    ///         <item><see cref="ValueProviderFactory"/></item>
    ///     </list>
    ///     <para>
    ///         Passing any type which is not on this to any method on this interface will cause
    ///         an <see cref="ArgumentException"/> to be thrown.
    ///     </para>
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
    public class DefaultServices : IDisposable
    {
        // This lock protects both caches (and _lastKnownDependencyResolver is updated under it as well)
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private readonly Dictionary<Type, object[]> _cacheMulti = new Dictionary<Type, object[]>();
        private readonly Dictionary<Type, object> _cacheSingle = new Dictionary<Type, object>();
        private readonly HttpConfiguration _configuration;
        private readonly Dictionary<Type, List<object>> _defaultServices = new Dictionary<Type, List<object>>();
        private IDependencyResolver _lastKnownDependencyResolver;
        private readonly HashSet<Type> _serviceTypes;

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        protected DefaultServices()
        {
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class needs references to large number of types.")]
        public DefaultServices(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;

            // Initialize the dictionary with all known service types, even if the list for that service type is
            // empty, because we will throw if the developer tries to read or write unsupported types.
            _defaultServices.Add(typeof(IActionValueBinder), new List<object> { new DefaultActionValueBinder() });
            _defaultServices.Add(typeof(IApiExplorer), new List<object> { new ApiExplorer(configuration) });
            _defaultServices.Add(typeof(IAssembliesResolver), new List<object> { new DefaultAssembliesResolver() });
            _defaultServices.Add(typeof(IBodyModelValidator), new List<object> { new DefaultBodyModelValidator() });
            _defaultServices.Add(typeof(IContentNegotiator), new List<object> { new DefaultContentNegotiator() });
            _defaultServices.Add(typeof(IDocumentationProvider), new List<object>());
            _defaultServices.Add(typeof(IFilterProvider), new List<object>
                                                          {
                                                              new ConfigurationFilterProvider(),
                                                              new ActionDescriptorFilterProvider()
                                                          });
            _defaultServices.Add(typeof(IHttpActionInvoker), new List<object> { new ApiControllerActionInvoker() });
            _defaultServices.Add(typeof(IHttpActionSelector), new List<object> { new ApiControllerActionSelector() });
            _defaultServices.Add(typeof(IHttpControllerActivator), new List<object> { new DefaultHttpControllerActivator() });
            _defaultServices.Add(typeof(IHttpControllerSelector), new List<object> { new DefaultHttpControllerSelector(configuration) });
            _defaultServices.Add(typeof(IHttpControllerTypeResolver), new List<object> { new DefaultHttpControllerTypeResolver() });
            _defaultServices.Add(typeof(ITraceManager), new List<object> { new TraceManager() });
            _defaultServices.Add(typeof(ITraceWriter), new List<object>());

            // This is a priority list. So put the most common binders at the top. 
            _defaultServices.Add(typeof(ModelBinderProvider), new List<object>
                                                              {
                                                                  new TypeConverterModelBinderProvider(),
                                                                  new TypeMatchModelBinderProvider(),
                                                                  new BinaryDataModelBinderProvider(),
                                                                  new KeyValuePairModelBinderProvider(),
                                                                  new ComplexModelDtoModelBinderProvider(),
                                                                  new ArrayModelBinderProvider(),
                                                                  new DictionaryModelBinderProvider(),
                                                                  new CollectionModelBinderProvider(),                                                                  
                                                                  new MutableObjectModelBinderProvider()
                                                              });
            _defaultServices.Add(typeof(ModelMetadataProvider), new List<object> { new DataAnnotationsModelMetadataProvider() });
            _defaultServices.Add(typeof(ModelValidatorProvider), new List<object>
                                                                 {
                                                                     new DataAnnotationsModelValidatorProvider(),
                                                                     new DataMemberModelValidatorProvider()
                                                                 });

            // This is an ordered list,so put the most common providers at the top. 
            _defaultServices.Add(typeof(ValueProviderFactory), new List<object>
                                                               {
                                                                   new QueryStringValueProviderFactory(),
                                                                   new RouteDataValueProviderFactory()                                                                   
                                                               });

            _serviceTypes = new HashSet<Type>(_defaultServices.Keys);
            // Reset the caches and the known dependency scope
            ResetCache();
        }

        /// <summary>
        /// Returns a list of supported service types registered in the service list.
        /// </summary>
        public ICollection<Type> ServiceTypes
        {
            get { return _serviceTypes.ToArray(); }
        }

        /// <summary>
        /// Adds a service to the end of services list for the given service type. 
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service instance.</param>
        public void Add(Type serviceType, object service)
        {
            Insert(serviceType, Int32.MaxValue, service);
        }

        /// <summary>
        /// Adds the services of the specified collection to the end of the services list for
        /// the given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The services to add.</param>
        public void AddRange(Type serviceType, IEnumerable<object> services)
        {
            InsertRange(serviceType, Int32.MaxValue, services);
        }

        /// <summary>
        /// Removes all the service instances of the given service type. 
        /// </summary>
        /// <param name="serviceType">The service type to clear from the services list.</param>
        public void Clear(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            List<object> instances = GetServiceInstances(serviceType);
            instances.Clear();

            ResetCache(serviceType);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
        public virtual void Dispose()
        {
            _cacheLock.Dispose();
        }

        /// <summary>
        /// Searches for a service that matches the conditions defined by the specified
        /// predicate, and returns the zero-based index of the first occurrence.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="match">The delegate that defines the conditions of the element
        /// to search for. </param>
        /// <returns>The zero-based index of the first occurrence, if found; otherwise, -1.</returns>
        public int FindIndex(Type serviceType, Predicate<object> match)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (match == null)
            {
                throw Error.ArgumentNull("match");
            }

            List<object> instances = GetServiceInstances(serviceType);
            return instances.FindIndex(match);
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The first instance of the service, or null if the service is not found.</returns>
        public virtual object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (!_serviceTypes.Contains(serviceType))
            {
                throw Error.Argument("serviceType", SRResources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            // Invalidate the cache if the dependency scope has switched
            if (_lastKnownDependencyResolver != _configuration.DependencyResolver)
            {
                ResetCache();
            }

            object result;

            _cacheLock.EnterReadLock();
            try
            {
                if (_cacheSingle.TryGetValue(serviceType, out result))
                {
                    return result;
                }
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }

            // Get the service from DI, outside of the lock. If we're coming up hot, this might
            // mean we end up creating the service more than once.
            object dependencyService = _configuration.DependencyResolver.GetService(serviceType);

            _cacheLock.EnterWriteLock();
            try
            {
                if (!_cacheSingle.TryGetValue(serviceType, out result))
                {
                    result = dependencyService ?? _defaultServices[serviceType].FirstOrDefault();
                    _cacheSingle[serviceType] = result;
                }

                return result;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The list of service instances of the given type. Returns an empty enumeration if the
        /// service is not found. </returns>
        public virtual IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (!_serviceTypes.Contains(serviceType))
            {
                throw Error.Argument("serviceType", SRResources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            // Invalidate the cache if the dependency scope has switched
            if (_lastKnownDependencyResolver != _configuration.DependencyResolver)
            {
                ResetCache();
            }

            object[] result;

            _cacheLock.EnterReadLock();
            try
            {
                if (_cacheMulti.TryGetValue(serviceType, out result))
                {
                    return result;
                }
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }

            // Get the service from DI, outside of the lock. If we're coming up hot, this might
            // mean we end up creating the service more than once.
            IEnumerable<object> dependencyServices = _configuration.DependencyResolver.GetServices(serviceType);

            _cacheLock.EnterWriteLock();
            try
            {
                if (!_cacheMulti.TryGetValue(serviceType, out result))
                {
                    result = dependencyServices.Where(s => s != null)
                                               .Concat(_defaultServices[serviceType])
                                               .ToArray();
                    _cacheMulti[serviceType] = result;
                }

                return result;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        // Returns the List<object> for the given service type. Also validates serviceType is in the known service type list.
        private List<object> GetServiceInstances(Type serviceType)
        {
            Contract.Assert(serviceType != null);

            List<object> result;
            if (!_defaultServices.TryGetValue(serviceType, out result))
            {
                throw Error.Argument("serviceType", SRResources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            return result;
        }

        /// <summary>
        /// Inserts a service into the collection at the specified index.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="index">The zero-based index at which the service should be inserted.
        /// If <see cref="Int32.MaxValue"/> is passed, ensures the element is added to the end.</param>
        /// <param name="service">The service to insert.</param>
        public void Insert(Type serviceType, int index, object service)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (service == null)
            {
                throw Error.ArgumentNull("service");
            }
            if (!serviceType.IsAssignableFrom(service.GetType()))
            {
                throw Error.Argument("service", SRResources.Common_TypeMustDriveFromType, service.GetType().Name, serviceType.Name);
            }

            List<object> instances = GetServiceInstances(serviceType);
            if (index == Int32.MaxValue)
            {
                index = instances.Count;
            }

            instances.Insert(index, service);

            ResetCache(serviceType);
        }

        /// <summary>
        /// Inserts the elements of the collection into the service list at the specified index.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="index">The zero-based index at which the new elements should be inserted.
        /// If <see cref="Int32.MaxValue"/> is passed, ensures the elements are added to the end.</param>
        /// <param name="services">The collection of services to insert.</param>
        public void InsertRange(Type serviceType, int index, IEnumerable<object> services)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            object[] filteredServices = services.Where(svc => svc != null).ToArray();
            object incorrectlyTypedService = filteredServices.FirstOrDefault(svc => !serviceType.IsAssignableFrom(svc.GetType()));
            if (incorrectlyTypedService != null)
            {
                throw Error.Argument("services", SRResources.Common_TypeMustDriveFromType, incorrectlyTypedService.GetType().Name, serviceType.Name);
            }

            List<object> instances = GetServiceInstances(serviceType);
            if (index == Int32.MaxValue)
            {
                index = instances.Count;
            }

            instances.InsertRange(index, filteredServices);

            ResetCache(serviceType);
        }

        /// <summary>
        /// Removes the first occurrence of the given service from the service list for the given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service instance to remove.</param>
        /// <returns> <c>true</c> if the item is successfull removed; otherwise, <c>false</c>.</returns>
        public bool Remove(Type serviceType, object service)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (service == null)
            {
                throw Error.ArgumentNull("service");
            }

            List<object> instances = GetServiceInstances(serviceType);
            bool result = instances.Remove(service);

            ResetCache(serviceType);

            return result;
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the list.</returns>
        public int RemoveAll(Type serviceType, Predicate<object> match)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (match == null)
            {
                throw Error.ArgumentNull("match");
            }

            List<object> instances = GetServiceInstances(serviceType);
            int result = instances.RemoveAll(match);

            ResetCache(serviceType);

            return result;
        }

        /// <summary>
        /// Removes the service at the specified index.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="index">The zero-based index of the service to remove.</param>
        public void RemoveAt(Type serviceType, int index)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            List<object> instances = GetServiceInstances(serviceType);
            instances.RemoveAt(index);

            ResetCache(serviceType);
        }

        /// <summary>
        /// Replaces all existing services for the given service type with the given
        /// service instance.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service instance.</param>
        public void Replace(Type serviceType, object service)
        {
            // Check this early, so we don't call RemoveAll before Insert would catch the null service.
            if (service == null)
            {
                throw Error.ArgumentNull("service");
            }

            RemoveAll(serviceType, _ => true);
            Insert(serviceType, 0, service);
        }

        /// <summary>
        /// Replaces all existing services for the given service type with the given
        /// service instances.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The service instances.</param>
        public void ReplaceRange(Type serviceType, IEnumerable<object> services)
        {
            // Check this early, so we don't call RemoveAll before InsertRange would catch the null services.
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            RemoveAll(serviceType, _ => true);
            InsertRange(serviceType, 0, services);
        }

        // Removes the cached values for all service types. Called when the dependency scope
        // has changed since the last time we made a request.
        private void ResetCache()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _cacheSingle.Clear();
                _cacheMulti.Clear();
                _lastKnownDependencyResolver = _configuration.DependencyResolver;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        // Removes the cached values for a single service type. Called whenever the user manipulates
        // the local service list for a given service type.
        private void ResetCache(Type serviceType)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _cacheSingle.Remove(serviceType);
                _cacheMulti.Remove(serviceType);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }
    }
}
