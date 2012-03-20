using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http.Properties;

namespace System.Web.Http.Services
{
    public class DependencyResolver
    {
        // Allows user to specifically override certain resolution types, without needing to create a new DependencyResolver
        private ConcurrentDictionary<Type, object[]> _overrides = new ConcurrentDictionary<Type, object[]>();

        // Provides default objects for certain MVC types. 
        private IDependencyResolver _defaultServiceBuiltInResolver;

        // The user supplied resolver.  We provide a default implementation in case its null.
        private IDependencyResolver _userResolver;

        // Cache should always be a new CacheDependencyResolver(_current).
        private ConcurrentDictionary<Type, object> _cacheSingle = new ConcurrentDictionary<Type, object>();
        private ConcurrentDictionary<Type, IEnumerable<object>> _cacheMultiple = new ConcurrentDictionary<Type, IEnumerable<object>>();

        public DependencyResolver(HttpConfiguration configuration)
            : this(configuration, new DefaultServiceResolver(configuration))
        {
        }

        public DependencyResolver(HttpConfiguration configuration, IDependencyResolver defaultServiceResolver)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            SetResolver(new EmptyDependencyResolver());

            _defaultServiceBuiltInResolver = defaultServiceResolver ?? new EmptyDependencyResolver();
        }

        public void SetService(Type serviceType, object value)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (value == null)
            {
                object[] ignore;
                _overrides.TryRemove(serviceType, out ignore);
                return;
            }

            SetServices(serviceType, new object[] { value });
        }

        public void SetServices(Type serviceType, params object[] values)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            _overrides[serviceType] = values;
        }

        protected object GetCachedService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            return _cacheSingle.GetOrAdd(serviceType, (x) => GetService(x));
        }

        protected IEnumerable<object> GetCachedServices(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            return _cacheMultiple.GetOrAdd(serviceType, (x) => GetServices(x));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            object result;

            // First lookup in overrides
            object[] results;
            if (_overrides.TryGetValue(serviceType, out results))
            {
                if ((results != null) && (results.Length > 0))
                {
                    return results[0];
                }
            }

            // Then ask user
            result = _userResolver.GetService(serviceType);
            if (result != null)
            {
                return result;
            }

            // Then try defaults
            result = _defaultServiceBuiltInResolver.GetService(serviceType);
            return result;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            IEnumerable<object> result;

            // First lookup override
            object[] results;
            if (_overrides.TryGetValue(serviceType, out results))
            {
                if (results != null)
                {
                    return results;
                }
            }

            // Then ask user
            result = _userResolver.GetServices(serviceType);
            if (result.FirstOrDefault() != null)
            {
                return result;
            }

            // Then try defaults
            result = _defaultServiceBuiltInResolver.GetServices(serviceType);
            return result;
        }

        public void SetResolver(IDependencyResolver resolver)
        {
            if (resolver == null)
            {
                throw Error.ArgumentNull("resolver");
            }

            _userResolver = resolver;
            ResetCache();
        }

        public void SetResolver(object commonServiceLocator)
        {
            if (commonServiceLocator == null)
            {
                throw Error.ArgumentNull("commonServiceLocator");
            }

            Type locatorType = commonServiceLocator.GetType();
            MethodInfo getInstance = locatorType.GetMethod("GetInstance", new[] { typeof(Type) });
            MethodInfo getInstances = locatorType.GetMethod("GetAllInstances", new[] { typeof(Type) });

            if (getInstance == null ||
                getInstance.ReturnType != typeof(object) ||
                getInstances == null ||
                getInstances.ReturnType != typeof(IEnumerable<object>))
            {
                throw Error.Argument("commonServiceLocator", SRResources.DependencyResolver_DoesNotImplementICommonServiceLocator, locatorType.FullName);
            }

            var getService = (Func<Type, object>)Delegate.CreateDelegate(typeof(Func<Type, object>), commonServiceLocator, getInstance);
            var getServices = (Func<Type, IEnumerable<object>>)Delegate.CreateDelegate(typeof(Func<Type, IEnumerable<object>>), commonServiceLocator, getInstances);

            SetResolver(new DelegateBasedDependencyResolver(getService, getServices));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types.")]
        public void SetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
        {
            if (getService == null)
            {
                throw Error.ArgumentNull("getService");
            }

            if (getServices == null)
            {
                throw Error.ArgumentNull("getServices");
            }

            SetResolver(new DelegateBasedDependencyResolver(getService, getServices));
        }

        private void ResetCache()
        {
            _cacheSingle = new ConcurrentDictionary<Type, object>();
            _cacheMultiple = new ConcurrentDictionary<Type, IEnumerable<object>>();
        }

        // Helper classes

        private class DelegateBasedDependencyResolver : IDependencyResolver
        {
            private Func<Type, object> _getService;
            private Func<Type, IEnumerable<object>> _getServices;

            public DelegateBasedDependencyResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
            {
                _getService = getService;
                _getServices = getServices;
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method might throw exceptions whose type we cannot strongly link against; namely, ActivationException from common service locator")]
            public object GetService(Type type)
            {
                try
                {
                    return _getService.Invoke(type);
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type type)
            {
                return _getServices(type);
            }
        }

        private class EmptyDependencyResolver : IDependencyResolver
        {
            public object GetService(Type serviceType)
            {
                return null;
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return Enumerable.Empty<object>();
            }
        }
    }
}
