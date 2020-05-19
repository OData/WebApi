// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
#else
    using Microsoft.AspNetCore.Routing;
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

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">IActionDescriptorCollectionProvider instance from dependency injection.</param>
        /// <param name="actionConstraintProviders">ActionConstraintCache instance from dependency injection.</param>
        /// <param name="loggerFactory">ILoggerFactory instance from dependency injection.</param>
        public ODataActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {
            _innerSelector = new ActionSelector(actionDescriptorCollectionProvider, actionConstraintProviders, loggerFactory);
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="innerSelector">The inner action selector.</param>
        public ODataActionSelector(IActionSelector innerSelector)
        {
            _innerSelector = innerSelector;
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

            if (odataPath != null && routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                var odataRoute = routeData.Routers.OfType<ODataRoute>().FirstOrDefault();
                var routePrefix = odataRoute?.RoutePrefix;
                // Get the available parameter names from the route data. Ignore case of key names.
                // Remove route prefix and other non-parameter values from availableKeys
                IList<string> availableKeys = routeData.Values.Keys
                    .Where((key) => routePrefix != "{" + key + "}"
                        && key != ODataRouteConstants.Action
                        && key != ODataRouteConstants.ODataPath)
                    .Select(k => k.ToLowerInvariant())
                    .ToList();

                // Filter out types we know how to bind out of the parameter lists. These values
                // do not show up in RouteData() but will bind properly later.
                var considerCandidates = candidates
                    .Select(c => new ActionIdAndParameters(c.Id, c.Parameters.Count, c.Parameters
                        .Where(p =>
                        {
                            return p.ParameterType != typeof(ODataPath) &&
                            !ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.IsODataQueryOptions(p.ParameterType);
                        })));

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
                var matchedCandidates = considerCandidates
                    .Where(c => TryMatch(context, c.FilteredParameters, availableKeys, optionalWrapper, c.TotalParameterCount))
                    .OrderByDescending(c => c.FilteredParameters.Count)
                    .ThenByDescending(c => c.TotalParameterCount)
                    .ToList();

                return (matchedCandidates.Any())
                    ? candidates.Where(c => c.Id == matchedCandidates.FirstOrDefault().Id).FirstOrDefault()
                    : null;
            }

            return _innerSelector.SelectBestCandidate(context, candidates);
        }

        private bool TryMatch(RouteContext context, IList<ParameterDescriptor> parameters, IList<string> availableKeys, ODataOptionalParameter optionalWrapper, int totalParameterCount)
        {
            if (totalParameterCount == 0 && availableKeys.Count > 0)
            {
                return false;
            }
            bool matchedBody = false;
            var conventionsStore = context.HttpContext.ODataFeature().RoutingConventionsStore;
            // use the parameter name to match.
            foreach (var p in parameters)
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
            return context.HttpContext.Request.ContentLength > 0 ||
                !string.IsNullOrEmpty(context.HttpContext.Request.ContentType);
        }

        private class ActionIdAndParameters
        {
            public ActionIdAndParameters(string id, int parameterCount, IEnumerable<ParameterDescriptor> filteredParameters)
            {
                Id = id;
                TotalParameterCount = parameterCount;
                FilteredParameters = filteredParameters.ToList();
            }

            public string Id { get; set; }

            public int TotalParameterCount { get; set; }

            public IList<ParameterDescriptor> FilteredParameters { get; private set; }
        }
    }
}
