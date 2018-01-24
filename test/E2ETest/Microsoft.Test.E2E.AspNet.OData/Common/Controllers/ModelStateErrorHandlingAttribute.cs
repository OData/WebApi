// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class ModelStateErrorHandlingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
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
    }
}
