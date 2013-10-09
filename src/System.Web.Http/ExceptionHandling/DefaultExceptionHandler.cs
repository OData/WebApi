// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using System.Web.Http.Results;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides the default implementation for handling exceptions within Web API.</summary>
    /// <remarks>
    /// This class preserves the legacy behavior of catch blocks and is the the default registered IExceptionHandler.
    /// This default service allows adding the IExceptionHandler service extensibility point without making any
    /// breaking changes in the default implementation.
    /// </remarks>
    internal class DefaultExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.FromResult<object>(null);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "We already shipped this code; avoiding even minor breaking changes in error handling.")]
        private static void Handle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;

            if (exceptionContext == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionHandlerContext).Name, "ExceptionContext"), "context");
            }

            Exception exception = exceptionContext.Exception;

            if (exception == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Exception"), "context");
            }

            HttpRequestMessage request = exceptionContext.Request;

            if (request == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Request"), "context");
            }

            if (exceptionContext.CatchBlock == ExceptionCatchBlocks.IExceptionFilter)
            {
                // The exception filter stage propagates unhandled exceptions by default (when no filter handles the
                // exception).
                return;
            }

            if (exceptionContext.CatchBlock == "HttpControllerHandler.WriteBufferedResponseContentAsync")
            {
                HandleWebHostBufferedContentException(context);
                return;
            }

            context.Result = new ResponseMessageResult(request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                exception));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "We already shipped this code; avoiding even minor breaking changes in error handling.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "All exceptions caught here become error responses")]
        private static void HandleWebHostBufferedContentException(ExceptionHandlerContext context)
        {
            Contract.Assert(context != null);

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            Exception exception = exceptionContext.Exception;
            Contract.Assert(exception != null);

            HttpRequestMessage request = exceptionContext.Request;
            Contract.Assert(request != null);

            HttpResponseMessage response = exceptionContext.Response;

            if (response == null)
            {
                // For HttpWebRoute.GetRouteData failures, preserve the default behavior of propogating exceptions.
                return;
            }

            HttpContent responseContent = response.Content;

            if (responseContent == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(HttpResponseMessage).Name, "Content"), "context");
            }

            HttpResponseMessage errorResponse;

            // Create a 500 response with content containing an explanatory message and
            // stack trace, subject to content negotiation and policy for error details.
            try
            {
                MediaTypeHeaderValue mediaType = responseContent.Headers.ContentType;
                string messageDetails = (mediaType != null)
                                            ? Error.Format(
                                                SRResources.Serialize_Response_Failed_MediaType,
                                                responseContent.GetType().Name,
                                                mediaType)
                                            : Error.Format(
                                                SRResources.Serialize_Response_Failed,
                                                responseContent.GetType().Name);

                errorResponse = request.CreateErrorResponse(
                                            HttpStatusCode.InternalServerError,
                                            new InvalidOperationException(messageDetails, exception));

                // CreateErrorResponse will choose 406 if it cannot find a formatter,
                // but we want our default error response to be 500 always
                errorResponse.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch
            {
                // Failed creating an HttpResponseMessage for the error response.
                // This can happen for missing config, missing conneg service, etc.
                // Create an empty error response and return a non-faulted task.
                errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            context.Result = new ResponseMessageResult(errorResponse);
        }
    }
}
