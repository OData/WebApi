// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IAuthorizationFilter"/>.
    /// </summary>
    internal class AuthorizationFilterTracer : FilterTracer, IAuthorizationFilter
    {
        private const string ExecuteAuthorizationFilterAsyncMethodName = "ExecuteAuthorizationFilterAsync";

        public AuthorizationFilterTracer(IAuthorizationFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        private IAuthorizationFilter InnerAuthorizationFilter
        {
            get { return InnerFilter as IAuthorizationFilter; }
        }

        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext,
                                                                         CancellationToken cancellationToken,
                                                                         Func<Task<HttpResponseMessage>> continuation)
        {
            return TraceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                actionContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                InnerAuthorizationFilter.GetType().Name,
                ExecuteAuthorizationFilterAsyncMethodName,
                beginTrace: null,
                execute: () => InnerAuthorizationFilter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, continuation),
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
