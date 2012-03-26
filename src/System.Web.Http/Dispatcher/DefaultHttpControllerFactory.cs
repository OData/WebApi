using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using System.Web.Http.Routing;
using System.Web.Http.Services;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Default <see cref="IHttpControllerFactory"/> instance creating new <see cref="IHttpController"/> instances.
    /// A different implementation can be registered via the <see cref="DependencyResolver"/>.   
    /// </summary>
    public class DefaultHttpControllerFactory : IHttpControllerFactory
    {
        public static readonly string ControllerSuffix = "Controller";

        private readonly HttpConfiguration _configuration;
        private readonly HttpControllerTypeCache _controllerTypeCache;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _controllerInfoCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DefaultHttpControllerFactory(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("resolver");
            }

            _controllerInfoCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(InitializeControllerInfoCache);
            _configuration = configuration;
            _controllerTypeCache = new HttpControllerTypeCache(_configuration);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public virtual IHttpController CreateController(HttpControllerContext controllerContext, string controllerName)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (String.IsNullOrEmpty(controllerName))
            {
                throw Error.ArgumentNullOrEmpty("controllerName");
            }

            HttpControllerDescriptor controllerDescriptor;
            if (_controllerInfoCache.Value.TryGetValue(controllerName, out controllerDescriptor))
            {
                // Create controller instance
                return CreateInstance(controllerContext, controllerDescriptor);
            }

            ICollection<Type> matchingTypes = _controllerTypeCache.GetControllerTypes(controllerName);
            switch (matchingTypes.Count)
            {
                case 0:
                    // no matching types
                    throw new HttpResponseException(controllerContext.Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        Error.Format(SRResources.DefaultControllerFactory_ControllerNameNotFound, controllerName)));

                case 1:
                    // single matching type
                    Type match = matchingTypes.First();

                    // Add controller descriptor to cache
                    controllerDescriptor = new HttpControllerDescriptor(_configuration, controllerName, match);
                    _controllerInfoCache.Value.TryAdd(controllerName, controllerDescriptor);

                    // Create controller instance
                    return CreateInstance(controllerContext, controllerDescriptor);

                default:
                    // multiple matching types
                    throw new HttpResponseException(controllerContext.Request.CreateResponse(
                        HttpStatusCode.InternalServerError,
                        CreateAmbiguousControllerExceptionMessage(controllerContext.RouteData.Route, controllerName, matchingTypes)));
            }
        }

        public virtual void ReleaseController(HttpControllerContext controllerContext, IHttpController controller)
        {
            IDisposable disposable = controller as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerInfoCache.Value.ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static IHttpController CreateInstance(HttpControllerContext controllerContext, HttpControllerDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(controllerDescriptor != null);

            // Fill in controller descriptor on execution context
            controllerContext.ControllerDescriptor = controllerDescriptor;

            // Invoke the controller activator
            IHttpController instance = controllerDescriptor.HttpControllerActivator.Create(controllerContext, controllerDescriptor.ControllerType);

            // Fill in controller instance on execution context
            controllerContext.Controller = instance;

            return instance;
        }

        private static string CreateAmbiguousControllerExceptionMessage(IHttpRoute route, string controllerName, ICollection<Type> matchingTypes)
        {
            Contract.Assert(route != null);
            Contract.Assert(controllerName != null);
            Contract.Assert(matchingTypes != null);

            // Generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            return Error.Format(SRResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate, controllerName, route.RouteTemplate, typeList);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> InitializeControllerInfoCache()
        {
            var result = new ConcurrentDictionary<string, HttpControllerDescriptor>();
            var duplicateControllers = new HashSet<string>();
            Dictionary<string, ILookup<string, Type>> controllerTypeGroups = _controllerTypeCache.Cache;

            foreach (KeyValuePair<string, ILookup<string, Type>> controllerTypeGroup in controllerTypeGroups)
            {
                string controllerName = controllerTypeGroup.Key;
                foreach (IGrouping<string, Type> controllerTypesGroupedByNs in controllerTypeGroup.Value)
                {
                    foreach (Type controllerType in controllerTypesGroupedByNs)
                    {
                        if (result.Keys.Contains(controllerName))
                        {
                            duplicateControllers.Add(controllerName);
                            break;
                        }
                        else
                        {
                            result.TryAdd(controllerName, new HttpControllerDescriptor(_configuration, controllerName, controllerType));
                        }
                    }
                }
            }

            foreach (string duplicateController in duplicateControllers)
            {
                HttpControllerDescriptor descriptor;
                result.TryRemove(duplicateController, out descriptor);
            }

            return result;
        }
    }
}
