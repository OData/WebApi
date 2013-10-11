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
        /// <remarks>This constructor is for unit testing purposes only.</remarks>
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

        /// <summary>Gets the exception caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public Exception Exception { get; set; }

        /// <summary>Gets the label for the catch block in which the exception was caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public string CatchBlock { get; set; }

        /// <summary>
        /// Gets a value indicating whether the catch block where the exception was caught is the last one before the
        /// host.
        /// </summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public bool IsTopLevelCatchBlock { get; set; }

        /// <summary>Gets the request being processed when the exception was caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public HttpRequestMessage Request { get; set; }

        /// <summary>Gets the request context in which the exception occurred.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>Gets the controller context in which the exception occurred, if available.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HttpControllerContext ControllerContext { get; set; }

        /// <summary>Gets the action context in which the exception occurred, if available.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HttpActionContext ActionContext { get; set; }

        /// <summary>Gets the response being sent when the exception was caught.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HttpResponseMessage Response { get; set; }
    }
}
