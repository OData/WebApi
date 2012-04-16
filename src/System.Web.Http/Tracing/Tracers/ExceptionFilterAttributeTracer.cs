// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="ExceptionFilterAttribute"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "internal type needs to override, tracer are not sealed")]
    internal class ExceptionFilterAttributeTracer : ExceptionFilterAttribute
    {
        private const string OnExceptionMethodName = "OnException";

        private readonly ExceptionFilterAttribute _innerFilter;
        private readonly ITraceWriter _traceStore;

        public ExceptionFilterAttributeTracer(ExceptionFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            Contract.Assert(innerFilter != null);
            Contract.Assert(traceWriter != null);

            _innerFilter = innerFilter;
            _traceStore = traceWriter;
        }

        public override bool AllowMultiple
        {
            get
            {
                return _innerFilter.AllowMultiple;
            }
        }

        public override object TypeId
        {
            get
            {
                return _innerFilter.TypeId;
            }
        }

        public override bool Equals(object obj)
        {
            return _innerFilter.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _innerFilter.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return _innerFilter.IsDefaultAttribute();
        }

        public override bool Match(object obj)
        {
            return _innerFilter.Match(obj);
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
