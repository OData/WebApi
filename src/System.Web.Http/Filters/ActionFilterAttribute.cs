using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Common;
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

            Task<HttpResponseMessage> internalTask = continuation();
            bool calledOnActionExecuted = false;

            return internalTask
                .Then(response =>
                {
                    calledOnActionExecuted = true;
                    return CallOnActionExecuted(actionContext, response: response);
                }, cancellationToken)
                .Catch(ex =>
                {
                    // If we've already called OnActionExecuted, that means this catch is running because
                    // OnActionExecuted threw an exception, so we just want to re-throw the exception rather
                    // that calling OnActionExecuted again.
                    if (calledOnActionExecuted)
                    {
                        return TaskHelpers.FromError<HttpResponseMessage>(ex);
                    }

                    return CallOnActionExecuted(actionContext, exception: ex);
                }, cancellationToken);
        }

        private Task<HttpResponseMessage> CallOnActionExecuted(HttpActionContext actionContext, HttpResponseMessage response = null, Exception exception = null)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(response != null || exception != null);

            HttpActionExecutedContext executedContext = new HttpActionExecutedContext(actionContext, exception) { Result = response };

            OnActionExecuted(executedContext);

            if (executedContext.Result != null)
            {
                return TaskHelpers.FromResult(executedContext.Result);
            }
            if (executedContext.Exception != null)
            {
                return TaskHelpers.FromError<HttpResponseMessage>(executedContext.Exception);
            }

            throw Error.InvalidOperation(SRResources.ActionFilterAttribute_MustSupplyResponseOrException, GetType().Name);
        }
    }
}
