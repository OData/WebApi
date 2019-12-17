
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
#else
using System;
using System.Collections.Generic;
using System.Web.Http.Filters;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// A record of an error.
    /// </summary>
    public struct WebHostErrorRecord
    {
        public string Controller;
        public string Method;
        public Exception Exception;
    }

    /// <summary>
    /// The WebHostTestFixture is create a web host to be used for a test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WebHostLogExceptionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostLogExceptionFilter"/> class.
        /// </summary>
        public WebHostLogExceptionFilter()
        {
            this.Exceptions = new List<WebHostErrorRecord>();
        }

#if NETCORE
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            ControllerActionDescriptor actionDescriptor = actionExecutedContext.ActionDescriptor as ControllerActionDescriptor;
            string controller = actionDescriptor?.ControllerName;
            string method = actionDescriptor?.ActionName;
#else
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            string controller = actionExecutedContext?.ActionContext?.ControllerContext?.ControllerDescriptor?.ControllerName;
            string method = actionExecutedContext?.ActionContext?.ActionDescriptor?.ActionName;
#endif
            // Log the exception to the console.
            if (actionExecutedContext.Exception != null)
            {
                WebHostErrorRecord record = new WebHostErrorRecord()
                {
                    Controller = controller,
                    Method = method,
                    Exception = actionExecutedContext.Exception,
                };

                this.Exceptions.Add(record);
            }
        }

        /// <summary>
        /// Gets a list of logged exceptions.
        /// </summary>
        public IList<WebHostErrorRecord> Exceptions { get; private set; }
    }
}
