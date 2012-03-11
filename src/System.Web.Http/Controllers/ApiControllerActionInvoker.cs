using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Common;

namespace System.Web.Http.Controllers
{
    public class ApiControllerActionInvoker : IHttpActionInvoker
    {
        private static ConcurrentDictionary<Type, ActionResponseConverter> _actionResponseConverterCache = new ConcurrentDictionary<Type, ActionResponseConverter>();

        public virtual Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            Contract.Assert(actionContext.ActionDescriptor != null);
            HttpActionDescriptor actionDescriptor = actionContext.ActionDescriptor;
            HttpControllerContext controllerContext = actionContext.ControllerContext;
            Task<HttpResponseMessage> invocationTask = TaskHelpers.RunSynchronously(
                () =>
                {
                    // Action always returns synchronously.
                    // 1. Either it runs synchronously and 
                    //   a. returns an immediate result The ActionResponseConverter will then wrap that in a task 
                    //   b. or throws an exception, which this helper may catch and wrap as a task. 
                    // 2. Or if it needs to do IO, it created a task and returns the task. We can then return that task. 
                    object result = actionDescriptor.Execute(controllerContext, actionContext.ActionArguments);

                    // The static signature for the action may return object. So check the runtime result type.
                    // Serializers key off the return type. Sometimes They may only understand a base class and not a derived class. 
                    // So if the action specifies something more specific than object, use the precise return type.                     
                    Type returnType = actionDescriptor.ReturnType;
                    if ((returnType == typeof(object)) && (result != null))
                    {
                        returnType = result.GetType();
                    }

                    ActionResponseConverter responseConverter = _actionResponseConverterCache.GetOrAdd(returnType, ActionResponseConverter.GetResponseMessageConverter);
                    return responseConverter.Convert(controllerContext, result, cancellationToken);
                },
                cancellationToken);

            // Error handling for HttpResponseException
            return invocationTask.Catch<HttpResponseMessage>(
                (exception) =>
                {
                    HttpResponseException httpResponseException = exception as HttpResponseException;

                    if (httpResponseException != null)
                    {
                        HttpResponseMessage response = httpResponseException.Response;
                        if (response.RequestMessage == null)
                        {
                            response.RequestMessage = actionContext.ControllerContext.Request;
                        }

                        return TaskHelpers.FromResult<HttpResponseMessage>(response);
                    }

                    // Propagate all other exceptions
                    return TaskHelpers.FromError<HttpResponseMessage>(exception);
                },
                cancellationToken);
        }
    }
}
