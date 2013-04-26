// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    public abstract class ApiController : IHttpController, IDisposable
    {
        private bool _disposed;
        private ModelStateDictionary _modelState;
        private HttpControllerContext _controllerContext;
        private IHostPrincipalService _principalService;
        private bool _initialized;

        /// <summary>
        /// Gets the <see name="HttpRequestMessage"/> of the current ApiController.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return ControllerContext.Request; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                ControllerContext.Request = value;
            }
        }

        /// <summary>
        /// Gets the <see name="HttpConfiguration"/> of the current ApiController.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get
            {
                return ControllerContext.Configuration;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                ControllerContext.Configuration = value;
            }
        }

        /// <summary>
        /// Gets the <see name="HttpControllerContext"/> of the current ApiController.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public HttpControllerContext ControllerContext
        {
            get
            {
                // unit test only.
                if (_controllerContext == null)
                {
                    _controllerContext = new HttpControllerContext();
                }

                return _controllerContext;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _controllerContext = value;
            }
        }

        /// <summary>
        /// Gets model state after the model binding process. This ModelState will be empty before model binding happens.
        /// Please do not populate this property other than for unit testing purpose.
        /// </summary>
        public ModelStateDictionary ModelState
        {
            get
            {
                if (_modelState == null)
                {
                    // The getter is not intended to be used by multiple threads, so it is fine to initialize here
                    _modelState = new ModelStateDictionary();
                }

                return _modelState;
            }
        }

        /// <summary>
        /// Gets an instance of a <see name="UrlHelper" />, which is used to generate URLs to other APIs.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public UrlHelper Url
        {
            get
            {
                if (Request == null)
                {
                    return null;
                }
                return Request.GetUrlHelper();
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                ThrowIfRequestIsNull();
                Request.SetUrlHelper(value);
            }
        }

        public IHttpRouteData RouteData
        {
            get
            {
                return ControllerContext.RouteData;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                ControllerContext.RouteData = value;
            }
        }

        /// <summary>
        /// Returns the current principal associated with this request.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "That would make for poor usability.")]
        public IPrincipal User
        {
            get { return Thread.CurrentPrincipal; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This method is a coordinator, so this coupling is expected.")]
        public virtual Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            if (_initialized)
            {
                // if user has registered a controller factory which produces the same controller instance, we should throw here
                throw Error.InvalidOperation(SRResources.CannotSupportSingletonInstance, typeof(ApiController).Name, typeof(IHttpControllerActivator).Name);
            }

            Initialize(controllerContext);

            // We can't be reused, and we know we're disposable, so make sure we go away when
            // the request has been completed.
            if (Request != null)
            {
                Request.RegisterForDispose(this);
            }

            HttpControllerDescriptor controllerDescriptor = controllerContext.ControllerDescriptor;
            ServicesContainer controllerServices = controllerDescriptor.Configuration.Services;
            HttpActionDescriptor actionDescriptor = controllerServices.GetActionSelector().SelectAction(controllerContext);

            if (Request != null)
            {
                Request.Properties[HttpPropertyKeys.HttpActionDescriptorKey] = actionDescriptor;
            }

            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);

            IEnumerable<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            FilterGrouping filterGrouping = new FilterGrouping(filters);

            IEnumerable<IActionFilter> actionFilters = filterGrouping.ActionFilters;
            IEnumerable<IAuthenticationFilter> authenticationFilters = filterGrouping.AuthenticationFilters;
            IEnumerable<IAuthorizationFilter> authorizationFilters = filterGrouping.AuthorizationFilters;
            IEnumerable<IExceptionFilter> exceptionFilters = filterGrouping.ExceptionFilters;

            Func<Task<HttpResponseMessage>> result = InvokeActionWithAuthenticationFilters(actionContext, cancellationToken, authenticationFilters,
                InvokeActionWithAuthorizationFilters(actionContext, cancellationToken, authorizationFilters, () =>
                    {
                        return ExecuteAction(actionDescriptor.ActionBinding, actionContext, cancellationToken, actionFilters, controllerServices);
                    }));

            return InvokeActionWithExceptionFilters(result, actionContext, cancellationToken, exceptionFilters);
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        public FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value, MediaTypeFormatter formatter)
        {
            return Content(statusCode, value, formatter, (MediaTypeHeaderValue)null);
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">The value for the Content-Type header.</param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        public FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value, MediaTypeFormatter formatter,
            string mediaType)
        {
            return Content(statusCode, value, formatter, new MediaTypeHeaderValue(mediaType));
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">
        /// The value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        public FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType)
        {
            return new FormattedContentResult<T>(statusCode, value, formatter, mediaType, this);
        }

        /// <summary>Creates a <see cref="MessageResult"/> with the specified response.</summary>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>A <see cref="MessageResult"/> for the specified response.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Consistency and discoverability are better with an instance method.")]
        public MessageResult Message(HttpResponseMessage response)
        {
            return new MessageResult(response);
        }

        /// <summary>Creates a <see cref="StatusCodeResult"/> with the specified status code.</summary>
        /// <param name="status">The HTTP status code for the response message</param>
        /// <returns>A <see cref="StatusCodeResult"/> with the specified status code.</returns>
        public StatusCodeResult StatusCode(HttpStatusCode status)
        {
            return new StatusCodeResult(status, this);
        }

        private async Task<HttpResponseMessage> ExecuteAction(HttpActionBinding actionBinding, HttpActionContext actionContext,
            CancellationToken cancellationToken, IEnumerable<IActionFilter> actionFilters, ServicesContainer controllerServices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await actionBinding.ExecuteBindingAsync(actionContext, cancellationToken);

            _modelState = actionContext.ModelState;
            cancellationToken.ThrowIfCancellationRequested();
            return await InvokeActionWithActionFilters(actionContext, cancellationToken, actionFilters, () =>
            {
                return controllerServices.GetActionInvoker().InvokeActionAsync(actionContext, cancellationToken);
            })();
        }

        protected virtual void Initialize(HttpControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            _initialized = true;
            _controllerContext = controllerContext;
            _principalService = Configuration.Services.GetHostPrincipalService();

            if (_principalService == null)
            {
                throw new InvalidOperationException(SRResources.ServicesContainerIHostPrincipalServiceRequired);
            }
        }

        internal static async Task<HttpResponseMessage> InvokeActionWithExceptionFilters(Func<Task<HttpResponseMessage>> innerAction, HttpActionContext actionContext, CancellationToken cancellationToken, IEnumerable<IExceptionFilter> filters)
        {
            Contract.Assert(innerAction != null);
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);

            Exception exception = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await innerAction();
            }
            catch (Exception e)
            {
                exception = e;
            }

            // This code path only runs if the task is faulted with an exception
            Contract.Assert(exception != null);

            HttpActionExecutedContext executedContext = new HttpActionExecutedContext(actionContext, exception);

            // Note: exception filters need to be scheduled in the reverse order so that
            // the more specific filter (e.g. Action) executes before the less specific ones (e.g. Global)
            filters = filters.Reverse();

            foreach (IExceptionFilter exceptionFilter in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await exceptionFilter.ExecuteExceptionFilterAsync(executedContext, cancellationToken);
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }
            else
            {
                throw executedContext.Exception;
            }
        }

        internal Func<Task<HttpResponseMessage>> InvokeActionWithAuthenticationFilters(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IEnumerable<IAuthenticationFilter> filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            return () => InvokeActionWithAuthenticationFiltersAsync(actionContext, cancellationToken, filters,
                innerAction);
        }

        internal async Task<HttpResponseMessage> InvokeActionWithAuthenticationFiltersAsync(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IEnumerable<IAuthenticationFilter> filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            IHttpActionResult innerResult = new ContinuationResult(innerAction);
            HttpAuthenticationContext authenticationContext = new HttpAuthenticationContext(actionContext);
            authenticationContext.Principal = _principalService.CurrentPrincipal;

            foreach (IAuthenticationFilter filter in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IAuthenticationResult result = await filter.AuthenticateAsync(authenticationContext,
                    cancellationToken);

                if (result != null)
                {
                    IHttpActionResult error = result.ErrorResult;

                    // Short-circuit on the first authentication filter to provide an error result.
                    if (error != null)
                    {
                        innerResult = error;
                        break;
                    }

                    IPrincipal principal = result.Principal;

                    if (principal != null)
                    {
                        authenticationContext.Principal = principal;
                        _principalService.CurrentPrincipal = principal;
                    }
                }
            }

            foreach (IAuthenticationFilter filter in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                innerResult = await filter.ChallengeAsync(actionContext, innerResult, cancellationToken) ??
                    innerResult;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await innerResult.ExecuteAsync(cancellationToken);
        }

        internal static Func<Task<HttpResponseMessage>> InvokeActionWithAuthorizationFilters(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IEnumerable<IAuthorizationFilter> filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            filters = filters.Reverse();

            Func<Task<HttpResponseMessage>> result = filters.Aggregate(innerAction, (continuation, filter) =>
            {
                return () => filter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, continuation);
            });

            return result;
        }

        internal static Func<Task<HttpResponseMessage>> InvokeActionWithActionFilters(HttpActionContext actionContext, CancellationToken cancellationToken, IEnumerable<IActionFilter> filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            filters = filters.Reverse();

            Func<Task<HttpResponseMessage>> result = filters.Aggregate(innerAction, (continuation, filter) =>
            {
                return () => filter.ExecuteActionFilterAsync(actionContext, cancellationToken, continuation);
            });

            return result;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // TODO: Dispose controller state
                }
            }
        }

        #endregion IDisposable

        private void ThrowIfRequestIsNull()
        {
            if (ControllerContext.Request == null)
            {
                throw Error.InvalidOperation(SRResources.RequestIsNull, GetType().Name);
            }
        }

        /// <summary>
        /// Quickly split filters into different types
        /// </summary>
        /// <remarks>Avoid <see cref="M:ReadOnlyCollection.Select"/> because it has a very slow implementation that shows on profiles.</remarks>
        private class FilterGrouping
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
}