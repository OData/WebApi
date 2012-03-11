using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using System.Web.Http.Query;

namespace System.Web.Http
{
    /// <summary>
    /// This result filter indicates that the results returned from an action should
    /// be limited to the specified ResultLimit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ResultLimitAttribute : ActionFilterAttribute
    {
        private int _resultLimit;

        public ResultLimitAttribute(int resultLimit)
        {
            _resultLimit = resultLimit;
        }

        public int ResultLimit
        {
            get { return _resultLimit; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            Contract.Assert(actionContext.ControllerContext.Request != null);

            if (_resultLimit <= 0)
            {
                Error.ArgumentOutOfRange("resultLimit", _resultLimit, SRResources.ResultLimitFilter_OutOfRange, actionContext.ActionDescriptor.ActionName);
            }

            HttpActionDescriptor action = actionContext.ActionDescriptor;
            if (!typeof(IEnumerable).IsAssignableFrom(action.ReturnType))
            {
                Error.InvalidOperation(SRResources.ResultLimitFilter_InvalidReturnType, action.ActionName);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            Contract.Assert(actionExecutedContext.Request != null);

            HttpResponseMessage response = actionExecutedContext.Result;
            IEnumerable results;
            if (response != null && response.TryGetObjectValue(out results))
            {
                // apply the result limit
                results = results.AsQueryable().Take(_resultLimit);
                response.TrySetObjectValue(results);
            }
        }
    }
}
