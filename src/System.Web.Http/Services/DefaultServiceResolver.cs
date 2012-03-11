using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.Services
{
    /// <summary>
    /// This class is what <see cref="DependencyResolver"/> will ultimately fall back to. 
    /// It handles built-in dependencies that the runtime expects to be present. 
    /// </summary>
    internal class DefaultServiceResolver : IDependencyResolver
    {
        private readonly IDictionary<Type, object[]> _services = new Dictionary<Type, object[]>();

        // Use activator function instead of just object so that we can avoid eagerly calling the constructor. 
        // This is especially important when the constructors will in turn ask for other objects. In that case, eagerly calling 
        // ctors can result in infinite recursion. 
        private readonly IDictionary<Type, Func<HttpConfiguration, object>> _deferredService = new Dictionary<Type, Func<HttpConfiguration, object>>();

        private readonly HttpConfiguration _configuration;

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class needs references to large number of types.")]
        public DefaultServiceResolver(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;

            Add<IBuildManager>(new DefaultBuildManager());

            Add<IHttpControllerFactory>(config => new DefaultHttpControllerFactory(config));

            Add<IHttpControllerActivator>(config => new DefaultHttpControllerActivator(config));

            Add<IHttpActionSelector>(config => new ApiControllerActionSelector());

            Add<IHttpActionInvoker>(config => new ApiControllerActionInvoker());

            Add<ModelMetadataProvider>(new EmptyModelMetadataProvider());

            Add<IContentNegotiator>(new DefaultContentNegotiator());

            Add<IActionValueBinder>(new DefaultActionValueBinder());

            Add<IApiExplorer>(new ApiExplorer(configuration));

            Add<IBodyModelValidator>(new DefaultBodyModelValidator());

            AddRange<IFilterProvider>(
                new ConfigurationFilterProvider(),
                new ActionDescriptorFilterProvider(),
                new EnumerableEvaluatorFilterProvider(),
                new QueryCompositionFilterProvider());

            AddRange<ModelBinderProvider>(
                new TypeMatchModelBinderProvider(),
                new BinaryDataModelBinderProvider(),
                new KeyValuePairModelBinderProvider(),
                new ComplexModelDtoModelBinderProvider(),
                new ArrayModelBinderProvider(),
                new DictionaryModelBinderProvider(),
                new CollectionModelBinderProvider(),
                new TypeConverterModelBinderProvider(),
                new MutableObjectModelBinderProvider());

            AddRange<ModelValidatorProvider>(
                new DataAnnotationsModelValidatorProvider(),
                new ClientDataTypeModelValidatorProvider(),
                new DataMemberModelValidatorProvider());

            AddRange<ValueProviderFactory>(
                //new FormValueProviderFactory(),
                //new JsonValueProviderFactory(),
                new RouteDataValueProviderFactory(),
                new QueryStringValueProviderFactory());

            Add<ModelMetadataProvider>(new CachedDataAnnotationsModelMetadataProvider());

            Add<ITraceManager>(new TraceManager());
        }

        // activator creates the instance of TInterface
        private void Add<TInterface>(Func<HttpConfiguration, object> activator)
        {
            Type type = typeof(TInterface);
            Contract.Assert(type.IsInterface || type.IsAbstract);

            _deferredService[typeof(TInterface)] = activator;
        }

        // singleton for all requests for TInterface
        private void Add<TInterface>(TInterface singleton)
        {
            Add<TInterface>(_ => singleton); // just return existing instance
        }

        private void AddRange<T>(params object[] services)
        {
            // We'd like this to be T[] services, but you can't cast from T[] to object[]. 
            _services[typeof(T)] = services;
        }

        public object GetService(Type t)
        {
            Contract.Assert(t != null);

            Func<HttpConfiguration, object> activator;
            if (_deferredService.TryGetValue(t, out activator))
            {
                return activator(_configuration);
            }

            IEnumerable<object> results = GetServices(t);
            if (results == null)
            {
                return null;
            }

            return results.FirstOrDefault();
        }

        public IEnumerable<object> GetServices(Type t)
        {
            Contract.Assert(t != null);

            object[] services;
            if (!_services.TryGetValue(t, out services))
            {
                return Enumerable.Empty<object>();
            }

            return services;
        }
    }
}
