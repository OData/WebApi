// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
#else
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelStateErrorHandlingAttribute : ActionFilterAttribute
    {
#if NETCORE
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                HttpResponse response = context.HttpContext.Response;
                response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            }
            base.OnActionExecuting(context);
        }
#else
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                throw new HttpResponseException(
                    actionContext.Request.CreateErrorResponse(
                        System.Net.HttpStatusCode.BadRequest,
                        actionContext.ModelState));
            }
            base.OnActionExecuting(actionContext);
        }
#endif
    }
}
