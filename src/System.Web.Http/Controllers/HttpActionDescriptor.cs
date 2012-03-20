using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;

namespace System.Web.Http.Controllers
{
    public abstract class HttpActionDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private readonly object _thisLock = new object();
        private Collection<FilterInfo> _filterPipeline;

        private HttpConfiguration _configuration;
        private HttpControllerDescriptor _controllerDescriptor;
        private readonly Collection<HttpMethod> _supportedHttpMethods = new Collection<HttpMethod>();

        protected HttpActionDescriptor()
        {
        }

        protected HttpActionDescriptor(HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDesriptor");
            }

            _controllerDescriptor = controllerDescriptor;
            _configuration = _controllerDescriptor.Configuration;
        }

        public abstract string ActionName { get; }

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

        public HttpControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerDescriptor = value;
            }
        }

        public abstract Type ReturnType { get; }

        public virtual Collection<HttpMethod> SupportedHttpMethods
        {
            get { return _supportedHttpMethods; }
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            return new Collection<T>();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Filters can be built dynamically")]
        public virtual Collection<IFilter> GetFilters()
        {
            return new Collection<IFilter>();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Parameters can be built dynamically")]
        public abstract Collection<HttpParameterDescriptor> GetParameters();

        /// <summary>
        /// Executes the described action.
        /// </summary>
        /// <param name="controllerContext">The context.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value of the action.</returns>
        public abstract object Execute(HttpControllerContext controllerContext, IDictionary<string, object> arguments);

        /// <summary>
        /// Returns the filters for the given configuration and action. The filter collection is ordered
        /// according to the FilterScope (in order from least specific to most specific: First, Global, Controller, Action).
        /// 
        /// If a given filter disallows duplicates (AllowMultiple=False) then the most specific filter is maintained
        /// and less specific filters get removed (e.g. if there is a Authorize filter with a Controller scope and another
        /// one with an Action scope then the one with the Action scope will be maintained and the one with the Controller
        /// scope will be discarded).
        /// </summary>
        /// <returns>A <see cref="Collection{T}"/> of all filters associated with this <see cref="HttpActionDescriptor"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Filter pipeline can be built dynamically")]
        public virtual Collection<FilterInfo> GetFilterPipeline()
        {
            if (_filterPipeline == null)
            {
                lock (_thisLock)
                {
                    if (_filterPipeline == null)
                    {
                        IEnumerable<IFilterProvider> filterProviders = _configuration.ServiceResolver.GetFilterProviders();

                        IEnumerable<FilterInfo> filters = filterProviders.SelectMany(fp => fp.GetFilters(_configuration, this)).OrderBy(f => f, FilterInfoComparer.Instance);

                        // Need to discard duplicate filters from the end, so that most specific ones get kept (Action scope) and
                        // less specific ones get removed (Global)
                        filters = RemoveDuplicates(filters.Reverse()).Reverse();

                        _filterPipeline = new Collection<FilterInfo>(filters.ToList());
                    }
                }
            }

            return _filterPipeline;
        }

        private static IEnumerable<FilterInfo> RemoveDuplicates(IEnumerable<FilterInfo> filters)
        {
            Contract.Assert(filters != null);

            HashSet<Type> visitedTypes = new HashSet<Type>();

            foreach (FilterInfo filter in filters)
            {
                object filterInstance = filter.Instance;
                Type filterInstanceType = filterInstance.GetType();

                if (!visitedTypes.Contains(filterInstanceType) || AllowMultiple(filterInstance))
                {
                    yield return filter;
                    visitedTypes.Add(filterInstanceType);
                }
            }
        }

        private static bool AllowMultiple(object filterInstance)
        {
            IFilter filter = filterInstance as IFilter;
            return filter == null || filter.AllowMultiple;
        }
    }
}
