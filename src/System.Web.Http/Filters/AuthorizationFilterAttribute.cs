// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class AuthorizationFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(HttpActionContext actionContext)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
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
                OnAuthorization(actionContext);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<HttpResponseMessage>(e);
            }

            if (actionContext.Response != null)
            {
                return TaskHelpers.FromResult(actionContext.Response);
            }
            else
            {
                return continuation();
            }
        }
    }
}
