// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
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
                    throw Error.ArgumentNull("value");
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
                    throw Error.ArgumentNull("value");
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
                    throw Error.ArgumentNull("value");
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
        /// Returns an instance of a UrlHelper, which is used to generate URLs to other APIs.
        /// </summary>
        public UrlHelper Url
        {
            get { return ControllerContext.Url; }
        }

        /// <summary>
        /// Returns the current principal associated with this request.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "That would make for poor usability.")]
        public IPrincipal User
        {
            get { return Thread.CurrentPrincipal; }
        }

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
            HttpActionDescriptor actionDescriptor = controllerDescriptor.HttpActionSelector.SelectAction(controllerContext);
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);

            IEnumerable<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            FilterGrouping filterGrouping = new FilterGrouping(filters);

            IEnumerable<IActionFilter> actionFilters = filterGrouping.ActionFilters;
            IEnumerable<IAuthorizationFilter> authorizationFilters = filterGrouping.AuthorizationFilters;
            IEnumerable<IExceptionFilter> exceptionFilters = filterGrouping.ExceptionFilters;

            // Func<Task<HttpResponseMessage>>
            Task<HttpResponseMessage> result = InvokeActionWithAuthorizationFilters(actionContext, cancellationToken, authorizationFilters, () =>
            {
                HttpActionBinding actionBinding = actionDescriptor.ActionBinding;
                Task bindTask = actionBinding.ExecuteBindingAsync(actionContext, cancellationToken);
                return bindTask.Then<HttpResponseMessage>(() =>
                {
                    _modelState = actionContext.ModelState;
                    Func<Task<HttpResponseMessage>> invokeFunc = InvokeActionWithActionFilters(actionContext, cancellationToken, actionFilters, () =>
                    {
                        return controllerDescriptor.HttpActionInvoker.InvokeActionAsync(actionContext, cancellationToken);
                    });
                    return invokeFunc();
                });
            })();

            result = InvokeActionWithExceptionFilters(result, actionContext, cancellationToken, exceptionFilters);

            return result;
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
        }

        internal static Task<HttpResponseMessage> InvokeActionWithExceptionFilters(Task<HttpResponseMessage> actionTask, HttpActionContext actionContext, CancellationToken cancellationToken, IEnumerable<IExceptionFilter> filters)
        {
            Contract.Assert(actionTask != null);
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);

            return actionTask.Catch<HttpResponseMessage>(
                info =>
                {
                    HttpActionExecutedContext executedContext = new HttpActionExecutedContext(actionContext, info.Exception);

                    // Note: exception filters need to be scheduled in the reverse order so that
                    // the more specific filter (e.g. Action) executes before the less specific ones (e.g. Global)
                    filters = filters.Reverse();

                    // Note: in order to work correctly with the TaskHelpers.Iterate method, the lazyTaskEnumeration
                    // must be lazily evaluated. Otherwise all the tasks might start executing even though we want to run them
                    // sequentially and not invoke any of the following ones if an earlier fails.
                    IEnumerable<Task> lazyTaskEnumeration = filters.Select(filter => filter.ExecuteExceptionFilterAsync(executedContext, cancellationToken));
                    Task<HttpResponseMessage> resultTask =
                        TaskHelpers.Iterate(lazyTaskEnumeration, cancellationToken)
                                   .Then<HttpResponseMessage>(() =>
                                   {
                                       if (executedContext.Response != null)
                                       {
                                           return TaskHelpers.FromResult<HttpResponseMessage>(executedContext.Response);
                                       }
                                       else
                                       {
                                           return TaskHelpers.FromError<HttpResponseMessage>(executedContext.Exception);
                                       }
                                   });

                    return info.Task(resultTask);
                });
        }

        internal static Func<Task<HttpResponseMessage>> InvokeActionWithAuthorizationFilters(HttpActionContext actionContext, CancellationToken cancellationToken, IEnumerable<IAuthorizationFilter> filters, Func<Task<HttpResponseMessage>> innerAction)
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

        #endregion

        /// <summary>
        /// Quickly split filters into different types 
        /// </summary>
        /// <remarks>Avoid <see cref="M:ReadOnlyCollection.Select"/> because it has a very slow implementation that shows on profiles.</remarks>
        private class FilterGrouping
        {
            private List<IActionFilter> _actionFilters = new List<IActionFilter>();
            private List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
            private List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();

            public FilterGrouping(IEnumerable<FilterInfo> filters)
            {
                Contract.Assert(filters != null);

                foreach (FilterInfo f in filters)
                {
                    var filter = f.Instance;
                    Categorize(filter, _actionFilters);
                    Categorize(filter, _authorizationFilters);
                    Categorize(filter, _exceptionFilters);
                }
            }

            public IEnumerable<IActionFilter> ActionFilters
            {
                get { return _actionFilters; }
            }

            public IEnumerable<IAuthorizationFilter> AuthorizationFilters
            {
                get { return _authorizationFilters; }
            }

            public IEnumerable<IExceptionFilter> ExceptionFilters
            {
                get { return _exceptionFilters; }
            }

            private static void Categorize<T>(IFilter filter, List<T> list) where T : class
            {
                T match = filter as T;
                if (match != null)
                {
                    list.Add(match);
                }
            }
        }
    }
}
