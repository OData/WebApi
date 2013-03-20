// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Filters;

namespace System.Web.Mvc
{
    public class FilterInfo
    {
        private readonly List<IActionFilter> _actionFilters = new List<IActionFilter>();
        private readonly List<IAuthenticationFilter> _authenticationFilters = new List<IAuthenticationFilter>();
        private readonly List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
        private readonly List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();
        private readonly List<IResultFilter> _resultFilters = new List<IResultFilter>();

        public FilterInfo()
        {
        }

        public FilterInfo(IEnumerable<Filter> filters)
        {
            // evaluate the 'filters' enumerable only once since the operation can be quite expensive
            var cache = filters.ToList();

            var overrides = cache.Where(f => f.Instance is IOverrideFilter);

            FilterScope actionOverride = SelectLastOverrideScope<IActionFilter>(overrides);
            FilterScope authenticationOverride = SelectLastOverrideScope<IAuthenticationFilter>(overrides);
            FilterScope authorizationOverride = SelectLastOverrideScope<IAuthorizationFilter>(overrides);
            FilterScope exceptionOverride = SelectLastOverrideScope<IExceptionFilter>(overrides);
            FilterScope resultOverride = SelectLastOverrideScope<IResultFilter>(overrides);

            _actionFilters.AddRange(SelectAvailable<IActionFilter>(cache, actionOverride));
            _authenticationFilters.AddRange(SelectAvailable<IAuthenticationFilter>(cache, authenticationOverride));
            _authorizationFilters.AddRange(SelectAvailable<IAuthorizationFilter>(cache, authorizationOverride));
            _exceptionFilters.AddRange(SelectAvailable<IExceptionFilter>(cache, exceptionOverride));
            _resultFilters.AddRange(SelectAvailable<IResultFilter>(cache, resultOverride));
        }

        public IList<IActionFilter> ActionFilters
        {
            get { return _actionFilters; }
        }

        public IList<IAuthenticationFilter> AuthenticationFilters
        {
            get { return _authenticationFilters; }
        }

        public IList<IAuthorizationFilter> AuthorizationFilters
        {
            get { return _authorizationFilters; }
        }

        public IList<IExceptionFilter> ExceptionFilters
        {
            get { return _exceptionFilters; }
        }

        public IList<IResultFilter> ResultFilters
        {
            get { return _resultFilters; }
        }

        private static IEnumerable<T> SelectAvailable<T>(List<Filter> filters, FilterScope overrideFiltersBeforeScope)
        {
            // Determine which filters are available for this filter type, given the current overrides in place.
            // A filter should be processed if:
            //  1. It implements the appropriate interface for this filter type.
            //  2. It has not been overridden (its scope is not before the scope of the last override for this type).
            return filters.Where(f => f.Scope >= overrideFiltersBeforeScope && (f.Instance is T)).Select(
                f => (T)f.Instance);
        }

        private static FilterScope SelectLastOverrideScope<T>(IEnumerable<Filter> overrideFilters)
        {
            // A filter type (such as action filter) can be overridden, which means every filter of that type at an
            // earlier scope must be ignored. Determine the scope of the last override filter (if any). Only filters at
            // this scope or later will be processed.

            Filter lastOverride = overrideFilters.Where(
                f => ((IOverrideFilter)f.Instance).FiltersToOverride == typeof(T)).LastOrDefault();

            // If no override is present, the filter is not overridden (and filters at any scope, starting with First
            // are processed). Not overriding a filter is equivalent to placing an override at the First filter scope
            // (since there's nothing before First to override).
            if (lastOverride == null)
            {
                return FilterScope.First;
            }

            return lastOverride.Scope;
        }
    }
}
