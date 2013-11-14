// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Extension methods for <see cref="TraceRecord"/>.
    /// </summary>
    internal static class TraceWriterExceptionMapper
    {
        private static string httpErrorExceptionMessageFormat = "ExceptionMessage{0}='{1}'";
        private static string httpErrorExceptionTypeFormat = "ExceptionType{0}='{1}'";
        private static string httpErrorMessageDetailFormat = "MessageDetail='{0}'";
        private static string httpErrorModelStateErrorFormat = "ModelStateError=[{0}]";
        private static string httpErrorModelStatePairFormat = "{0}=[{1}]";
        private static string httpErrorStackTraceFormat = "StackTrace{0}={1}";
        private static string httpErrorUserMessageFormat = "UserMessage='{0}'";

        /// <summary>
        /// Examines the given <see cref="TraceRecord"/> to determine whether it
        /// contains an <see cref="HttpResponseException"/> and if so, modifies
        /// the <see cref="TraceRecord"/> to capture more detailed information.
        /// </summary>
        /// <param name="traceRecord">The <see cref="TraceRecord"/> to examine and modify.</param>
        public static void TranslateHttpResponseException(TraceRecord traceRecord)
        {
            if (traceRecord == null)
            {
                throw Error.ArgumentNull("traceRecord");
            }

            HttpResponseException httpResponseException = traceRecord.Exception as HttpResponseException;
            if (httpResponseException == null)
            {
                return;
            }

            HttpResponseMessage response = httpResponseException.Response;
            Contract.Assert(response != null);

            // If the status has been set already, do not overwrite it,
            // otherwise propagate the status into the record.
            if (traceRecord.Status == 0)
            {
                traceRecord.Status = response.StatusCode;
            }

            // HttpResponseExceptions often contain HttpError instances that carry
            // detailed information that may be filtered out by IncludeErrorDetailPolicy
            // before reaching the client. Capture it here for the trace.
            ObjectContent objectContent = response.Content as ObjectContent;
            if (objectContent == null)
            {
                return;
            }

            HttpError httpError = objectContent.Value as HttpError;
            if (httpError == null)
            {
                return;
            }

            object messageObject = null;
            object messageDetailsObject = null;

            List<string> messages = new List<string>();

            if (httpError.TryGetValue(HttpErrorKeys.MessageKey, out messageObject))
            {
                messages.Add(Error.Format(httpErrorUserMessageFormat, messageObject));
            }

            if (httpError.TryGetValue(HttpErrorKeys.MessageDetailKey, out messageDetailsObject))
            {
                messages.Add(Error.Format(httpErrorMessageDetailFormat, messageDetailsObject));
            }

            // Extract the exception from this HttpError and then incrementally
            // walk down all inner exceptions.
            AddExceptions(httpError, messages);

            // ModelState errors are handled with a nested HttpError
            object modelStateErrorObject = null;
            if (httpError.TryGetValue(HttpErrorKeys.ModelStateKey, out modelStateErrorObject))
            {
                HttpError modelStateError = modelStateErrorObject as HttpError;
                if (modelStateError != null)
                {
                    messages.Add(FormatModelStateErrors(modelStateError));
                }
            }

            traceRecord.Message = String.Join(", ", messages);
        }

        /// <summary>
        /// Map the <see cref="TraceLevel"/> according to information from <see cref="HttpResponseException"/> if possible.
        /// </summary>
        /// <param name="httpResponseException">The <see cref="HttpResponseException"/> that determines the <see cref="TraceLevel"/>.</param>
        /// <returns>The mapped result of <see cref="TraceLevel"/>.</returns>
        public static TraceLevel? GetTraceLevel(HttpResponseException httpResponseException)
        {
            if (httpResponseException == null)
            {
                throw new ArgumentNullException("httpResponseException");
            }

            HttpResponseMessage response = httpResponseException.Response;
            Contract.Assert(response != null);

            TraceLevel? level = null;

            // Client level errors are downgraded to TraceLevel.Warn
            if ((int)response.StatusCode < (int)HttpStatusCode.InternalServerError)
            {
                level = TraceLevel.Warn;
            }

            // Non errors are downgraded to TraceLevel.Info
            if ((int)response.StatusCode < (int)HttpStatusCode.BadRequest)
            {
                level = TraceLevel.Info;
            }

            return level;
        }

        public static HttpResponseException ExtractHttpResponseException(TraceRecord traceRecord)
        {
            return ExtractHttpResponseException(traceRecord.Exception);
        }

        public static HttpResponseException ExtractHttpResponseException(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            var httpResponseException = exception as HttpResponseException;
            if (httpResponseException != null)
            {
                return httpResponseException;
            }

            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                httpResponseException = aggregateException
                    .Flatten()
                    .InnerExceptions
                    .Select(ExtractHttpResponseException)
                    .Where(ex => ex != null && ex.Response != null)
                    .OrderByDescending(ex => ex.Response.StatusCode)
                    .FirstOrDefault();
                return httpResponseException;
            }

            return ExtractHttpResponseException(exception.InnerException);
        }

        /// <summary>
        /// Unpacks any exceptions in the given <see cref="HttpError"/> and adds
        /// them into a collection of name-value pairs that can be composed into a single string.
        /// </summary>
        /// <remarks>
        /// This helper also iterates over all inner exceptions and unpacks them too.
        /// </remarks>
        /// <param name="httpError">The <see cref="HttpError"/> to unpack.</param>
        /// <param name="messages">A collection of messages to which the new information should be added.</param>
        private static void AddExceptions(HttpError httpError, List<string> messages)
        {
            Contract.Assert(httpError != null);
            Contract.Assert(messages != null);

            object exceptionMessageObject = null;
            object exceptionTypeObject = null;
            object stackTraceObject = null;
            object innerExceptionObject = null;

            for (int i = 0; httpError != null; i++)
            {
                // For uniqueness, key names append the depth of inner exception
                string indexText = i == 0 ? String.Empty : Error.Format("[{0}]", i);

                if (httpError.TryGetValue(HttpErrorKeys.ExceptionTypeKey, out exceptionTypeObject))
                {
                    messages.Add(Error.Format(httpErrorExceptionTypeFormat, indexText, exceptionTypeObject));
                }

                if (httpError.TryGetValue(HttpErrorKeys.ExceptionMessageKey, out exceptionMessageObject))
                {
                    messages.Add(Error.Format(httpErrorExceptionMessageFormat, indexText, exceptionMessageObject));
                }

                if (httpError.TryGetValue(HttpErrorKeys.StackTraceKey, out stackTraceObject))
                {
                    messages.Add(Error.Format(httpErrorStackTraceFormat, indexText, stackTraceObject));
                }

                if (!httpError.TryGetValue(HttpErrorKeys.InnerExceptionKey, out innerExceptionObject))
                {
                    break;
                }

                Contract.Assert(!Object.ReferenceEquals(httpError, innerExceptionObject));

                httpError = innerExceptionObject as HttpError;
            }
        }

        private static string FormatModelStateErrors(HttpError modelStateError)
        {
            Contract.Assert(modelStateError != null);

            List<string> messages = new List<string>();
            foreach (var pair in modelStateError)
            {
                IEnumerable<string> errorList = pair.Value as IEnumerable<string>;
                if (errorList != null)
                {
                    messages.Add(Error.Format(httpErrorModelStatePairFormat, pair.Key, String.Join(", ", errorList)));
                }
            }

            return Error.Format(httpErrorModelStateErrorFormat, String.Join(", ", messages));
        }
    }
}
