// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Reflection based action selector. 
    /// We optimize for the case where we have an <see cref="ApiControllerActionSelector"/> instance per <see cref="HttpControllerDescriptor"/>
    /// instance but can support cases where there are many <see cref="HttpControllerDescriptor"/> instances for one 
    /// <see cref="ApiControllerActionSelector"/> as well. In the latter case the lookup is slightly slower because it goes through
    /// the <see cref="P:HttpControllerDescriptor.Properties"/> dictionary.
    /// </summary>
    public class ApiControllerActionSelector : IHttpActionSelector
    {
        private const string ActionRouteKey = "action";
        private const string ControllerRouteKey = "controller";

        private ActionSelectorCacheItem _fastCache;
        private readonly object _cacheKey = new object();

        public virtual HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            ActionSelectorCacheItem internalSelector = GetInternalSelector(controllerContext.ControllerDescriptor);
            return internalSelector.SelectAction(controllerContext);
        }

        public virtual ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            ActionSelectorCacheItem internalSelector = GetInternalSelector(controllerDescriptor);
            return internalSelector.GetActionMapping();
        }

        private ActionSelectorCacheItem GetInternalSelector(HttpControllerDescriptor controllerDescriptor)
        {
            // First check in the local fast cache and if not a match then look in the broader 
            // HttpControllerDescriptor.Properties cache
            if (_fastCache == null)
            {
                ActionSelectorCacheItem selector = new ActionSelectorCacheItem(controllerDescriptor);
                Interlocked.CompareExchange(ref _fastCache, selector, null);
                return selector;
            }
            else if (_fastCache.HttpControllerDescriptor == controllerDescriptor)
            {
                // If the key matches and we already have the delegate for creating an instance then just execute it
                return _fastCache;
            }
            else
            {
                // If the key doesn't match then lookup/create delegate in the HttpControllerDescriptor.Properties for
                // that HttpControllerDescriptor instance
                ActionSelectorCacheItem selector = (ActionSelectorCacheItem)controllerDescriptor.Properties.GetOrAdd(
                    _cacheKey,
                    _ => new ActionSelectorCacheItem(controllerDescriptor));
                return selector;
            }
        }

        // All caching is in a dedicated cache class, which may be optionally shared across selector instances.
        // Make this a private nested class so that nobody else can conflict with our state.
        // Cache is initialized during ctor on a single thread. 
        private class ActionSelectorCacheItem
        {
            private readonly HttpControllerDescriptor _controllerDescriptor;

            private readonly ReflectedHttpActionDescriptor[] _actionDescriptors;

            private readonly IDictionary<ReflectedHttpActionDescriptor, IEnumerable<string>> _actionParameterNames = new Dictionary<ReflectedHttpActionDescriptor, IEnumerable<string>>();

            private readonly ILookup<string, ReflectedHttpActionDescriptor> _actionNameMapping;

            // Selection commonly looks up an action by verb. 
            // Cache this mapping. These caches are completely optional and we still behave correctly if we cache miss. 
            // We can adjust the specific set we cache based on profiler information.
            // Conceptually, this set of caches could be a HttpMethod --> ReflectedHttpActionDescriptor[]. 
            // - Beware that HttpMethod has a very slow hash function (it does case-insensitive string hashing). So don't use Dict.
            // - there are unbounded number of http methods, so make sure the cache doesn't grow indefinitely.  
            // - we can build the cache at startup and don't need to continually add to it. 
            private readonly HttpMethod[] _cacheListVerbKinds = new HttpMethod[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Post };
            private readonly ReflectedHttpActionDescriptor[][] _cacheListVerbs;

            public ActionSelectorCacheItem(HttpControllerDescriptor controllerDescriptor)
            {
                Contract.Assert(controllerDescriptor != null);

                // Initialize the cache entirely in the ctor on a single thread.
                _controllerDescriptor = controllerDescriptor;

                MethodInfo[] allMethods = _controllerDescriptor.ControllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                MethodInfo[] validMethods = Array.FindAll(allMethods, IsValidActionMethod);

                _actionDescriptors = new ReflectedHttpActionDescriptor[validMethods.Length];
                for (int i = 0; i < validMethods.Length; i++)
                {
                    MethodInfo method = validMethods[i];
                    ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(_controllerDescriptor, method);
                    _actionDescriptors[i] = actionDescriptor;
                    HttpActionBinding actionBinding = controllerDescriptor.ActionValueBinder.GetBinding(actionDescriptor);

                    // Build action parameter name mapping, only consider parameters that are simple types, do not have default values and come from URI
                    _actionParameterNames.Add(
                        actionDescriptor,
                        actionBinding.ParameterBindings
                            .Where(binding => TypeHelper.IsSimpleUnderlyingType(binding.Descriptor.ParameterType) && !binding.HasDefaultValue() && binding.WillReadUri())
                            .Select(binding => binding.Descriptor.Prefix ?? binding.Descriptor.ParameterName));
                }

                _actionNameMapping = _actionDescriptors.ToLookup(actionDesc => actionDesc.ActionName, StringComparer.OrdinalIgnoreCase);

                // Bucket the action descriptors by common verbs. 
                int len = _cacheListVerbKinds.Length;
                _cacheListVerbs = new ReflectedHttpActionDescriptor[len][];
                for (int i = 0; i < len; i++)
                {
                    _cacheListVerbs[i] = FindActionsForVerbWorker(_cacheListVerbKinds[i]);
                }
            }

            public HttpControllerDescriptor HttpControllerDescriptor
            {
                get { return _controllerDescriptor; }
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
            public HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
            {
                string actionName;
                bool useActionName = controllerContext.RouteData.Values.TryGetValue(ActionRouteKey, out actionName);

                ReflectedHttpActionDescriptor[] actionsFoundByHttpMethods;

                HttpMethod incomingMethod = controllerContext.Request.Method;

                // First get an initial candidate list. 
                if (useActionName)
                {
                    // We have an explicit {action} value, do traditional binding. Just lookup by actionName
                    ReflectedHttpActionDescriptor[] actionsFoundByName = _actionNameMapping[actionName].ToArray();

                    // Throws HttpResponseException with NotFound status because no action matches the Name
                    if (actionsFoundByName.Length == 0)
                    {
                        throw new HttpResponseException(controllerContext.Request.CreateResponse(
                            HttpStatusCode.NotFound,
                            Error.Format(SRResources.ApiControllerActionSelector_ActionNameNotFound, _controllerDescriptor.ControllerName, actionName)));
                    }

                    // This filters out any incompatible verbs from the incoming action list
                    actionsFoundByHttpMethods = actionsFoundByName.Where(actionDescriptor => actionDescriptor.SupportedHttpMethods.Contains(incomingMethod)).ToArray();
                }
                else
                {
                    // No {action} parameter, infer it from the verb.
                    actionsFoundByHttpMethods = FindActionsForVerb(incomingMethod);
                }

                // Throws HttpResponseException with MethodNotAllowed status because no action matches the Http Method
                if (actionsFoundByHttpMethods.Length == 0)
                {
                    throw new HttpResponseException(controllerContext.Request.CreateResponse(
                        HttpStatusCode.MethodNotAllowed,
                        Error.Format(SRResources.ApiControllerActionSelector_HttpMethodNotSupported, incomingMethod)));
                }

                // If there are multiple candidates, then apply overload resolution logic.
                if (actionsFoundByHttpMethods.Length > 1)
                {
                    actionsFoundByHttpMethods = FindActionUsingRouteAndQueryParameters(controllerContext, actionsFoundByHttpMethods).ToArray();
                }

                List<ReflectedHttpActionDescriptor> selectedActions = RunSelectionFilters(controllerContext, actionsFoundByHttpMethods);
                actionsFoundByHttpMethods = null;

                switch (selectedActions.Count)
                {
                    case 0:
                        // Throws HttpResponseException with NotFound status because no action matches the request
                        throw new HttpResponseException(controllerContext.Request.CreateResponse(
                            HttpStatusCode.NotFound,
                            Error.Format(SRResources.ApiControllerActionSelector_ActionNotFound, _controllerDescriptor.ControllerName)));
                    case 1:
                        return selectedActions[0];
                    default:
                        // Throws HttpResponseException with InternalServerError status because multiple action matches the request
                        string ambiguityList = CreateAmbiguousMatchList(selectedActions);
                        throw new HttpResponseException(controllerContext.Request.CreateResponse(
                            HttpStatusCode.InternalServerError,
                            Error.Format(SRResources.ApiControllerActionSelector_AmbiguousMatch, ambiguityList)));
                }
            }

            public ILookup<string, HttpActionDescriptor> GetActionMapping()
            {
                return new LookupAdapter() { Source = _actionNameMapping };
            }

            private IEnumerable<ReflectedHttpActionDescriptor> FindActionUsingRouteAndQueryParameters(HttpControllerContext controllerContext, IEnumerable<ReflectedHttpActionDescriptor> actionsFound)
            {
                // TODO, DevDiv 320655, improve performance of this method.
                IDictionary<string, object> routeValues = controllerContext.RouteData.Values;
                IEnumerable<string> routeParameterNames = routeValues.Select(route => route.Key)
                    .Where(key =>
                           !String.Equals(key, ControllerRouteKey, StringComparison.OrdinalIgnoreCase) &&
                           !String.Equals(key, ActionRouteKey, StringComparison.OrdinalIgnoreCase));

                IEnumerable<string> queryParameterNames = controllerContext.Request.RequestUri.ParseQueryString().AllKeys;
                bool hasRouteParameters = routeParameterNames.Any();
                bool hasQueryParameters = queryParameterNames.Any();

                if (hasRouteParameters || hasQueryParameters)
                {
                    // refine the results based on route parameters to make sure that route parameters take precedence over query parameters
                    if (hasRouteParameters && hasQueryParameters)
                    {
                        // route parameters is a subset of action parameters
                        actionsFound = actionsFound.Where(descriptor => !routeParameterNames.Except(_actionParameterNames[descriptor], StringComparer.OrdinalIgnoreCase).Any());
                    }

                    // further refine the results making sure that action parameters is a subset of route parameters and query parameters
                    if (actionsFound.Count() > 1)
                    {
                        IEnumerable<string> combinedParameterNames = queryParameterNames.Union(routeParameterNames);

                        // action parameters is a subset of route parameters and query parameters
                        actionsFound = actionsFound.Where(descriptor => !_actionParameterNames[descriptor].Except(combinedParameterNames, StringComparer.OrdinalIgnoreCase).Any());

                        // select the results with the longest parameter match 
                        if (actionsFound.Count() > 1)
                        {
                            actionsFound = actionsFound
                                .GroupBy(descriptor => _actionParameterNames[descriptor].Count())
                                .OrderByDescending(g => g.Key)
                                .First();
                        }
                    }
                }
                else
                {
                    // return actions with no parameters
                    actionsFound = actionsFound.Where(descriptor => !_actionParameterNames[descriptor].Any());
                }

                return actionsFound;
            }

            private static List<ReflectedHttpActionDescriptor> RunSelectionFilters(HttpControllerContext controllerContext, IEnumerable<HttpActionDescriptor> descriptorsFound)
            {
                // remove all methods which are opting out of this request
                // to opt out, at least one attribute defined on the method must return false

                List<ReflectedHttpActionDescriptor> matchesWithSelectionAttributes = null;
                List<ReflectedHttpActionDescriptor> matchesWithoutSelectionAttributes = new List<ReflectedHttpActionDescriptor>();

                foreach (ReflectedHttpActionDescriptor actionDescriptor in descriptorsFound)
                {
                    IActionMethodSelector[] attrs = actionDescriptor.CacheAttrsIActionMethodSelector;
                    if (attrs.Length == 0)
                    {
                        matchesWithoutSelectionAttributes.Add(actionDescriptor);
                    }
                    else
                    {
                        bool match = Array.TrueForAll(attrs, selector => selector.IsValidForRequest(controllerContext, actionDescriptor.MethodInfo));
                        if (match)
                        {
                            if (matchesWithSelectionAttributes == null)
                            {
                                matchesWithSelectionAttributes = new List<ReflectedHttpActionDescriptor>();
                            }
                            matchesWithSelectionAttributes.Add(actionDescriptor);
                        }
                    }
                }

                // if a matching action method had a selection attribute, consider it more specific than a matching action method
                // without a selection attribute
                if ((matchesWithSelectionAttributes != null) && (matchesWithSelectionAttributes.Count > 0))
                {
                    return matchesWithSelectionAttributes;
                }
                else
                {
                    return matchesWithoutSelectionAttributes;
                }                
            }

            // This is called when we don't specify an Action name
            // Get list of actions that match a given verb. This can match by name or IActionHttpMethodSelecto
            private ReflectedHttpActionDescriptor[] FindActionsForVerb(HttpMethod verb)
            {
                // Check cache for common verbs. 
                for (int i = 0; i < _cacheListVerbKinds.Length; i++)
                {
                    // verb selection on common verbs is normalized to have object reference identity. 
                    // This is significantly more efficient than comparing the verbs based on strings. 
                    if (Object.ReferenceEquals(verb, _cacheListVerbKinds[i]))
                    {
                        return _cacheListVerbs[i];
                    }
                }

                // General case for any verbs. 
                return FindActionsForVerbWorker(verb);
            }

            // This is called when we don't specify an Action name
            // Get list of actions that match a given verb. This can match by name or IActionHttpMethodSelector.
            // Since this list is fixed for a given verb type, it can be pre-computed and cached.   
            // This function should not do caching. It's the helper that builds the caches. 
            private ReflectedHttpActionDescriptor[] FindActionsForVerbWorker(HttpMethod verb)
            {
                List<ReflectedHttpActionDescriptor> listMethods = new List<ReflectedHttpActionDescriptor>();

                foreach (ReflectedHttpActionDescriptor descriptor in _actionDescriptors)
                {
                    if (descriptor.SupportedHttpMethods.Contains(verb))
                    {
                        listMethods.Add(descriptor);
                    }
                }

                return listMethods.ToArray();
            }

            private static string CreateAmbiguousMatchList(IEnumerable<HttpActionDescriptor> ambiguousDescriptors)
            {
                StringBuilder exceptionMessageBuilder = new StringBuilder();
                foreach (ReflectedHttpActionDescriptor descriptor in ambiguousDescriptors)
                {
                    MethodInfo methodInfo = descriptor.MethodInfo;

                    exceptionMessageBuilder.AppendLine();
                    exceptionMessageBuilder.Append(Error.Format(
                        SRResources.ActionSelector_AmbiguousMatchType,
                        methodInfo, methodInfo.DeclaringType.FullName));
                }

                return exceptionMessageBuilder.ToString();
            }

            private static bool IsValidActionMethod(MethodInfo methodInfo)
            {
                if (methodInfo.IsSpecialName)
                {
                    // not a normal method, e.g. a constructor or an event
                    return false;
                }

                if (methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(TypeHelper.ApiControllerType))
                {
                    // is a method on Object, IHttpController, ApiController
                    return false;
                }

                return true;
            }
        }

        // We need to expose ILookup<string, HttpActionDescriptor>, but we have a ILookup<string, ReflectedHttpActionDescriptor>
        // ReflectedHttpActionDescriptor derives from HttpActionDescriptor, but ILookup doesn't support Covariance.
        // Adapter class since ILookup doesn't support Covariance. 
        // Fortunately, IGrouping, IEnumerable support Covariance, so it's easy to forward.
        private class LookupAdapter : ILookup<string, HttpActionDescriptor>
        {
            public ILookup<string, ReflectedHttpActionDescriptor> Source;

            public int Count
            {
                get { return Source.Count; }
            }

            public IEnumerable<HttpActionDescriptor> this[string key]
            {
                get { return Source[key]; }
            }

            public bool Contains(string key)
            {
                return Source.Contains(key);
            }
                        
            public IEnumerator<IGrouping<string, HttpActionDescriptor>> GetEnumerator()
            {
                return Source.GetEnumerator();
            }

            Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
            {
                return Source.GetEnumerator();
            }
        }
    }
}
