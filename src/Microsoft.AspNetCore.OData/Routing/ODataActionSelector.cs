//-----------------------------------------------------------------------------
// <copyright file="ODataActionSelector.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.Extensions.Logging;
#endif

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IActionSelector"/> that uses the server's OData routing conventions
    /// to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : IActionSelector
    {
        private readonly IActionSelector _innerSelector;
        private IModelBinderFactory _modelBinderFactory;
        private IModelMetadataProvider _modelMetadataProvider;

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">IActionDescriptorCollectionProvider instance from dependency injection.</param>
        /// <param name="actionConstraintProviders">ActionConstraintCache instance from dependency injection.</param>
        /// <param name="loggerFactory">ILoggerFactory instance from dependency injection.</param>
        /// <param name="modelBinderFactory">IModelBinderFactory instance from dependency injection</param>
        /// <param name="modelMetadataProvider">IModelMetadataProvider instance from dependency injection</param>
        public ODataActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintProviders,
            ILoggerFactory loggerFactory,
            IModelBinderFactory modelBinderFactory,
            IModelMetadataProvider modelMetadataProvider
        )
        {
            _innerSelector = new ActionSelector(actionDescriptorCollectionProvider, actionConstraintProviders, loggerFactory);
            _modelBinderFactory = modelBinderFactory;
            _modelMetadataProvider = modelMetadataProvider;
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="innerSelector">The inner action selector.</param>
        /// <param name="modelBinderFactory">IModelBinderFactory instance from dependency injection</param>
        /// <param name="modelMetadataProvider">IModelMetadataProvider instance from dependency injection</param>
        public ODataActionSelector(
            IActionSelector innerSelector,
            IModelBinderFactory modelBinderFactory,
            IModelMetadataProvider modelMetadataProvider)
        {
            _innerSelector = innerSelector;
            _modelBinderFactory = modelBinderFactory;
            _modelMetadataProvider = modelMetadataProvider;
        }
#endif

        /// <inheritdoc />
        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HttpRequest request = context.HttpContext.Request;
            ODataPath odataPath = context.HttpContext.ODataFeature().Path;
            RouteData routeData = context.RouteData;

            if (odataPath == null || routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                // If there is no path, no routing conventions or there is already and indication we routed it,
                // let the _innerSelector handle the request.
                return _innerSelector.SelectCandidates(context);
            }

            IEnumerable<IODataRoutingConvention> routingConventions = request.GetRoutingConventions();
            if (routingConventions != null)
            {
                foreach (IODataRoutingConvention convention in routingConventions)
                {
                    IEnumerable<ControllerActionDescriptor> actionDescriptor = convention.SelectAction(context);
                    if (actionDescriptor != null && actionDescriptor.Any())
                    {
                        // All actions have the same name but may differ by number of parameters.
                        context.RouteData.Values[ODataRouteConstants.Action] = actionDescriptor.First().ActionName;
                        return actionDescriptor.ToList();
                    }
                }
            }

            return _innerSelector.SelectCandidates(context);
        }

        /// <inheritdoc />
        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            RouteData routeData = context.RouteData;
            ODataPath odataPath = context.HttpContext.ODataFeature().Path;
            IDictionary<string, object> routingConventionsStore = context.HttpContext.ODataFeature().RoutingConventionsStore;

            if (odataPath != null && routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                // Get the available parameter names from the route data. Ignore case of key names.
                // Remove route prefix and other non-parameter values from availableKeys
                IList<string> availableKeys = routeData.Values.Keys
                    .Where((key) => !RoutingConventionHelpers.IsRouteParameter(key)
                        && key != ODataRouteConstants.Action
                        && key != ODataRouteConstants.ODataPath)
                    .Select(k => k.ToLowerInvariant())
                    .ToList();

                int availableKeysCount = 0;
                if (routingConventionsStore.ContainsKey(ODataRouteConstants.KeyCount))
                {
                    availableKeysCount = (int)routingConventionsStore[ODataRouteConstants.KeyCount];
                }

                // Filter out types we know how to bind out of the parameter lists. These values
                // do not show up in RouteData() but will bind properly later.
                IEnumerable<ActionIdAndParameters> considerCandidates = candidates
                    .Select(c => new ActionIdAndParameters(
                        id: c.Id,
                        parameterCount: c.Parameters.Count,
                        filteredParameters: c.Parameters.Where(p => p.ParameterType != typeof(ODataPath) &&
                            !ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.IsODataQueryOptions(p.ParameterType) &&
                            !IsParameterFromQuery(c as ControllerActionDescriptor, p.Name)),
                        descriptor: c));

                // retrieve the optional parameters
                routeData.Values.TryGetValue(ODataRouteConstants.OptionalParameters, out object wrapper);
                ODataOptionalParameter optionalWrapper = wrapper as ODataOptionalParameter;

                // Find the action with the all matched parameters from available keys including
                // matches with no parameters and matches with parameters bound to the body.
                // Ordered first by the total number of matched
                // parameters followed by the total number of parameters.  Ignore case of
                // parameter names. The first one is the best match.
                //
                // Assume key,relatedKey exist in RouteData. 1st one wins:
                // Method(ODataPath,ODataQueryOptions) vs Method(ODataPath).
                // Method(key,ODataQueryOptions) vs Method(key).
                // Method(key,ODataQueryOptions) vs Method(key).
                // Method(key,relatedKey) vs Method(key).
                // Method(key,relatedKey,ODataPath) vs Method(key,relatedKey).
                List<ActionIdAndParameters> matchedCandidates = considerCandidates
                    .Where(c => TryMatch(context, c.ActionDescriptor, c.FilteredParameters, availableKeys,
                        optionalWrapper, c.TotalParameterCount, availableKeysCount))
                    .OrderByDescending(c => c.FilteredParameters.Count)
                    .ThenByDescending(c => c.TotalParameterCount)
                    .ToList();

                // if there are still multiple candidate actions at this point, let's try some tie-breakers
                if (matchedCandidates.Count > 1)
                {
                    // prioritize actions which explicitly declare the request method
                    // e.g. using [AcceptVerbs("POST")], [HttpPost], etc.
                    ActionIdAndParameters bestCandidate = matchedCandidates.FirstOrDefault(candidate =>
                        ActionAcceptsMethod(candidate.ActionDescriptor as ControllerActionDescriptor, context.HttpContext.Request.Method));
                    if (bestCandidate != null)
                    {
                        return bestCandidate.ActionDescriptor;
                    }
                    
                    // also priorize actions that have the exact number of parameters as available keys
                    // this helps disambiguate between overloads of actions that implement actions
                    // e.g. DoSomething(int key) vs DoSomething(), if there are no availableKeys, the
                    // selector could still think that the `int key` param will come from the request body
                    // and end up returning DoSomething(int key) instead of DoSomething()
                    bestCandidate = matchedCandidates.FirstOrDefault(candidate => candidate.FilteredParameters.Count() == availableKeysCount);
                    if (bestCandidate != null)
                    {
                        return bestCandidate.ActionDescriptor;
                    }
                }

                return matchedCandidates.Select(c => c.ActionDescriptor).FirstOrDefault();
            }

            return _innerSelector.SelectBestCandidate(context, candidates);
        }

        /// <summary>
        /// Checks whether the a controller action matches the current route by comparing the parameters
        /// of the action with the data in the route.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext"/></param>
        /// <param name="actionDescriptor">The action descriptor</param>
        /// <param name="parameters">Parameters of the action. This excludes the <see cref="ODataPath"/> and <see cref="Query.ODataQueryOptions"/> parameters</param>
        /// <param name="availableKeys">The names of the keys found in the uri (entity set keys, related keys, operation parameters)</param>
        /// <param name="optionalWrapper">Used to check whether a parameter is optional</param>
        /// <param name="totalParameterCount">Total number of parameters in the action method</param>
        /// <param name="availableKeysCount">The number of key segments found in the uri.
        /// This might be less than the size of <paramref name="availableKeys"/> because some keys might have alias names</param>
        /// <returns></returns>
        private bool TryMatch(
            RouteContext context,
            ActionDescriptor actionDescriptor,
            IList<ParameterDescriptor> parameters,
            IList<string> availableKeys,
            ODataOptionalParameter optionalWrapper,
            int totalParameterCount,
            int availableKeysCount)
        {

            // if action has [EnableNestedPaths] attribute, then it doesn't
            // need to match parameters, since this action is expected to
            // match arbitrarily nested paths even if it doesn't have any parameters
            if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                if (controllerActionDescriptor.MethodInfo
                    .GetCustomAttributes<EnableNestedPathsAttribute>().Any())
                {
                    return true;
                }
            }

            // navigationProperty is optional in some cases, therefore an action
            // should not be rejected simply because it does not declare a navigationProperty parameter
            if (availableKeys.Contains(ODataRouteConstants.NavigationProperty.ToLowerInvariant()))
            {
                availableKeysCount -= 1;
            }

            // reject action if it doesn't declare a parameter for each segment key
            // e.g. Get() will be rejected for route /Persons/1
            if (totalParameterCount < availableKeysCount)
            {
                return false;
            }

            bool matchedBody = false;
            IDictionary<string, object> conventionsStore = context.HttpContext.ODataFeature().RoutingConventionsStore;
            // use the parameter name to match.
            foreach (ParameterDescriptor p in parameters)
            {
                string parameterName = p.Name.ToLowerInvariant();
                if (availableKeys.Contains(parameterName))
                {
                    continue;
                }

                if (conventionsStore != null)
                {
                    // the convention store can contain the parameter as key
                    // with a nested property (e.g. customer.Name) 
                    if (conventionsStore.Keys.Any(k => k.Contains(p.Name)))
                    {
                        continue;
                    }
                }

                if (context.HttpContext.Request.Query.ContainsKey(p.Name))
                {
                    continue;
                }

                ControllerParameterDescriptor cP = p as ControllerParameterDescriptor;
                if (cP != null && optionalWrapper != null)
                {
                    if (cP.ParameterInfo.IsOptional && optionalWrapper.OptionalParameters.Any(o => o.Name.ToLowerInvariant() == parameterName))
                    {
                        continue;
                    }
                }

                // if we can't find the parameter in the request, check whether
                // there's a special model binder registered to handle it
                if (ParameterHasRegisteredModelBinder(p))
                {
                    continue;
                }

                // if parameter is not bound to a key in the path,
                // assume that it's bound to the request body
                // only one parameter should be considered bound to the body
                if (!matchedBody && RequestHasBody(context))
                {
                    matchedBody = true;
                    continue;
                }

                return false;
            }

            return true;
        }
        private bool RequestHasBody(RouteContext context)
        {
            string method = context.HttpContext.Request.Method;
            return method.Equals("post", StringComparison.OrdinalIgnoreCase) ||
                method.Equals("put", StringComparison.OrdinalIgnoreCase) ||
                method.Equals("patch", StringComparison.OrdinalIgnoreCase) ||
                method.Equals("merge", StringComparison.OrdinalIgnoreCase);
        }

        private bool ActionAcceptsMethod(ControllerActionDescriptor action, string method)
        {
            Contract.Assert(action != null);

            return action.MethodInfo.GetCustomAttributes(false)
                .OfType<IActionHttpMethodProvider>()
                .Any(methodProvider => methodProvider.HttpMethods.Contains(method.ToUpperInvariant()));
        }

        private bool IsParameterFromQuery(ControllerActionDescriptor action, string paramName)
        {
            Contract.Assert(action != null);

            ParameterInfo param = action.MethodInfo.GetParameters().FirstOrDefault(p => p.Name == paramName);
            if (param == null)
            {
                return false;
            }

            return param.GetCustomAttributes(false).OfType<FromQueryAttribute>().Any();
        }

        private bool ParameterHasRegisteredModelBinder(ParameterDescriptor param)
        {
            if (_modelBinderFactory == null || _modelMetadataProvider == null)
            {
                return false;
            }

            ModelMetadata modelMetadata = _modelMetadataProvider.GetMetadataForType(param.ParameterType);
            ModelBinderFactoryContext binderContext = new ModelBinderFactoryContext()
            {
                Metadata = modelMetadata,
                BindingInfo = param.BindingInfo,
                // This is the same cache token used by aspnetcore when updating models
                CacheToken = modelMetadata,
            };

            try
            {
                IModelBinder binder = _modelBinderFactory.CreateBinder(binderContext);
                // ignore some built-in model binders because we already account for parameters that come from the request
                return (!(binder is SimpleTypeModelBinder)
                    && !(binder is BodyModelBinder)
                    && !(binder is ComplexTypeModelBinder)
                    && !(binder is BinderTypeModelBinder));
            }
            catch { }

            return false;
        }

        private class ActionIdAndParameters
        {
            public ActionIdAndParameters(string id, int parameterCount, IEnumerable<ParameterDescriptor> filteredParameters, ActionDescriptor descriptor)
            {
                Id = id;
                TotalParameterCount = parameterCount;
                FilteredParameters = filteredParameters.ToList();
                ActionDescriptor = descriptor;
            }

            public string Id { get; set; }

            public int TotalParameterCount { get; set; }

            public IList<ParameterDescriptor> FilteredParameters { get; private set; }

            public ActionDescriptor ActionDescriptor { get; set; }
        }
    }
}
