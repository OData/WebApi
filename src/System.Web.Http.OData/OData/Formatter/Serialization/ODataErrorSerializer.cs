// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal class ODataErrorSerializer : ODataSerializer
    {
        private const string MessageKey = "Message";
        private const string MessageLanguageKey = "MessageLanguage";
        private const string ErrorCodeKey = "ErrorCode";
        private const string ExceptionMessageKey = "ExceptionMessage";
        private const string ExceptionTypeKey = "ExceptionType";
        private const string StackTraceKey = "StackTrace";
        private const string InnerExceptionKey = "InnerException";

        public ODataErrorSerializer()
            : base(ODataPayloadKind.Error)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw Error.ArgumentNull("graph");
            }

            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            ODataError oDataError = graph as ODataError;
            if (oDataError == null)
            {
                HttpError httpError = graph as HttpError;
                if (httpError == null)
                {
                    throw Error.InvalidOperation(SRResources.ErrorTypeMustBeODataErrorOrHttpError, graph.GetType().Name);
                }
                else
                {
                    oDataError = ConvertToODataError(httpError);
                }
            }

            bool includeDebugInformation = oDataError.InnerError != null;
            messageWriter.WriteError(oDataError, includeDebugInformation);
        }

        internal static ODataError ConvertToODataError(HttpError httpError)
        {
            return new ODataError()
            {
                Message = GetPropertyValue<string>(httpError, MessageKey),
                MessageLanguage = GetPropertyValue<string>(httpError, MessageLanguageKey),
                ErrorCode = GetPropertyValue<string>(httpError, ErrorCodeKey),
                InnerError = ConvertToODataInnerError(httpError)
            };
        }

        private static ODataInnerError ConvertToODataInnerError(HttpError httpError)
        {
            string innerErrorMessage = GetPropertyValue<string>(httpError, ExceptionMessageKey);
            if (innerErrorMessage == null)
            {
                return null;
            }
            else
            {
                ODataInnerError innerError = new ODataInnerError();
                innerError.Message = innerErrorMessage;
                innerError.TypeName = GetPropertyValue<string>(httpError, ExceptionTypeKey);
                innerError.StackTrace = GetPropertyValue<string>(httpError, StackTraceKey);
                HttpError innerExceptionError = GetPropertyValue<HttpError>(httpError, InnerExceptionKey);
                if (innerExceptionError != null)
                {
                    innerError.InnerError = ConvertToODataInnerError(innerExceptionError);
                }
                return innerError;
            }
        }

        private static TValue GetPropertyValue<TValue>(HttpError httpError, string key)
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
