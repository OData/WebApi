// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http
{
    public class ExceptionController : ApiController
    {
        public static string ResponseExceptionHeaderKey = "responseExceptionStatusCode";

        public HttpResponseMessage Unavailable()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        }

        public Task<HttpResponseMessage> AsyncUnavailable()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        }

        public Task<HttpResponseMessage> AsyncUnavailableDelegate()
        {
            return Task.Factory.StartNew<HttpResponseMessage>(() => { throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)); });
        }

        public HttpResponseMessage ArgumentNull()
        {
            throw new ArgumentNullException("foo");
        }

        public Task<HttpResponseMessage> AsyncArgumentNull()
        {
            return Task.Factory.StartNew<HttpResponseMessage>(() => { throw new ArgumentNullException("foo"); });
        }

        [HttpGet]
        public string GetException()
        {
            return "foo";
        }

        [HttpGet]
        public string GetString()
        {
            return "bar";
        }

        public T GenericAction<T>() where T : User
        {
            return null;
        }

        [AuthorizationFilterThrows]
        public void AuthorizationFilter() { }

        [ActionFilterThrows]
        public void ActionFilter() { }

        [ExceptionFilterThrows]
        public void ExceptionFilter() { throw new ArgumentException("exception"); }

        private class AuthorizationFilterThrows : AuthorizeAttribute
        {
            public override void OnAuthorization(HttpActionContext actionContext)
            {
                TryThrowHttpResponseException(actionContext);
                throw new ArgumentException("authorization");
            }
        }

        private class ActionFilterThrows : ActionFilterAttribute
        {
            public override void OnActionExecuting(HttpActionContext actionContext)
            {
                TryThrowHttpResponseException(actionContext);
                throw new ArgumentException("action");
            }
        }

        private class ExceptionFilterThrows : ExceptionFilterAttribute
        {
            public override void OnException(HttpActionExecutedContext actionExecutedContext)
            {
                TryThrowHttpResponseException(actionExecutedContext.ActionContext);
                throw actionExecutedContext.Exception;
            }
        }

        private static void TryThrowHttpResponseException(HttpActionContext actionContext)
        {
            IEnumerable<string> values;
            if (actionContext.ControllerContext.Request.Headers.TryGetValues(ResponseExceptionHeaderKey, out values))
            {
                string statusString = values.First() as string;
                if (!String.IsNullOrEmpty(statusString))
                {
                    HttpStatusCode status = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusString);
                    throw new HttpResponseException(actionContext.Request.CreateResponse(status, "HttpResponseExceptionMessage"));
                }
            }
        }
    }
}
