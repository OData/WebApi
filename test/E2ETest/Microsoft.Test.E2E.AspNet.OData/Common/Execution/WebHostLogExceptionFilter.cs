
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Http.Filters;

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
    public class WebHostLogExceptionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostLogExceptionFilter"/> class.
        /// </summary>
        public WebHostLogExceptionFilter()
        {
            this.Exceptions = new List<WebHostErrorRecord>();
        }
        
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // Log the exception to the console.
            if (actionExecutedContext.Exception != null)
            {
                string controller = actionExecutedContext?.ActionContext?.ControllerContext?.ControllerDescriptor?.ControllerName;
                string method = actionExecutedContext?.ActionContext?.ActionDescriptor?.ActionName;
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
