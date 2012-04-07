// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public virtual void OnException(HttpActionExecutedContext actionExecutedContext)
        {
        }

        Task IExceptionFilter.ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            OnException(actionExecutedContext);
            return TaskHelpers.Completed();
        }
    }
}
