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

            FilterGrouping filterGrouping = actionDescriptor.GetFilterGrouping();

            IActionFilter[] actionFilters = filterGrouping.ActionFilters;
            IAuthenticationFilter[] authenticationFilters = filterGrouping.AuthenticationFilters;
            IAuthorizationFilter[] authorizationFilters = filterGrouping.AuthorizationFilters;
            IExceptionFilter[] exceptionFilters = filterGrouping.ExceptionFilters;

            Func<Task<HttpResponseMessage>> result = InvokeActionWithAuthenticationFilters(actionContext, cancellationToken, authenticationFilters,
                InvokeActionWithAuthorizationFilters(actionContext, cancellationToken, authorizationFilters, () =>
                    {
                        return ExecuteAction(actionDescriptor.ActionBinding, actionContext, cancellationToken, actionFilters, controllerServices);
                    }));

            return InvokeActionWithExceptionFilters(result, actionContext, cancellationToken, exceptionFilters);
        }

        /// <summary>Creates a <see cref="NegotiatedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="NegotiatedContentResult{T}"/> with the specified values.</returns>
        public NegotiatedContentResult<T> Content<T>(HttpStatusCode statusCode, T value)
        {
            return new NegotiatedContentResult<T>(statusCode, value, this);
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
            CancellationToken cancellationToken, IActionFilter[] actionFilters, ServicesContainer controllerServices)
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

        internal static async Task<HttpResponseMessage> InvokeActionWithExceptionFilters(Func<Task<HttpResponseMessage>> innerAction, HttpActionContext actionContext, CancellationToken cancellationToken, IExceptionFilter[] filters)
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
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                IExceptionFilter exceptionFilter = filters[i];
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
            IAuthenticationFilter[] filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            return () => InvokeActionWithAuthenticationFiltersAsync(actionContext, cancellationToken, filters,
                innerAction);
        }

        internal async Task<HttpResponseMessage> InvokeActionWithAuthenticationFiltersAsync(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IAuthenticationFilter[] filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            IHttpActionResult innerResult = new ContinuationResult(innerAction);
            HttpAuthenticationContext authenticationContext = new HttpAuthenticationContext(actionContext);
            authenticationContext.Principal = _principalService.GetCurrentPrincipal(Request);

            for (int i = 0; i < filters.Length; i++)
            {
                IAuthenticationFilter filter = filters[i];
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
                        _principalService.SetCurrentPrincipal(principal, Request);
                    }
                }
            }

            for (int i = 0; i < filters.Length; i++)
            {
                IAuthenticationFilter filter = filters[i];
                cancellationToken.ThrowIfCancellationRequested();
                innerResult = await filter.ChallengeAsync(actionContext, innerResult, cancellationToken) ??
                    innerResult;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await innerResult.ExecuteAsync(cancellationToken);
        }

        internal static Func<Task<HttpResponseMessage>> InvokeActionWithAuthorizationFilters(
            HttpActionContext actionContext, CancellationToken cancellationToken,
            IAuthorizationFilter[] filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            // We need to reverse the filter list so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            Func<Task<HttpResponseMessage>> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                IAuthorizationFilter filter = filters[i];
                Func<Func<Task<HttpResponseMessage>>, IAuthorizationFilter, Func<Task<HttpResponseMessage>>> chainContinuation = (continuation, innerFilter) =>
                {
                    return () => innerFilter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, continuation);
                };
                result = chainContinuation(result, filter);
            }

            return result;
        }

        internal static Func<Task<HttpResponseMessage>> InvokeActionWithActionFilters(HttpActionContext actionContext, CancellationToken cancellationToken, IActionFilter[] filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            Func<Task<HttpResponseMessage>> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                IActionFilter filter = filters[i];
                Func<Func<Task<HttpResponseMessage>>, IActionFilter, Func<Task<HttpResponseMessage>>> chainContinuation = (continuation, innerFilter) =>
                {
                    return () => innerFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, continuation);
                };
                result = chainContinuation(result, filter);
            }

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
    }
}