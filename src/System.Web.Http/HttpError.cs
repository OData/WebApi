// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Http
{
    /// <summary>
    /// Defines a serializable container for arbitrary error information.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This type is only a dictionary to get the right serialization format")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "DCS does not support IXmlSerializable types that are also marked as [Serializable]")]
    [XmlRoot("Error")]
    public sealed class HttpError : Dictionary<string, object>, IXmlSerializable
    {
        private const string MessageKey = "Message";
        private const string MessageDetailKey = "MessageDetail";
        private const string ModelStateKey = "ModelState";
        private const string ExceptionMessageKey = "ExceptionMessage";
        private const string ExceptionTypeKey = "ExceptionType";
        private const string StackTraceKey = "StackTrace";
        private const string InnerExceptionKey = "InnerException";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class.
        /// </summary>
        public HttpError()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class containing error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The error message to associate with this instance.</param>
        public HttpError(string message)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception to use for error information.</param>
        /// <param name="includeErrorDetail"><c>true</c> to include the exception information in the error; <c>false</c> otherwise</param>
        public HttpError(Exception exception, bool includeErrorDetail)
        {
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            Message = SRResources.ErrorOccurred;

            if (includeErrorDetail)
            {
                Add(ExceptionMessageKey, exception.Message);
                Add(ExceptionTypeKey, exception.GetType().FullName);
                Add(StackTraceKey, exception.StackTrace);
                if (exception.InnerException != null)
                {
                    Add(InnerExceptionKey, new HttpError(exception.InnerException, includeErrorDetail));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="modelState"/>.
        /// </summary>
        /// <param name="modelState">The invalid model state to use for error information.</param>
        /// <param name="includeErrorDetail"><c>true</c> to include exception messages in the error; <c>false</c> otherwise</param>
        public HttpError(ModelStateDictionary modelState, bool includeErrorDetail)
        {
            if (modelState == null)
            {
                throw Error.ArgumentNull("modelState");
            }

            if (modelState.IsValid)
            {
                throw Error.Argument("modelState", SRResources.ValidModelState);
            }

            Message = SRResources.BadRequest;

            HttpError modelStateError = new HttpError();
            foreach (KeyValuePair<string, ModelState> keyModelStatePair in modelState)
            {
                string key = keyModelStatePair.Key;
                ModelErrorCollection errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    IEnumerable<string> errorMessages = errors.Select(error =>
                    {
                        if (includeErrorDetail && error.Exception != null)
                        {
                            return error.Exception.Message;
                        }
                        else
                        {
                            return String.IsNullOrEmpty(error.ErrorMessage) ? SRResources.ErrorOccurred : error.ErrorMessage;
                        }
                    }).ToArray();
                    modelStateError.Add(key, errorMessages);
                }
            }

            Add(ModelStateKey, modelStateError);                        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class containing error message <paramref name="message"/> and error message detail <paramref name="messageDetail"/>.
        /// </summary>
        /// <param name="message">The error message to associate with this instance.</param>
        /// <param name="messageDetail">The error message detail to associate with this instance.</param>
        internal HttpError(string message, string messageDetail)
            : this(message)
        {
            if (messageDetail == null)
            {
                throw Error.ArgumentNull("message");
            }

            Add(MessageDetailKey, messageDetail);
        }

        /// <summary>
        /// The error message associated with this instance.
        /// </summary>
        public string Message
        {
            get
            {
                if (ContainsKey(MessageKey))
                {
                    return this[MessageKey] as string;
                }
                else
                {
                    return null;
                }
            }

            set { this[MessageKey] = value; }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                string key = XmlConvert.DecodeName(reader.LocalName);
                string value = reader.ReadInnerXml();

                this.Add(key, value);
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<string, object> keyValuePair in this)
            {
                string key = keyValuePair.Key;
                object value = keyValuePair.Value;
                writer.WriteStartElement(XmlConvert.EncodeLocalName(key));
                if (value != null)
                {
                    HttpError innerError = value as HttpError;
                    if (innerError == null)
                    {
                        writer.WriteValue(value);
                    }
                    else
                    {
                        ((IXmlSerializable)innerError).WriteXml(writer);
                    }
                }
                writer.WriteEndElement();
            }
        }
    }
}