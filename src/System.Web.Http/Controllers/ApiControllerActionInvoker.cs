// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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

            HttpActionDescriptor actionDescriptor = actionContext.ActionDescriptor;
            HttpControllerContext controllerContext = actionContext.ControllerContext;

            return TaskHelpers.RunSynchronously(() =>
            {
                return actionDescriptor.ExecuteAsync(controllerContext, actionContext.ActionArguments)
                    .Then(value => actionDescriptor.ResultConverter.Convert(controllerContext, value));
            }, cancellationToken)
            .Catch<HttpResponseMessage>(info =>
            {
                // Propagate anything which isn't HttpResponseException
                HttpResponseException httpResponseException = info.Exception as HttpResponseException;
                if (httpResponseException == null)
                {
                    return info.Throw();
                }

                HttpResponseMessage response = httpResponseException.Response;
                response.EnsureResponseHasRequest(actionContext.Request);

                return info.Handled(response);
            }, cancellationToken);
        }
    }
}
