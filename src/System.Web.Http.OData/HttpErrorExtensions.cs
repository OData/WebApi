// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.Data.OData;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpError"/> class.
    /// </summary>
    public static class HttpErrorExtensions
    {
        private const string MessageKey = "Message";
        private const string MessageLanguageKey = "MessageLanguage";
        private const string ErrorCodeKey = "ErrorCode";
        private const string ExceptionMessageKey = "ExceptionMessage";
        private const string ExceptionTypeKey = "ExceptionType";
        private const string StackTraceKey = "StackTrace";
        private const string InnerExceptionKey = "InnerException";

        /// <summary>
        /// Converts the <paramref name="httpError"/> to an <see cref="ODataError"/>.
        /// </summary>
        /// <param name="httpError">The <see cref="HttpError"/> instance to convert.</param>
        /// <returns>The converted <see cref="ODataError"/></returns>
        public static ODataError ToODataError(this HttpError httpError)
        {
            if (httpError == null)
            {
                throw Error.ArgumentNull("httpError");
            }

            return new ODataError()
            {
                Message = httpError.GetPropertyValue<string>(MessageKey),
                MessageLanguage = httpError.GetPropertyValue<string>(MessageLanguageKey),
                ErrorCode = httpError.GetPropertyValue<string>(ErrorCodeKey),
                InnerError = httpError.ToODataInnerError()
            };
        }

        private static ODataInnerError ToODataInnerError(this HttpError httpError)
        {
            string innerErrorMessage = httpError.GetPropertyValue<string>(ExceptionMessageKey);
            if (innerErrorMessage == null)
            {
                return null;
            }
            else
            {
                ODataInnerError innerError = new ODataInnerError();
                innerError.Message = innerErrorMessage;
                innerError.TypeName = httpError.GetPropertyValue<string>(ExceptionTypeKey);
                innerError.StackTrace = httpError.GetPropertyValue<string>(StackTraceKey);
                HttpError innerExceptionError = httpError.GetPropertyValue<HttpError>(InnerExceptionKey);
                if (innerExceptionError != null)
                {
                    innerError.InnerError = innerExceptionError.ToODataInnerError();
                }
                return innerError;
            }
        }

        private static TValue GetPropertyValue<TValue>(this HttpError httpError, string key)
        {
            Contract.Assert(httpError != null);

            object value;
            if (httpError.TryGetValue(key, out value))
            {
                if (value is TValue)
                {
                    return (TValue)value;
                }
            }
            return default(TValue);
        }
    }
}
