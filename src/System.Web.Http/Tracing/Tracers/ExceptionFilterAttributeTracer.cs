// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="ExceptionFilterAttribute"/>.
    /// </summary>
    internal sealed class ExceptionFilterAttributeTracer : ExceptionFilterAttribute
    {
        private const string OnExceptionMethodName = "OnException";

        private readonly ExceptionFilterAttribute _innerFilter;
        private readonly ITraceWriter _traceStore;

        public ExceptionFilterAttributeTracer(ExceptionFilterAttribute innerFilter, ITraceWriter traceStore)
        {
            _innerFilter = innerFilter;
            _traceStore = traceStore;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            _traceStore.TraceBeginEnd(
                actionExecutedContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                _innerFilter.GetType().Name,
                OnExceptionMethodName,
                beginTrace: (tr) =>
                {
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                execute: () =>
                {
                    _innerFilter.OnException(actionExecutedContext);
                },
                endTrace: (tr) =>
                {
                    Exception returnedException = actionExecutedContext.Exception;
                    tr.Level = returnedException == null ? TraceLevel.Info : TraceLevel.Error;
                    tr.Exception = returnedException;
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: (tr) =>
                {
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                });
        }
    }
}
