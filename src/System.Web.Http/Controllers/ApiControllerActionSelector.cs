// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
using System.Web.Http.Routing;

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
            // Performance-sensitive

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
                object cacheValue;
                if (controllerDescriptor.Properties.TryGetValue(_cacheKey, out cacheValue))
                {
                    return (ActionSelectorCacheItem)cacheValue;
                }
                // Race condition on initialization has no side effects
                ActionSelectorCacheItem selector = new ActionSelectorCacheItem(controllerDescriptor);
                controllerDescriptor.Properties.TryAdd(_cacheKey, selector);
                return selector;
            }
        }

        // All caching is in a dedicated cache class, which may be optionally shared across selector instances.
        // Make this a private nested class so that nobody else can conflict with our state.
        // Cache is initialized during ctor on a single thread.
        private class ActionSelectorCacheItem
        {
            private readonly HttpControllerDescriptor _controllerDescriptor;

            // Includes action descriptors for actions with and without route attributes.
            private readonly CandidateAction[] _combinedCandidateActions;

            // Includes action descriptors only for actions accessible via standard routing (without route attributes).
            private readonly CandidateAction[] _standardCandidateActions;

            private readonly IDictionary<ReflectedHttpActionDescriptor, string[]> _actionParameterNames = new Dictionary<ReflectedHttpActionDescriptor, string[]>();

            // Includes action descriptors for actions with and without route attributes.
            private readonly ILookup<string, ReflectedHttpActionDescriptor> _combinedActionNameMapping;

            // Includes action descriptors only for actions accessible via standard routing (without route attributes).
            private readonly ILookup<string, ReflectedHttpActionDescriptor> _standardActionNameMapping;

            // Selection commonly looks up an action by verb.
            // Cache this mapping. These caches are completely optional and we still behave correctly if we cache miss.
            // We can adjust the specific set we cache based on profiler information.
            // Conceptually, this set of caches could be a HttpMethod --> ReflectedHttpActionDescriptor[].
            // - Beware that HttpMethod has a very slow hash function (it does case-insensitive string hashing). So don't use Dict.
            // - there are unbounded number of http methods, so make sure the cache doesn't grow indefinitely.
            // - we can build the cache at startup and don't need to continually add to it.
            private readonly HttpMethod[] _cacheListVerbKinds = new HttpMethod[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Post };

            private readonly CandidateAction[][] _cacheListVerbs;

            public ActionSelectorCacheItem(HttpControllerDescriptor controllerDescriptor)
            {
                Contract.Assert(controllerDescriptor != null);

                // Initialize the cache entirely in the ctor on a single thread.
                _controllerDescriptor = controllerDescriptor;

                MethodInfo[] allMethods = _controllerDescriptor.ControllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                MethodInfo[] validMethods = Array.FindAll(allMethods, IsValidActionMethod);

                _combinedCandidateActions = new CandidateAction[validMethods.Length];
                for (int i = 0; i < validMethods.Length; i++)
                {
                    MethodInfo method = validMethods[i];
                    ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(_controllerDescriptor, method);
                    _combinedCandidateActions[i] = new CandidateAction
                    {
                        ActionDescriptor = actionDescriptor
                    };
                    HttpActionBinding actionBinding = actionDescriptor.ActionBinding;

                    // Building an action parameter name mapping to compare against the URI parameters coming from the request. Here we only take into account required parameters that are simple types and come from URI.
                    _actionParameterNames.Add(
                        actionDescriptor,
                        actionBinding.ParameterBindings
                            .Where(binding => !binding.Descriptor.IsOptional && TypeHelper.CanConvertFromString(binding.Descriptor.ParameterType) && binding.WillReadUri())
                            .Select(binding => binding.Descriptor.Prefix ?? binding.Descriptor.ParameterName).ToArray());
                }

                if (controllerDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: true).Any())
                {
                    // The controller has an attribute route; no actions are accessible via standard routing.
                    _standardCandidateActions = new CandidateAction[0];
                }
                else
                {
                    // The controller does not have an attribute route; some actions may be accessible via standard
                    // routing.
                    List<CandidateAction> standardCandidateActions = new List<CandidateAction>();

                    for (int i = 0; i < _combinedCandidateActions.Length; i++)
                    {
                        CandidateAction candidate = _combinedCandidateActions[i];
                        if (!candidate.ActionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: true).Any())
                        {
                            standardCandidateActions.Add(candidate);
                        }
                    }

                    _standardCandidateActions = standardCandidateActions.ToArray();
                }

                _combinedActionNameMapping = _combinedCandidateActions.Select(c => c.ActionDescriptor).ToLookup(actionDesc => actionDesc.ActionName, StringComparer.OrdinalIgnoreCase);
                _standardActionNameMapping = _standardCandidateActions.Select(c => c.ActionDescriptor).ToLookup(actionDesc => actionDesc.ActionName, StringComparer.OrdinalIgnoreCase);

                // Bucket the action descriptors by common verbs.
                int len = _cacheListVerbKinds.Length;
                _cacheListVerbs = new CandidateAction[len][];
                for (int i = 0; i < len; i++)
                {
                    _cacheListVerbs[i] = FindActionsForVerbWorker(_cacheListVerbKinds[i]);
                }
            }

            public HttpControllerDescriptor HttpControllerDescriptor
            {
                get { return _controllerDescriptor; }
            }

            public HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
            {
                // Performance-sensitive
                CandidateAction[] candidates = GetInitialCandidateList(controllerContext);

                // Make sure the action parameter matches the route and query parameters. Overload resolution logic is applied when needed.
                List<CandidateAction> actionsFoundByParams = FindActionUsingRouteAndQueryParameters(controllerContext, candidates);
                List<CandidateAction> maximumOrderCandidates = RunOrderFilter(actionsFoundByParams);
                List<CandidateAction> selectedCandidates = RunPrecedenceFilter(maximumOrderCandidates);
                candidates = null;
                actionsFoundByParams = null;

                switch (selectedCandidates.Count)
                {
                    case 0:
                        throw new HttpResponseException(CreateSelectionError(controllerContext));
                    case 1:
                        controllerContext.ElevateRouteData(selectedCandidates[0].ActionDescriptor);
                        return selectedCandidates[0].ActionDescriptor;
                    default:

                        // Throws exception because multiple actions match the request
                        string ambiguityList = CreateAmbiguousMatchList(selectedCandidates);
                        throw Error.InvalidOperation(SRResources.ApiControllerActionSelector_AmbiguousMatch, ambiguityList);
                }
            }

            // Selection error. Caller has already determined the request is an error, and now we need to provide the best error message.
            // If there's another verb that could satisfy this URL, then return 405.
            // Else return 404.
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
            private HttpResponseMessage CreateSelectionError(HttpControllerContext controllerContext)
            {
                // Check for 405.  
                CandidateAction[] candidateActions = GetInitialCandidateList(controllerContext, ignoreVerbs: true);
                List<CandidateAction> actionsFoundByParams = FindActionUsingRouteAndQueryParameters(controllerContext, candidateActions);

                if (actionsFoundByParams.Count > 0)
                {
                    return Create405Response(controllerContext, actionsFoundByParams);
                }

                // Throws HttpResponseException with NotFound status because no action matches the request
                return controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, controllerContext.Request.RequestUri),
                    Error.Format(SRResources.ApiControllerActionSelector_ActionNotFound, _controllerDescriptor.ControllerName));
            }

            // Create a 405 error response with proper headers and message string. 
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
            private static HttpResponseMessage Create405Response(HttpControllerContext controllerContext, IEnumerable<CandidateAction> allowedCandidates)
            {
                HttpMethod incomingMethod = controllerContext.Request.Method;
                HttpResponseMessage response = controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.MethodNotAllowed,
                    Error.Format(SRResources.ApiControllerActionSelector_HttpMethodNotSupported, incomingMethod));

                // 405 must include an Allow content-header with the allowable methods.
                // See: https://tools.ietf.org/html/rfc2616#section-14.7
                HashSet<HttpMethod> methods = new HashSet<HttpMethod>();
                foreach (var candidate in allowedCandidates)
                {
                    methods.UnionWith(candidate.ActionDescriptor.SupportedHttpMethods);
                }
                foreach (var method in methods)
                {
                    response.Content.Headers.Allow.Add(method.ToString());
                }

                return response;
            }

            private CandidateAction[] GetInitialCandidateList(HttpControllerContext controllerContext, bool ignoreVerbs = false)
            {
                // Initial candidate list is determined by:
                // - Direct route?
                // - {action} value?
                // - ignore verbs?
                string actionName;

                HttpMethod incomingMethod = controllerContext.Request.Method;
                IHttpRouteData routeData = controllerContext.RouteData;

                IHttpRoute route = routeData.Route;
                CandidateAction[] candidates;
                if (route != null)
                {
                    // Attribute routing gives the action selector an explicit initial candidate list.
                    candidates = routeData.GetDirectRouteCandidates();
                    if (candidates != null)
                    {
                        actionName = GetActionNameFromDirectRoute(routeData);

                        if (actionName != null)
                        {
                            CandidateAction[] candidatesFoundByName = Array.FindAll(candidates, candidate => candidate.MatchName(actionName));
                            candidates = GetInitialCandidateListByActionName(controllerContext, candidatesFoundByName, actionName, ignoreVerbs);                            
                        } 
                        else if (!ignoreVerbs)
                        {
                            candidates = FindActionsForVerbWorker(incomingMethod, candidates);
                        }

                        return candidates;
                    }
                }

                if (routeData.Values.TryGetValue(RouteKeys.ActionKey, out actionName))
                {
                    // We have an explicit {action} value, do traditional binding. Just lookup by actionName
                    ReflectedHttpActionDescriptor[] actionsFoundByName = _standardActionNameMapping[actionName].ToArray();

                    CandidateAction[] candidatesFoundByName = new CandidateAction[actionsFoundByName.Length];

                    for (int i = 0; i < actionsFoundByName.Length; i++)
                    {
                        candidatesFoundByName[i] = new CandidateAction
                        {
                            ActionDescriptor = actionsFoundByName[i]
                        };
                    }

                    candidates = GetInitialCandidateListByActionName(controllerContext, candidatesFoundByName, actionName, ignoreVerbs);
                }
                else
                {
                    if (ignoreVerbs)
                    {
                        candidates = _standardCandidateActions;
                    }
                    else
                    {
                        // No direct routing or {action} parameter, infer it from the verb.
                        candidates = FindActionsForVerb(incomingMethod);
                    }
                }

                return candidates;
            }

            // Get the action name from a direct route.
            // {action} makes sense in a direct route on a controller, so there should be only 1 
            private static string GetActionNameFromDirectRoute(IHttpRouteData routeData)
            {
                string actionName;
                IEnumerable<IHttpRouteData> subRoutes = routeData.GetSubRoutes();
                if (subRoutes == null)
                {
                    return null;
                }

                if (subRoutes.Count() != 1)
                {
                    return null;
                }

                IHttpRouteData subRoute = subRoutes.First();

                if (subRoute == null)
                {
                    return null;
                }
                if (!subRoute.Values.TryGetValue(RouteKeys.ActionKey, out actionName))
                {
                    return null;
                }
                return actionName;
            }
                        
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
            private CandidateAction[] GetInitialCandidateListByActionName(HttpControllerContext controllerContext, CandidateAction[] candidatesFoundByName, string actionName, bool ignoreVerbs)
            {
                HttpMethod incomingMethod = controllerContext.Request.Method;

                CandidateAction[] candidates;
                // Throws HttpResponseException with NotFound status because no action matches the Name
                if (candidatesFoundByName.Length == 0)
                {
                    throw new HttpResponseException(controllerContext.Request.CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        Error.Format(SRResources.ResourceNotFound, controllerContext.Request.RequestUri),
                        Error.Format(SRResources.ApiControllerActionSelector_ActionNameNotFound, _controllerDescriptor.ControllerName, actionName)));
                }

                if (ignoreVerbs)
                {
                    candidates = candidatesFoundByName;
                }
                else
                {
                    candidates = FilterIncompatibleVerbs(incomingMethod, candidatesFoundByName);
                }
                return candidates;
            }

            private static CandidateAction[] FilterIncompatibleVerbs(HttpMethod incomingMethod, CandidateAction[] candidatesFoundByName)
            {
                return candidatesFoundByName.Where(candidate => candidate.ActionDescriptor.SupportedHttpMethods.Contains(incomingMethod)).ToArray();
            }

            public ILookup<string, HttpActionDescriptor> GetActionMapping()
            {
                return new LookupAdapter() { Source = _combinedActionNameMapping };
            }

            private List<CandidateAction> FindActionUsingRouteAndQueryParameters(HttpControllerContext controllerContext, CandidateAction[] candidatesFound)
            {
                IDictionary<string, object> routeValues = controllerContext.RouteData.Values;
                HashSet<string> routeParameterNames = new HashSet<string>(routeValues.Keys, StringComparer.OrdinalIgnoreCase);
                routeParameterNames.Remove(RouteKeys.ControllerKey);
                routeParameterNames.Remove(RouteKeys.ActionKey);

                HttpRequestMessage request = controllerContext.Request;
                Uri requestUri = request.RequestUri;
                bool hasQueryParameters = requestUri != null && !String.IsNullOrEmpty(requestUri.Query);
                bool hasRouteParameters = routeParameterNames.Count != 0;

                List<CandidateAction> matches = new List<CandidateAction>(candidatesFound.Length);
                if (hasRouteParameters || hasQueryParameters)
                {
                    var combinedParameterNames = new HashSet<string>(routeParameterNames, StringComparer.OrdinalIgnoreCase);
                    if (hasQueryParameters)
                    {
                        foreach (var queryNameValuePair in request.GetQueryNameValuePairs())
                        {
                            combinedParameterNames.Add(queryNameValuePair.Key);
                        }
                    }

                    // action parameters is a subset of route parameters and query parameters
                    for (int i = 0; i < candidatesFound.Length; i++)
                    {
                        CandidateAction candidate = candidatesFound[i];
                        ReflectedHttpActionDescriptor descriptor = candidate.ActionDescriptor;
                        if (IsSubset(_actionParameterNames[descriptor], combinedParameterNames))
                        {
                            matches.Add(candidate);
                        }
                    }
                    if (matches.Count > 1)
                    {
                        // select the results that match the most number of required parameters
                        matches = matches
                            .GroupBy(candidate => _actionParameterNames[candidate.ActionDescriptor].Length)
                            .OrderByDescending(g => g.Key)
                            .First()
                            .ToList();
                    }
                }
                else
                {
                    // return actions with no parameters
                    for (int i = 0; i < candidatesFound.Length; i++)
                    {
                        CandidateAction candidate = candidatesFound[i];
                        ReflectedHttpActionDescriptor descriptor = candidate.ActionDescriptor;
                        if (_actionParameterNames[descriptor].Length == 0)
                        {
                            matches.Add(candidate);
                        }
                    }
                }
                return matches;
            }

            private static bool IsSubset(string[] actionParameters, HashSet<string> routeAndQueryParameters)
            {
                foreach (string actionParameter in actionParameters)
                {
                    if (!routeAndQueryParameters.Contains(actionParameter))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static List<CandidateAction> RunOrderFilter(List<CandidateAction> candidatesFound)
            {
                if (candidatesFound.Count == 0)
                {
                    return candidatesFound;
                }
                int maxOrder = candidatesFound.Max(c => c.Order);
                return candidatesFound.Where(c => c.Order == maxOrder).AsList();
            }

            private static List<CandidateAction> RunPrecedenceFilter(List<CandidateAction> candidatesFound)
            {
                if (candidatesFound.Count == 0)
                {
                    return candidatesFound;
                }
                decimal highestPrecedence = candidatesFound.Min(c => c.Precedence);
                return candidatesFound.Where(c => c.Precedence == highestPrecedence).AsList();
            }

            // This is called when we don't specify an Action name
            // Get list of actions that match a given verb. This can match by name or IActionHttpMethodSelecto
            private CandidateAction[] FindActionsForVerb(HttpMethod verb)
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
            // Given all the standard actions on the controller, filter it to ones that match a given verb.
            private CandidateAction[] FindActionsForVerbWorker(HttpMethod verb)
            {
                return FindActionsForVerbWorker(verb, _standardCandidateActions);
            }

            // Given a list of actions, filter it to ones that match a given verb. This can match by name or IActionHttpMethodSelector.
            // Since this list is fixed for a given verb type, it can be pre-computed and cached.
            // This function should not do caching. It's the helper that builds the caches.
            private static CandidateAction[] FindActionsForVerbWorker(HttpMethod verb, CandidateAction[] candidates)
            {
                List<CandidateAction> listCandidates = new List<CandidateAction>();

                foreach (CandidateAction candidate in candidates)
                {
                    if (candidate.ActionDescriptor != null && candidate.ActionDescriptor.SupportedHttpMethods.Contains(verb))
                    {
                        listCandidates.Add(candidate);
                    }
                }

                return listCandidates.ToArray();
            }

            private static string CreateAmbiguousMatchList(IEnumerable<CandidateAction> ambiguousCandidates)
            {
                StringBuilder exceptionMessageBuilder = new StringBuilder();
                foreach (CandidateAction candidate in ambiguousCandidates)
                {
                    ReflectedHttpActionDescriptor descriptor = candidate.ActionDescriptor;
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

                if (methodInfo.GetCustomAttribute<NonActionAttribute>() != null)
                {
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
