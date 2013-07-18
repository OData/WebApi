// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Newtonsoft.Json;

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
            internal set
            {
                Contract.Assert(value != null);
                _modelState = value;
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

            IHttpActionResult result = new ActionFilterResult(actionDescriptor.ActionBinding, actionContext, this,
                controllerServices, actionFilters);
            if (authorizationFilters.Length > 0)
            {
                result = new AuthorizationFilterResult(actionContext, authorizationFilters, result);
            }
            if (authenticationFilters.Length > 0)
            {
                result = new AuthenticationFilterResult(actionContext, authenticationFilters, _principalService,
                    controllerContext.Request, result);
            }

            return InvokeActionWithExceptionFilters(result, actionContext, cancellationToken, exceptionFilters);
        }

        /// <summary>Creates a <see cref="BadRequestResult"/> (400 Bad Request).</summary>
        /// <returns>A <see cref="BadRequestResult"/>.</returns>
        protected internal virtual BadRequestResult BadRequest()
        {
            return new BadRequestResult(this);
        }

        /// <summary>
        /// Creates an <see cref="ErrorMessageResult"/> (400 Bad Request) with the specified error message.
        /// </summary>
        /// <param name="message">The user-visible error message.</param>
        /// <returns>An <see cref="InvalidModelStateResult"/> with the specified model state.</returns>
        protected internal virtual ErrorMessageResult BadRequest(string message)
        {
            return new ErrorMessageResult(message, this);
        }

        /// <summary>
        /// Creates an <see cref="InvalidModelStateResult"/> (400 Bad Request) with the specified model state.
        /// </summary>
        /// <param name="modelState">The model state to include in the error.</param>
        /// <returns>An <see cref="InvalidModelStateResult"/> with the specified model state.</returns>
        protected internal virtual InvalidModelStateResult BadRequest(ModelStateDictionary modelState)
        {
            return new InvalidModelStateResult(modelState, this);
        }

        /// <summary>Creates a <see cref="ConflictResult"/> (409 Conflict).</summary>
        /// <returns>A <see cref="ConflictResult"/>.</returns>
        protected internal virtual ConflictResult Conflict()
        {
            return new ConflictResult(this);
        }

        /// <summary>Creates a <see cref="NegotiatedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="NegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual NegotiatedContentResult<T> Content<T>(HttpStatusCode statusCode, T value)
        {
            return new NegotiatedContentResult<T>(statusCode, value, this);
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        protected internal FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value,
            MediaTypeFormatter formatter)
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
        protected internal FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value,
            MediaTypeFormatter formatter, string mediaType)
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
        protected internal virtual FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value,
            MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
        {
            return new FormattedContentResult<T>(statusCode, value, formatter, mediaType, this);
        }

        /// <summary>
        /// Creates a <see cref="CreatedNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="location">The location at which the content has been created.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal CreatedNegotiatedContentResult<T> Created<T>(string location, T content)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            return Created<T>(new Uri(location), content);
        }

        /// <summary>
        /// Creates a <see cref="CreatedNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="location">The location at which the content has been created.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual CreatedNegotiatedContentResult<T> Created<T>(Uri location, T content)
        {
            return new CreatedNegotiatedContentResult<T>(location, content, this);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal CreatedAtRouteNegotiatedContentResult<T> CreatedAtRoute<T>(string routeName,
            object routeValues, T content)
        {
            return CreatedAtRoute<T>(routeName, new HttpRouteValueDictionary(routeValues), content);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual CreatedAtRouteNegotiatedContentResult<T> CreatedAtRoute<T>(string routeName,
            IDictionary<string, object> routeValues, T content)
        {
            return new CreatedAtRouteNegotiatedContentResult<T>(routeName, routeValues, content, this);
        }

        /// <summary>Creates an <see cref="InternalServerErrorResult"/> (500 Internal Server Error).</summary>
        /// <returns>A <see cref="InternalServerErrorResult"/>.</returns>
        protected internal virtual InternalServerErrorResult InternalServerError()
        {
            return new InternalServerErrorResult(this);
        }

        /// <summary>
        /// Creates an <see cref="ExceptionResult"/> (500 Internal Server Error) with the specified exception.
        /// </summary>
        /// <param name="exception">The exception to include in the error.</param>
        /// <returns>An <see cref="ExceptionResult"/> with the specified exception.</returns>
        protected internal virtual ExceptionResult InternalServerError(Exception exception)
        {
            return new ExceptionResult(exception, this);
        }

        /// <summary>Creates a <see cref="JsonResult{T}"/> (200 OK) with the specified value.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <returns>A <see cref="JsonResult{T}"/> with the specified value.</returns>
        protected internal JsonResult<T> Json<T>(T content)
        {
            return Json<T>(content, new JsonSerializerSettings());
        }

        /// <summary>Creates a <see cref="JsonResult{T}"/> (200 OK) with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <returns>A <see cref="JsonResult{T}"/> with the specified values.</returns>
        protected internal JsonResult<T> Json<T>(T content, JsonSerializerSettings serializerSettings)
        {
            return Json<T>(content, serializerSettings, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
        }

        /// <summary>Creates a <see cref="JsonResult{T}"/> (200 OK) with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <returns>A <see cref="JsonResult{T}"/> with the specified values.</returns>
        protected internal virtual JsonResult<T> Json<T>(T content, JsonSerializerSettings serializerSettings,
            Encoding encoding)
        {
            return new JsonResult<T>(content, serializerSettings, encoding, this);
        }

        /// <summary>Creates a <see cref="NotFoundResult"/> (404 Not Found).</summary>
        /// <returns>A <see cref="NotFoundResult"/>.</returns>
        protected internal virtual NotFoundResult NotFound()
        {
            return new NotFoundResult(this);
        }

        /// <summary>Creates an <see cref="OkResult"/> (200 OK).</summary>
        /// <returns>An <see cref="OkResult"/>.</returns>
        protected internal virtual OkResult Ok()
        {
            return new OkResult(this);
        }

        /// <summary>
        /// Creates an <see cref="OkNegotiatedContentResult{T}"/> (200 OK) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>An <see cref="OkNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual OkNegotiatedContentResult<T> Ok<T>(T content)
        {
            return new OkNegotiatedContentResult<T>(content, this);
        }

        /// <summary>Creates a <see cref="ResponseMessageResult"/> with the specified response.</summary>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>A <see cref="ResponseMessageResult"/> for the specified response.</returns>
        protected internal virtual ResponseMessageResult ResponseMessage(HttpResponseMessage response)
        {
            return new ResponseMessageResult(response);
        }

        /// <summary>Creates a <see cref="StatusCodeResult"/> with the specified status code.</summary>
        /// <param name="status">The HTTP status code for the response message</param>
        /// <returns>A <see cref="StatusCodeResult"/> with the specified status code.</returns>
        protected internal virtual StatusCodeResult StatusCode(HttpStatusCode status)
        {
            return new StatusCodeResult(status, this);
        }

        /// <summary>
        /// Creates an <see cref="UnauthorizedResult"/> (401 Unauthorized) with the specified values.
        /// </summary>
        /// <param name="challenges">The WWW-Authenticate challenges.</param>
        /// <returns>An <see cref="UnauthorizedResult"/> with the specified values.</returns>
        protected internal UnauthorizedResult Unauthorized(params AuthenticationHeaderValue[] challenges)
        {
            return Unauthorized((IEnumerable<AuthenticationHeaderValue>)challenges);
        }

        /// <summary>
        /// Creates an <see cref="UnauthorizedResult"/> (401 Unauthorized) with the specified values.
        /// </summary>
        /// <param name="challenges">The WWW-Authenticate challenges.</param>
        /// <returns>An <see cref="UnauthorizedResult"/> with the specified values.</returns>
        protected internal virtual UnauthorizedResult Unauthorized(IEnumerable<AuthenticationHeaderValue> challenges)
        {
            return new UnauthorizedResult(challenges, this);
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

        internal static async Task<HttpResponseMessage> InvokeActionWithExceptionFilters(IHttpActionResult innerResult,
            HttpActionContext actionContext, CancellationToken cancellationToken, IExceptionFilter[] filters)
        {
            Contract.Assert(innerResult != null);
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);

            Exception exception = null;
            try
            {
                return await innerResult.ExecuteAsync(cancellationToken);
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