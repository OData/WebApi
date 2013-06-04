// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

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

        private static Task<HttpResponseMessage> InvokeActionAsyncCore(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpActionDescriptor actionDescriptor = actionContext.ActionDescriptor;
            Contract.Assert(actionDescriptor != null);

            if (typeof(IHttpActionResult).IsAssignableFrom(actionDescriptor.ReturnType))
            {
                return InvokeUsingActionResultAsync(actionContext, actionDescriptor, cancellationToken);
            }
            else
            {
                return InvokeUsingResultConverterAsync(actionContext, actionDescriptor, cancellationToken);
            }
        }

        private static async Task<HttpResponseMessage> InvokeUsingActionResultAsync(HttpActionContext actionContext,
            HttpActionDescriptor actionDescriptor, CancellationToken cancellationToken)
        {
            Contract.Assert(actionContext != null);

            HttpControllerContext controllerContext = actionContext.ControllerContext;

            object result = await actionDescriptor.ExecuteAsync(controllerContext, actionContext.ActionArguments,
                cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException(SRResources.ApiControllerActionInvoker_NullHttpActionResult);
            }

            IHttpActionResult actionResult = result as IHttpActionResult;

            if (actionResult == null)
            {
                throw new InvalidOperationException(Error.Format(
                    SRResources.ApiControllerActionInvoker_InvalidHttpActionResult, result.GetType().Name));
            }

            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            if (response == null)
            {
                throw new InvalidOperationException(
                    SRResources.ResponseMessageResultConverter_NullHttpResponseMessage);
            }

            response.EnsureResponseHasRequest(actionContext.Request);

            return response;
        }

        private static async Task<HttpResponseMessage> InvokeUsingResultConverterAsync(HttpActionContext actionContext,
            HttpActionDescriptor actionDescriptor, CancellationToken cancellationToken)
        {
            Contract.Assert(actionContext != null);

            HttpControllerContext controllerContext = actionContext.ControllerContext;

            try
            {
                object actionResult = await actionDescriptor.ExecuteAsync(controllerContext,
                    actionContext.ActionArguments, cancellationToken);
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
