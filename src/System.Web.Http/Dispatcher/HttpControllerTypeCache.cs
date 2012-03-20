using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Internal;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Manages a cache of <see cref="System.Web.Http.Controllers.IHttpController"/> types detected in the system.
    /// </summary>
    internal sealed class HttpControllerTypeCache
    {
        private const string TypeCacheName = "MS-ControllerTypeCache.xml";
        private const string WildcardNamespace = ".*";

        private readonly HttpConfiguration _configuration;
        private readonly IBuildManager _buildManager;
        private readonly Dictionary<string, ILookup<string, Type>> _cache;

        public HttpControllerTypeCache(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
            _buildManager = _configuration.ServiceResolver.GetBuildManager();
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
            List<Type> controllerTypes = HttpControllerTypeCacheUtil.GetFilteredTypesFromAssemblies(TypeCacheName, IsControllerType, _buildManager);
            var groupedByName = controllerTypes.GroupBy(
                t => t.Name.Substring(0, t.Name.Length - DefaultHttpControllerFactory.ControllerSuffix.Length),
                StringComparer.OrdinalIgnoreCase);

            return groupedByName.ToDictionary(
                g => g.Key,
                g => g.ToLookup(t => t.Namespace ?? String.Empty, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }

        // TODO: Shouldn't "Controller" suffix be matched case-SENsitive so that we don't match "testcontroller" but only "testController"?
        public static bool IsControllerType(Type t)
        {
            return
                t != null &&
                t.IsClass &&
                t.IsPublic &&
                t.Name.EndsWith(DefaultHttpControllerFactory.ControllerSuffix, StringComparison.OrdinalIgnoreCase) &&
                !t.IsAbstract &&
                TypeHelper.HttpControllerType.IsAssignableFrom(t);
        }
    }
}
