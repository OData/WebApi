// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Controllers
{
    public class ApiControllerActionInvoker : IHttpActionInvoker
    {
        public virtual Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return InvokeActionAsyncCore(actionContext, cancellationToken);
        }

        private async Task<HttpResponseMessage> InvokeActionAsyncCore(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpActionDescriptor actionDescriptor = actionContext.ActionDescriptor;
            HttpControllerContext controllerContext = actionContext.ControllerContext;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                object actionResult = await actionDescriptor.ExecuteAsync(controllerContext, actionContext.ActionArguments, cancellationToken);
                return actionDescriptor.ResultConverter.Convert(controllerContext, actionResult);
            }
            catch (HttpResponseException httpResponseException)
            {
                HttpResponseMessage response = httpResponseException.Response;
                response.EnsureResponseHasRequest(actionContext.Request);

                return response;
            }
        }
    }
}
