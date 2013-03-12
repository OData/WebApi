// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

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

            FilterScope actionOverride = SelectLastScope<IActionFilter>(overrides);
            FilterScope authenticationOverride = SelectLastScope<IAuthenticationFilter>(overrides);
            FilterScope authorizationOverride = SelectLastScope<IAuthorizationFilter>(overrides);
            FilterScope exceptionOverride = SelectLastScope<IExceptionFilter>(overrides);
            FilterScope resultOverride = SelectLastScope<IResultFilter>(overrides);

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
            return filters.Where(f => f.Scope >= overrideFiltersBeforeScope && (f.Instance is T)).Select(
                f => (T)f.Instance);
        }

        private static FilterScope SelectLastScope<T>(IEnumerable<Filter> overrideFilters)
        {
            Filter lastOverride = overrideFilters.Where(
                f => ((IOverrideFilter)f.Instance).FiltersToOverride == typeof(T)).LastOrDefault();

            if (lastOverride == null)
            {
                return FilterScope.First;
            }

            return lastOverride.Scope;
        }
    }
}
