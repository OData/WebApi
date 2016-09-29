// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="SerializableError"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SerializableErrorExtensions
    {
        /// <summary>
        /// Converts the <paramref name="serializableError"/> to an <see cref="ODataError"/>.
        /// </summary>
        /// <param name="httpserializableErrorError">The <see cref="SerializableError"/> instance to convert.</param>
        /// <returns>The converted <see cref="ODataError"/></returns>
        public static ODataError CreateODataError(this SerializableError serializableError)
        {
            if (serializableError == null)
            {
                throw Error.ArgumentNull("serializableError");
            }

            return new ODataError
            {
                Message = serializableError.GetPropertyValue<string>(SerializableErrorKeys.MessageKey),
                ErrorCode = serializableError.GetPropertyValue<string>(SerializableErrorKeys.ErrorCodeKey),
                InnerError = ToODataInnerError(serializableError)
            };
        }

        private static ODataInnerError ToODataInnerError(SerializableError serializableError)
        {
            string innerErrorMessage = serializableError.GetPropertyValue<string>(SerializableErrorKeys.ExceptionMessageKey);
            if (innerErrorMessage == null)
            {
                string messageDetail = serializableError.GetPropertyValue<string>(SerializableErrorKeys.MessageDetailKey);
                if (messageDetail == null)
                {
                    SerializableError modelStateError = serializableError.GetPropertyValue<SerializableError>(SerializableErrorKeys.ModelStateKey);
                    return (modelStateError == null) ? null
                        : new ODataInnerError { Message = ConvertModelStateErrors(modelStateError) };
                }
                else
                {
                    return new ODataInnerError() { Message = messageDetail };
                }
            }
            else
            {
                ODataInnerError innerError = new ODataInnerError();
                innerError.Message = innerErrorMessage;
                innerError.TypeName = serializableError.GetPropertyValue<string>(SerializableErrorKeys.ExceptionTypeKey);
                innerError.StackTrace = serializableError.GetPropertyValue<string>(SerializableErrorKeys.StackTraceKey);
                SerializableError innerExceptionError = serializableError.GetPropertyValue<SerializableError>(SerializableErrorKeys.InnerExceptionKey);
                if (innerExceptionError != null)
                {
                    innerError.InnerError = ToODataInnerError(innerExceptionError);
                }
                return innerError;
            }
        }

        // Convert the model state errors in to a string (for debugging only).
        // This should be improved once ODataError allows more details.
        private static string ConvertModelStateErrors(SerializableError error)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object> modelStateError in error)
            {
                if (modelStateError.Value != null)
                {
                    builder.Append(modelStateError.Key);
                    builder.Append(" : ");

                    IEnumerable<string> errorMessages = modelStateError.Value as IEnumerable<string>;
                    if (errorMessages != null)
                    {
                        foreach (string errorMessage in errorMessages)
                        {
                            builder.AppendLine(errorMessage);
                        }
                    }
                    else
                    {
                        builder.AppendLine(modelStateError.Value.ToString());
                    }
                }
            }

            return builder.ToString();
        }

        private static TValue GetPropertyValue<TValue>(this SerializableError error, string errorKey)
        {
            object value;
            if (error.TryGetValue(errorKey, out value) && value is TValue)
            {
                return (TValue)value;
            }

            return default(TValue);
        }
    }

    internal static class SerializableErrorKeys
    {
        /// <summary>
        /// Provides a key for the Message.
        /// </summary>
        public static readonly string MessageKey = "Message";

        /// <summary>
        /// Provides a key for the MessageDetail.
        /// </summary>
        public static readonly string MessageDetailKey = "MessageDetail";

        /// <summary>
        /// Provides a key for the ModelState.
        /// </summary>
        public static readonly string ModelStateKey = "ModelState";

        /// <summary>
        /// Provides a key for the ExceptionMessage.
        /// </summary>
        public static readonly string ExceptionMessageKey = "ExceptionMessage";

        /// <summary>
        /// Provides a key for the ExceptionType.
        /// </summary>
        public static readonly string ExceptionTypeKey = "ExceptionType";

        /// <summary>
        /// Provides a key for the StackTrace.
        /// </summary>
        public static readonly string StackTraceKey = "StackTrace";

        /// <summary>
        /// Provides a key for the InnerException.
        /// </summary>
        public static readonly string InnerExceptionKey = "InnerException";

        /// <summary>
        /// Provides a key for the MessageLanguage.
        /// </summary>
        public static readonly string MessageLanguageKey = "MessageLanguage";

        /// <summary>
        /// Provides a key for the ErrorCode.
        /// </summary>
        public static readonly string ErrorCodeKey = "ErrorCode";
    }
}
