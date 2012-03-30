using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Manages a cache of <see cref="System.Web.Http.Controllers.IHttpController"/> types detected in the system.
    /// </summary>
    internal sealed class HttpControllerTypeCache
    {
        private readonly HttpConfiguration _configuration;
        private readonly IAssembliesResolver _assembliesResolver;
        private readonly IHttpControllerTypeResolver _controllersResolver;
        private readonly Dictionary<string, ILookup<string, Type>> _cache;

        public HttpControllerTypeCache(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
            _assembliesResolver = _configuration.ServiceResolver.GetAssembliesResolver();
            _controllersResolver = _configuration.ServiceResolver.GetHttpControllerTypeResolver();
            _cache = InitializeCache();
        }

        internal Dictionary<string, ILookup<string, Type>> Cache
        {
            get { return _cache; }
        }

        public ICollection<Type> GetControllerTypes(string controllerName)
        {
            if (String.IsNullOrEmpty(controllerName))
            {
                throw Error.ArgumentNullOrEmpty("controllerName");
            }

            HashSet<Type> matchingTypes = new HashSet<Type>();

            ILookup<string, Type> namespaceLookup;
            if (_cache.TryGetValue(controllerName, out namespaceLookup))
            {
                foreach (var namespaceGroup in namespaceLookup)
                {
                    matchingTypes.UnionWith(namespaceGroup);
                }
            }

            return matchingTypes;
        }

        private Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            List<Type> controllerTypes = _controllersResolver.GetControllerTypes(_assembliesResolver).ToList();
            var groupedByName = controllerTypes.GroupBy(
                t => t.Name.Substring(0, t.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length),
                StringComparer.OrdinalIgnoreCase);

            return groupedByName.ToDictionary(
                g => g.Key,
                g => g.ToLookup(t => t.Namespace ?? String.Empty, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
