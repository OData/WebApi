// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace System.Web.Http.Controllers
{
    internal class ExceptionFilterResult : IHttpActionResult
    {
        private readonly HttpActionContext _context;
        private readonly IExceptionFilter[] _filters;
        private readonly IHttpActionResult _innerResult;

        public ExceptionFilterResult(HttpActionContext context, IExceptionFilter[] filters,
            IHttpActionResult innerResult)
        {
            Contract.Assert(context != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerResult != null);

            _context = context;
            _filters = filters;
            _innerResult = innerResult;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo exceptionInfo = null;

            try
            {
                return await _innerResult.ExecuteAsync(cancellationToken);
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            // This code path only runs if the task is faulted with an exception
            Contract.Assert(exceptionInfo != null);
            Contract.Assert(exceptionInfo.SourceException != null);

            HttpActionExecutedContext executedContext = new HttpActionExecutedContext(_context,
                exceptionInfo.SourceException);

            // Note: exception filters need to be scheduled in the reverse order so that
            // the more specific filter (e.g. Action) executes before the less specific ones (e.g. Global)
            for (int i = _filters.Length - 1; i >= 0; i--)
            {
                IExceptionFilter exceptionFilter = _filters[i];
                await exceptionFilter.ExecuteExceptionFilterAsync(executedContext, cancellationToken);
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }
            else
            {
                // Preserve the original stack trace when the exception is not changed by any filter.
                if (exceptionInfo.SourceException == executedContext.Exception)
                {
                    exceptionInfo.Throw();
                }

                // If the exception is changed by a filter, throw the new exception instead.
                throw executedContext.Exception;
            }
        }
    }
}
