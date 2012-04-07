// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IActionFilter"/>.
    /// </summary>
    internal class ActionFilterTracer : FilterTracer, IActionFilter
    {
        private const string ExecuteActionFilterAsyncMethodName = "ExecuteActionFilterAsync";

        public ActionFilterTracer(IActionFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        private IActionFilter InnerActionFilter
        {
            get { return InnerFilter as IActionFilter; }
        }

        Task<HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(HttpActionContext actionContext,
                                                                         CancellationToken cancellationToken,
                                                                         Func<Task<HttpResponseMessage>> continuation)
        {
            return TraceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                actionContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                InnerActionFilter.GetType().Name,
                ExecuteActionFilterAsyncMethodName,
                beginTrace: null,
                execute: () => InnerActionFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, continuation),
                endTrace: (tr, response) =>
                {
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: null);
        }
    }
}
