// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents an exception and the contextual data associated with it when it was caught.</summary>
    public class ExceptionContext
    {
        /// <summary>Initializes a new instance of the <see cref="ExceptionContext"/> class.</summary>
        public ExceptionContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="actionContext">The action context in which the exception occurred.</param>
        /// <param name="catchBlock">The label for the catch block where the exception was caught.</param>
        /// <param name="isTopLevelCatchBlock">
        /// A value indicating whether the catch block where the exception was caught is the last one before the host.
        /// </param>
        public ExceptionContext(Exception exception, HttpActionContext actionContext, string catchBlock,
            bool isTopLevelCatchBlock)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;

            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            ActionContext = actionContext;

            HttpControllerContext controllerContext = actionContext.ControllerContext;

            if (controllerContext == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(HttpActionContext).Name, "ControllerContext"), "actionContext");
            }

            ControllerContext = controllerContext;

            HttpRequestContext requestContext = controllerContext.RequestContext;
            Contract.Assert(requestContext != null);
            RequestContext = requestContext;

            HttpRequestMessage request = controllerContext.Request;

            if (request == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(HttpControllerContext).Name, "Request"), "actionContext");
            }

            Request = request;

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            CatchBlock = catchBlock;

            IsTopLevelCatchBlock = isTopLevelCatchBlock;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="request">The request being processed when the exception was caught.</param>
        /// <param name="catchBlock">The label for the catch block where the exception was caught.</param>
        /// <param name="isTopLevelCatchBlock">
        /// A value indicating whether the catch block where the exception was caught is the last one before the host.
        /// </param>
        public ExceptionContext(Exception exception, HttpRequestMessage request, string catchBlock,
            bool isTopLevelCatchBlock)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Request = request;
            RequestContext = request.GetRequestContext();

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            CatchBlock = catchBlock;

            IsTopLevelCatchBlock = isTopLevelCatchBlock;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="request">The request being processed when the exception was caught.</param>
        /// <param name="response">The repsonse being returned when the exception was caught.</param>
        /// <param name="catchBlock">The label for the catch block where the exception was caught.</param>
        /// <param name="isTopLevelCatchBlock">
        /// A value indicating whether the catch block where the exception was caught is the last one before the host.
        /// </param>
        public ExceptionContext(Exception exception, HttpRequestMessage request, HttpResponseMessage response,
            string catchBlock, bool isTopLevelCatchBlock)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Request = request;
            RequestContext = request.GetRequestContext();

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            Response = response;

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            CatchBlock = catchBlock;

            IsTopLevelCatchBlock = isTopLevelCatchBlock;
        }

        /// <summary>The exception caught.</summary>
        public Exception Exception { get; set; }

        /// <summary>The label for the catch block in which the exception was caught.</summary>
        public string CatchBlock { get; set; }

        /// <summary>
        /// A value indicating whether the catch block where the exception was caught is the last one before the host.
        /// </summary>
        public bool IsTopLevelCatchBlock { get; set; }

        /// <summary>The request being processed when the exception was caught.</summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>The request context in which the exception occurred.</summary>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>The controller context in which the exception occurred, if available.</summary>
        /// <remarks>This property will be <see langword="null"/> in most cases.</remarks>
        public HttpControllerContext ControllerContext { get; set; }

        /// <summary>The action context in which the exception occurred, if available.</summary>
        /// <remarks>This property will be <see langword="null"/> in most cases.</remarks>
        public HttpActionContext ActionContext { get; set; }

        /// <summary>The response being sent when the exception was caught.</summary>
        /// <remarks>This property will be <see langword="null"/> in most cases.</remarks>
        public HttpResponseMessage Response { get; set; }
    }
}
