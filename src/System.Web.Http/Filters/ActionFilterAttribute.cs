// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class ActionFilterAttribute : FilterAttribute, IActionFilter
    {
        public virtual void OnActionExecuting(HttpActionContext actionContext)
        {
        }

        public virtual void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        Task<HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }
            if (continuation == null)
            {
                throw Error.ArgumentNull("continuation");
            }

            try
            {
                OnActionExecuting(actionContext);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<HttpResponseMessage>(e);
            }

            if (actionContext.Response != null)
            {
                return TaskHelpers.FromResult(actionContext.Response);
            }

            return CallOnActionExecutedAsync(actionContext, cancellationToken, continuation);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        private async Task<HttpResponseMessage> CallOnActionExecutedAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = null;
            Exception exception = null;
            try
            {
                response = await continuation();
            }
            catch (Exception e)
            {
                exception = e;
            }

            try
            {
                HttpActionExecutedContext executedContext = new HttpActionExecutedContext(actionContext, exception) { Response = response };
                OnActionExecuted(executedContext);

                if (executedContext.Response != null)
                {
                    return executedContext.Response;
                }
                if (executedContext.Exception != null)
                {
                    throw executedContext.Exception;
                }
            }
            catch
            {
                // Catch is running because OnActionExecuted threw an exception, so we just want to re-throw the exception.
                // We also need to reset the response to forget about it since a filter threw an exception.
                actionContext.Response = null;
                throw;
            }

            throw Error.InvalidOperation(SRResources.ActionFilterAttribute_MustSupplyResponseOrException, GetType().Name);
        }
    }
}
