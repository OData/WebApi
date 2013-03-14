// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private HttpRequestMessage _request;
        private ModelStateDictionary _modelState;
        private HttpConfiguration _configuration;
        private HttpControllerContext _controllerContext;
        private UrlHelper _urlHelper;
        private IPrincipalProvider _principalProvider;

        /// <summary>
        /// Gets the <see name="HttpRequestMessage"/> of the current ApiController.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _request = value;
            }
        }

        /// <summary>
        /// Gets the <see name="HttpConfiguration"/> of the current ApiController.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _configuration = value;
            }
        }

        /// <summary>
        /// Gets the <see name="HttpControllerContext"/> of the current ApiController.
        ///
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public HttpControllerContext ControllerContext
        {
            get { return _controllerContext; }
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
                if (_urlHelper == null)
                {
                    _urlHelper = Request.GetUrlHelper();
                }

                return _urlHelper;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _urlHelper = value;
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
            if (_request != null)
            {
                // if user has registered a controller factory which produces the same controller instance, we should throw here
                throw Error.InvalidOperation(SRResources.CannotSupportSingletonInstance, typeof(ApiController).Name, typeof(IHttpControllerActivator).Name);
            }

            Initialize(controllerContext);

            // We can't be reused, and we know we're disposable, so make sure we go away when
            // the request has been completed.
            if (_request != null)
            {
                _request.RegisterForDispose(this);
            }

            HttpControllerDescriptor controllerDescriptor = controllerContext.ControllerDescriptor;
            ServicesContainer controllerServices = controllerDescriptor.Configuration.Services;
            HttpActionDescriptor actionDescriptor = controllerServices.GetActionSelector().SelectAction(controllerContext);

            if (_request != null)
            {
                _request.Properties[HttpPropertyKeys.HttpActionDescriptorKey] = actionDescriptor;
            }

            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);

            IEnumerable<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            FilterGrouping filterGrouping = new FilterGrouping(filters);

            IEnumerable<IActionFilter> actionFilters = filterGrouping.ActionFilters;
            IEnumerable<IAuthenticationFilter> authenticationFilters = filterGrouping.AuthenticationFilters;
            IEnumerable<IAuthorizationFilter> authorizationFilters = filterGrouping.AuthorizationFilters;
            IEnumerable<IExceptionFilter> exceptionFilters = filterGrouping.ExceptionFilters;

            Task<HttpResponseMessage> result = InvokeActionWithAuthenticationFilters(actionContext, cancellationToken, authenticationFilters, () =>
                // Func<Task<HttpResponseMessage>>
                InvokeActionWithAuthorizationFilters(actionContext, cancellationToken, authorizationFilters, authenticationFilters, () =>
                    {
                        return ExecuteAction(actionDescriptor.ActionBinding, actionContext, cancellationToken, actionFilters, controllerServices);
                    })());

            return InvokeActionWithExceptionFilters(result, actionContext, cancellationToken, exceptionFilters);
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

            ControllerContext = controllerContext;

            _request = controllerContext.Request;
            _configuration = controllerContext.Configuration;
            _principalProvider = _configuration.Services.GetPrincipalProvider();

            if (_principalProvider == null)
            {
                throw new InvalidOperationException(SRResources.ServicesContainerIPrincipalProviderRequired);
            }
        }

        internal static async Task<HttpResponseMessage> InvokeActionWithExceptionFilters(Task<HttpResponseMessage> actionTask, HttpActionContext actionContext, CancellationToken cancellationToken, IEnumerable<IExceptionFilter> filters)
        {
            Contract.Assert(actionTask != null);
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);

            Exception exception = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await actionTask;
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

        internal async Task<HttpResponseMessage> InvokeActionWithAuthenticationFilters(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IEnumerable<IAuthenticationFilter> filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            foreach (IAuthenticationFilter filter in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IAuthenticationResult result = await filter.AuthenticateAsync(actionContext, cancellationToken);

                if (result != null)
                {
                    IHttpActionResult error = result.ErrorResult;

                    if (error != null)
                    {
                        // The first authentication filter to provide an error result or principal wins.

                        foreach (IAuthenticationFilter errorFilter in filters)
                        {
                            if (errorFilter != filter)
                            {
                                // But, on error, the other authentication filters still collaborate to provide the
                                // authentication challenge.
                                cancellationToken.ThrowIfCancellationRequested();
                                error = await errorFilter.ChallengeAsync(actionContext, error, cancellationToken);
                            }
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                        return await error.ExecuteAsync(cancellationToken);
                    }

                    IPrincipal principal = result.Principal;

                    if (principal != null)
                    {
                        // The first authentication filter to provide an error result or principal wins.
                        _principalProvider.CurrentPrincipal = principal;
                        break;
                    }
                }
            }

            return await innerAction();
        }

        internal static Func<Task<HttpResponseMessage>> InvokeActionWithAuthorizationFilters(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IEnumerable<IAuthorizationFilter> filters, IEnumerable<IAuthenticationFilter> authenticationFilters,
            Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(authenticationFilters != null);
            Contract.Assert(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            filters = filters.Reverse();

            Func<Task<HttpResponseMessage>> result = filters.Aggregate(innerAction, (continuation, filter) =>
            {
                return () => InvokeActionWithAuthorizationFilter(filter, actionContext, cancellationToken, authenticationFilters, continuation);
            });

            return result;
        }

        internal static async Task<HttpResponseMessage> InvokeActionWithAuthorizationFilter(
            IAuthorizationFilter filter, HttpActionContext actionContext, CancellationToken cancellationToken,
            IEnumerable<IAuthenticationFilter> authenticationFilters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(filter != null);
            Contract.Assert(actionContext != null);
            Contract.Assert(authenticationFilters != null);
            Contract.Assert(innerAction != null);

            cancellationToken.ThrowIfCancellationRequested();
            HttpResponseMessage response = await filter.ExecuteAuthorizationFilterAsync(actionContext,
                cancellationToken, innerAction);

            // NOTE: Unlike MVC, there's no way to tell if an authorization filter failed. So we have to rely on the
            // status code and do filter-layer authentication even if not doing filter-layer authorization. Consider a
            // new authorization filter type in Web API with MVC-like action result semantics to resolve this
            // discrepancy.

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                IHttpActionResult currentResult = new MessageResult(response);

                foreach (IAuthenticationFilter authenticationFilter in authenticationFilters)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    currentResult = await authenticationFilter.ChallengeAsync(actionContext, currentResult,
                        cancellationToken);
                }

                return await currentResult.ExecuteAsync(cancellationToken);
            }
            else
            {
                return response;
            }
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

                FilterScope actionOverride = SelectLastScope<IActionFilter>(overrides);
                FilterScope authenticationOverride = SelectLastScope<IAuthenticationFilter>(overrides);
                FilterScope authorizationOverride = SelectLastScope<IAuthorizationFilter>(overrides);
                FilterScope exceptionOverride = SelectLastScope<IExceptionFilter>(overrides);

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
                return filters.Where(f => f.Scope >= overrideFiltersBeforeScope
                    && (f.Instance is T)).Select(f => (T)f.Instance);
            }

            private static FilterScope SelectLastScope<T>(IEnumerable<FilterInfo> overrideFilters)
            {
                FilterInfo lastOverride = overrideFilters.Where(
                    f => ((IOverrideFilter)f.Instance).FiltersToOverride == typeof(T)).LastOrDefault();

                if (lastOverride == null)
                {
                    return FilterScope.Global;
                }

                return lastOverride.Scope;
            }
        }
    }
}