// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IExceptionFilter"/>.
    /// </summary>
    internal class ExceptionFilterTracer : FilterTracer, IExceptionFilter
    {
        private const string ExecuteExceptionFilterAsyncMethodName = "ExecuteExceptionFilterAsync";

        public ExceptionFilterTracer(IExceptionFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        public IExceptionFilter InnerExceptionFilter
        {
            get { return InnerFilter as IExceptionFilter; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "This layer needs to observe all completion paths")]
        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext,
                                                CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                actionExecutedContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                InnerExceptionFilter.GetType().Name,
                ExecuteExceptionFilterAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Exception = actionExecutedContext.Exception;
                },

                execute: () => InnerExceptionFilter.ExecuteExceptionFilterAsync(actionExecutedContext, cancellationToken),

                endTrace: (tr) =>
                {
                    tr.Exception = actionExecutedContext.Exception;
                },

                errorTrace: null);
        }
    }
}
