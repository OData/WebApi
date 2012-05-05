// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Defines a container for arbitrary error information.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This type is only a dictionary to get the right serialization format")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "DCS does not support IXmlSerializable types that are also marked as [Serializable]")]
    [XmlRoot("Error")]
    public sealed class HttpError : Dictionary<string, object>, IXmlSerializable
    {
        private const string MessageKey = "Message";
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
        public HttpError(Exception exception)
        {
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            Message = SRResources.ExceptionOccurred;

            Add(ExceptionMessageKey, exception.Message);
            Add(ExceptionTypeKey, exception.GetType().FullName);
            Add(StackTraceKey, exception.StackTrace);
            if (exception.InnerException != null)
            {
                Add(InnerExceptionKey, exception.InnerException);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="modelState"/>.
        /// </summary>
        /// <param name="modelState">The invalid model state to use for error information.</param>
        public HttpError(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw Error.ArgumentNull("modelState");
            }

            if (modelState.IsValid)
            {
                throw Error.Argument("modelState", SRResources.ValidModelState);
            }

            Message = SRResources.InvalidModelState;
            foreach (KeyValuePair<string, ModelState> keyModelStatePair in modelState)
            {
                string key = keyModelStatePair.Key;
                ModelErrorCollection errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    // Combine the error messages for the key to avoid duplicate keys being added to the dictionary
                    string commaSeparatedErrorString = String.Join(", ", errors.Select(error => error.Exception == null ? error.ErrorMessage : error.Exception.ToString()));
                    Add(key, commaSeparatedErrorString);
                }
            }
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

        /// <summary>
        /// Returns a value indicating whether or not this instance contains error information beyond the error message
        /// </summary>
        /// <returns><c>true</c> if this instance contains information that isn't in the error message, <c>false</c> otherwise</returns>
        public bool ContainsErrorDetail()
        {
            return Keys.Any(key => key != MessageKey);
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
                string key = reader.LocalName;
                reader.ReadStartElement();
                string value = reader.Value;
                reader.Read();
                reader.ReadEndElement();

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
                writer.WriteStartElement(key);
                if (value != null)
                {
                    writer.WriteValue(value);
                }
                writer.WriteEndElement();
            }
        }
    }
}
