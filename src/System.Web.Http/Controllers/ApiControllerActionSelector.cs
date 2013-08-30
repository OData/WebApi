// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
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

                if (controllerDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false).Any())
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
                        // Allow standard routes access inherited actions or actions without Route attributes.
                        if (candidate.ActionDescriptor.MethodInfo.DeclaringType != controllerDescriptor.ControllerType ||
                            !candidate.ActionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false).Any())
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
                List<CandidateActionWithParams> selectedCandidates = FindMatchingActions(controllerContext); 

                switch (selectedCandidates.Count)
                {
                    case 0:
                        throw new HttpResponseException(CreateSelectionError(controllerContext));
                    case 1:
                        ElevateRouteData(controllerContext, selectedCandidates[0]);
                        return selectedCandidates[0].ActionDescriptor;
                    default:

                        // Throws exception because multiple actions match the request
                        string ambiguityList = CreateAmbiguousMatchList(selectedCandidates);
                        throw Error.InvalidOperation(SRResources.ApiControllerActionSelector_AmbiguousMatch, ambiguityList);
                }
            }

            private static void ElevateRouteData(HttpControllerContext controllerContext, CandidateActionWithParams selectedCandidate)
            {
                controllerContext.RouteData = selectedCandidate.RouteDataSource;
            }
                        
            // Find all actions on this controller that match the request. 
            // if ignoreVerbs = true, then don't filter actions based on mismatching Http verb. This is useful for detecting 404/405. 
            private List<CandidateActionWithParams> FindMatchingActions(HttpControllerContext controllerContext, bool ignoreVerbs = false)
            {
                // If matched with direct route?
                IHttpRouteData routeData = controllerContext.RouteData;
                IEnumerable<IHttpRouteData> subRoutes = routeData.GetSubRoutes();
                                
                IEnumerable<CandidateActionWithParams> actionsWithParameters = (subRoutes == null) ? 
                    GetInitialCandidateWithParameterListForRegularRoutes(controllerContext, ignoreVerbs) :
                    GetInitialCandidateWithParameterListForDirectRoutes(controllerContext, subRoutes, ignoreVerbs);

                // Make sure the action parameter matches the route and query parameters.
                List<CandidateActionWithParams> actionsFoundByParams = FindActionMatchRequiredRouteAndQueryParameters(actionsWithParameters);

                List<CandidateActionWithParams> orderCandidates = RunOrderFilter(actionsFoundByParams);
                List<CandidateActionWithParams> precedenceCandidates = RunPrecedenceFilter(orderCandidates);

                // Overload resolution logic is applied when needed.
                List<CandidateActionWithParams> selectedCandidates = FindActionMatchMostRouteAndQueryParameters(precedenceCandidates);

                return selectedCandidates;
            }

            // Selection error. Caller has already determined the request is an error, and now we need to provide the best error message.
            // If there's another verb that could satisfy this URL, then return 405.
            // Else return 404.
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
            private HttpResponseMessage CreateSelectionError(HttpControllerContext controllerContext)
            {
                // Check for 405.  
                List<CandidateActionWithParams> actionsFoundByParams = FindMatchingActions(controllerContext, ignoreVerbs: true);

                if (actionsFoundByParams.Count > 0)
                {
                    return Create405Response(controllerContext, actionsFoundByParams);
                }

                // Throws HttpResponseException with NotFound status because no action matches the request
                return CreateActionNotFoundResponse(controllerContext);
            }

            // Create a 405 error response with proper headers and message string. 
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
            private static HttpResponseMessage Create405Response(HttpControllerContext controllerContext, IEnumerable<CandidateActionWithParams> allowedCandidates)
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

            // Create a 404
            private HttpResponseMessage CreateActionNotFoundResponse(HttpControllerContext controllerContext)
            {
                return controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, controllerContext.Request.RequestUri),
                    Error.Format(SRResources.ApiControllerActionSelector_ActionNotFound, _controllerDescriptor.ControllerName));
            }

            // Create a 404, including the name of the action we were looking for. 
            // This overload includes the action name. 
            private HttpResponseMessage CreateActionNotFoundResponse(HttpControllerContext controllerContext, string actionName)
            {
                return controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, controllerContext.Request.RequestUri),
                    Error.Format(SRResources.ApiControllerActionSelector_ActionNameNotFound, _controllerDescriptor.ControllerName, actionName));
            }

            // Call for direct routes. 
            private static List<CandidateActionWithParams> GetInitialCandidateWithParameterListForDirectRoutes(HttpControllerContext controllerContext, IEnumerable<IHttpRouteData> subRoutes, bool ignoreVerbs)
            {
                HttpRequestMessage request = controllerContext.Request;
                HttpMethod incomingMethod = controllerContext.Request.Method;

                var queryNameValuePairs = request.GetQueryNameValuePairs();

                List<CandidateActionWithParams> candidateActionWithParams = new List<CandidateActionWithParams>();

                foreach (IHttpRouteData subRouteData in subRoutes)
                {
                    // Each route may have different route parameters.
                    ISet<string> combinedParameterNames = GetCombinedParameterNames(queryNameValuePairs, subRouteData.Values);

                    CandidateAction[] candidates = subRouteData.Route.GetDirectRouteCandidates();

                    string actionName;
                    subRouteData.Values.TryGetValue(RouteKeys.ActionKey, out actionName);

                    foreach (var candidate in candidates)
                    {
                        if ((actionName == null) || candidate.MatchName(actionName))
                        {
                            if (ignoreVerbs || candidate.MatchVerb(incomingMethod))
                            {
                                candidateActionWithParams.Add(new CandidateActionWithParams(candidate, combinedParameterNames, subRouteData));
                            }
                        }
                    }
                }
                return candidateActionWithParams;
            }

            // Call for non-direct routes
            private IEnumerable<CandidateActionWithParams> GetInitialCandidateWithParameterListForRegularRoutes(HttpControllerContext controllerContext, bool ignoreVerbs = false)
            {
                CandidateAction[] candidates = GetInitialCandidateList(controllerContext, ignoreVerbs);
                return GetCandidateActionsWithBindings(controllerContext, candidates);
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

                Contract.Assert(routeData.GetSubRoutes() == null, "Should not be called on a direct route");
                CandidateAction[] candidates;

                if (routeData.Values.TryGetValue(RouteKeys.ActionKey, out actionName))
                {
                    // We have an explicit {action} value, do traditional binding. Just lookup by actionName
                    ReflectedHttpActionDescriptor[] actionsFoundByName = _standardActionNameMapping[actionName].ToArray();

                    // Throws HttpResponseException with NotFound status because no action matches the Name
                    if (actionsFoundByName.Length == 0)
                    {
                        throw new HttpResponseException(CreateActionNotFoundResponse(controllerContext, actionName));
                    }

                    CandidateAction[] candidatesFoundByName = new CandidateAction[actionsFoundByName.Length];

                    for (int i = 0; i < actionsFoundByName.Length; i++)
                    {
                        candidatesFoundByName[i] = new CandidateAction
                        {
                            ActionDescriptor = actionsFoundByName[i]
                        };
                    }

                    if (ignoreVerbs)
                    {
                        candidates = candidatesFoundByName;
                    }
                    else
                    {
                        candidates = FilterIncompatibleVerbs(incomingMethod, candidatesFoundByName);
                    }
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

            private static CandidateAction[] FilterIncompatibleVerbs(HttpMethod incomingMethod, CandidateAction[] candidatesFoundByName)
            {
                return candidatesFoundByName.Where(candidate => candidate.ActionDescriptor.SupportedHttpMethods.Contains(incomingMethod)).ToArray();
            }

            public ILookup<string, HttpActionDescriptor> GetActionMapping()
            {
                return new LookupAdapter() { Source = _combinedActionNameMapping };
            }

            // Get a non-null set that combines both the route and query parameters. 
            private static ISet<string> GetCombinedParameterNames(IEnumerable<KeyValuePair<string, string>> queryNameValuePairs, IDictionary<string, object> routeValues)
            {
                HashSet<string> routeParameterNames = new HashSet<string>(routeValues.Keys, StringComparer.OrdinalIgnoreCase);
                routeParameterNames.Remove(RouteKeys.ControllerKey);
                routeParameterNames.Remove(RouteKeys.ActionKey);

                var combinedParameterNames = new HashSet<string>(routeParameterNames, StringComparer.OrdinalIgnoreCase);
                if (queryNameValuePairs != null)
                {
                    foreach (var queryNameValuePair in queryNameValuePairs)
                    {
                        combinedParameterNames.Add(queryNameValuePair.Key);
                    }
                }
                return combinedParameterNames;
            }

            private List<CandidateActionWithParams> FindActionMatchRequiredRouteAndQueryParameters(IEnumerable<CandidateActionWithParams> candidatesFound)
            {
                List<CandidateActionWithParams> matches = new List<CandidateActionWithParams>();

                foreach (var candidate in candidatesFound)
                {
                    ReflectedHttpActionDescriptor descriptor = candidate.ActionDescriptor;
                    if (IsSubset(_actionParameterNames[descriptor], candidate.CombinedParameterNames))
                    {
                        matches.Add(candidate);
                    }
                }

                return matches;
            }

            private List<CandidateActionWithParams> FindActionMatchMostRouteAndQueryParameters(List<CandidateActionWithParams> candidatesFound)
            {
                if (candidatesFound.Count > 1)
                {
                    // select the results that match the most number of required parameters
                    return candidatesFound
                        .GroupBy(candidate => _actionParameterNames[candidate.ActionDescriptor].Length)
                        .OrderByDescending(g => g.Key)
                        .First()
                        .ToList();
                }

                return candidatesFound;
            }

            // Given a list of candidate actions, return a parallel list that includes the parameter information. 
            // This is used for regular routing where all candidates come from a single route, so they all share the same route parameter names. 
            private static CandidateActionWithParams[] GetCandidateActionsWithBindings(HttpControllerContext controllerContext, CandidateAction[] candidatesFound)
            {
                HttpRequestMessage request = controllerContext.Request;
                var queryNameValuePairs = request.GetQueryNameValuePairs();
                IHttpRouteData routeData = controllerContext.RouteData;
                IDictionary<string, object> routeValues = routeData.Values;
                ISet<string> combinedParameterNames = GetCombinedParameterNames(queryNameValuePairs, routeValues);

                CandidateActionWithParams[] candidatesWithParams = Array.ConvertAll(candidatesFound, candidate => new CandidateActionWithParams(candidate, combinedParameterNames, routeData));
                return candidatesWithParams;
            }

            private static bool IsSubset(string[] actionParameters, ISet<string> routeAndQueryParameters)
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

            private static List<CandidateActionWithParams> RunOrderFilter(List<CandidateActionWithParams> candidatesFound)
            {
                if (candidatesFound.Count == 0)
                {
                    return candidatesFound;
                }
                int minOrder = candidatesFound.Min(c => c.CandidateAction.Order);
                return candidatesFound.Where(c => c.CandidateAction.Order == minOrder).AsList();
            }

            private static List<CandidateActionWithParams> RunPrecedenceFilter(List<CandidateActionWithParams> candidatesFound)
            {
                if (candidatesFound.Count == 0)
                {
                    return candidatesFound;
                }
                decimal highestPrecedence = candidatesFound.Min(c => c.CandidateAction.Precedence);
                return candidatesFound.Where(c => c.CandidateAction.Precedence == highestPrecedence).AsList();
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

                FindActionsForVerbWorker(verb, candidates, listCandidates);

                return listCandidates.ToArray();
            }

            // Adds to existing list rather than send back as a return value.
            private static void FindActionsForVerbWorker(HttpMethod verb, CandidateAction[] candidates, List<CandidateAction> listCandidates)
            {
                foreach (CandidateAction candidate in candidates)
                {
                    if (candidate.ActionDescriptor != null && candidate.ActionDescriptor.SupportedHttpMethods.Contains(verb))
                    {
                        listCandidates.Add(candidate);
                    }
                }
            }

            private static string CreateAmbiguousMatchList(IEnumerable<CandidateActionWithParams> ambiguousCandidates)
            {
                StringBuilder exceptionMessageBuilder = new StringBuilder();
                foreach (CandidateActionWithParams candidate in ambiguousCandidates)
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

        // Associate parameter (route and query) with each action. 
        // For regular routing, there was just a single route, and so single set of route parameters and so all of these
        // may share the same set of combined parameter names.
        // For attribute routing, there may be multiple routes, each with different route parameter names, and 
        // so each instance of a CandidateActionWithParams may have a different parameter set.
        [DebuggerDisplay("{DebuggerToString()}")]
        private class CandidateActionWithParams
        {
            public CandidateActionWithParams(CandidateAction candidateAction, ISet<string> parameters, IHttpRouteData routeDataSource)
            {
                CandidateAction = candidateAction;
                CombinedParameterNames = parameters;
                RouteDataSource = routeDataSource;
            }

            public CandidateAction CandidateAction { get; private set; }
            public ISet<string> CombinedParameterNames { get; private set; }

            // Remember this so that we can apply it for model binding. 
            public IHttpRouteData RouteDataSource { get; private set; }

            public ReflectedHttpActionDescriptor ActionDescriptor
            {
                get
                {
                    return CandidateAction.ActionDescriptor;
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from DebuggerDisplay")]
            private string DebuggerToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(CandidateAction.DebuggerToString());
                if (CombinedParameterNames.Count > 0)
                {
                    sb.Append(", Params =");
                    foreach (string param in CombinedParameterNames)
                    {
                        sb.AppendFormat(" {0}", param);
                    }
                }
                return sb.ToString();
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
