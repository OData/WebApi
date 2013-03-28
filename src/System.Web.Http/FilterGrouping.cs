// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Filters;

namespace System.Web.Http
{
    /// <summary>
    /// Quickly split filters into different types
    /// </summary>
    internal class FilterGrouping
    {
        private readonly List<IActionFilter> _actionFilters = new List<IActionFilter>();
        private readonly List<IAuthenticationFilter> _authenticationFilters = new List<IAuthenticationFilter>();
        private readonly List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
        private readonly List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();

        public FilterGrouping(IEnumerable<FilterInfo> filters)
        {
            // evaluate the 'filters' enumerable only once since the operation can be quite expensive
            var cache = filters.ToList();

            var overrides = cache.Where(f => f.Instance is IOverrideFilter);

            FilterScope actionOverride = SelectLastOverrideScope<IActionFilter>(overrides);
            FilterScope authenticationOverride = SelectLastOverrideScope<IAuthenticationFilter>(overrides);
            FilterScope authorizationOverride = SelectLastOverrideScope<IAuthorizationFilter>(overrides);
            FilterScope exceptionOverride = SelectLastOverrideScope<IExceptionFilter>(overrides);

            _actionFilters.AddRange(SelectAvailable<IActionFilter>(cache, actionOverride));
            _authenticationFilters.AddRange(SelectAvailable<IAuthenticationFilter>(cache, authenticationOverride));
            _authorizationFilters.AddRange(SelectAvailable<IAuthorizationFilter>(cache, authorizationOverride));
            _exceptionFilters.AddRange(SelectAvailable<IExceptionFilter>(cache, exceptionOverride));
        }

        public IEnumerable<IActionFilter> ActionFilters
        {
            get { return _actionFilters; }
        }

        public IEnumerable<IAuthenticationFilter> AuthenticationFilters
        {
            get { return _authenticationFilters; }
        }

        public IEnumerable<IAuthorizationFilter> AuthorizationFilters
        {
            get { return _authorizationFilters; }
        }

        public IEnumerable<IExceptionFilter> ExceptionFilters
        {
            get { return _exceptionFilters; }
        }

        private static IEnumerable<T> SelectAvailable<T>(List<FilterInfo> filters,
            FilterScope overrideFiltersBeforeScope)
        {
            // Determine which filters are available for this filter type, given the current overrides in place.
            // A filter should be processed if:
            //  1. It implements the appropriate interface for this filter type.
            //  2. It has not been overridden (its scope is not before the scope of the last override for this
            //     type).
            return filters.Where(f => f.Scope >= overrideFiltersBeforeScope
                && (f.Instance is T)).Select(f => (T)f.Instance);
        }

        private static FilterScope SelectLastOverrideScope<T>(IEnumerable<FilterInfo> overrideFilters)
        {
            // A filter type (such as action filter) can be overridden, which means every filter of that type at an
            // earlier scope must be ignored. Determine the scope of the last override filter (if any). Only
            // filters at this scope or later will be processed.

            FilterInfo lastOverride = overrideFilters.Where(
                f => ((IOverrideFilter)f.Instance).FiltersToOverride == typeof(T)).LastOrDefault();

            // If no override is present, the filter is not overridden (and filters at any scope, starting with
            // First are processed). Not overriding a filter is equivalent to placing an override at the First
            // filter scope (since there's nothing before First to override).
            if (lastOverride == null)
            {
                return FilterScope.Global;
            }

            return lastOverride.Scope;
        }
    }
}
