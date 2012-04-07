// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace System.Web.Http.Dispatcher
{
    // Infrastructure class used to serialize unhandled exceptions at the dispatcher layer. Not meant to be used by end users.
    // Only public to enable its serialization by XmlSerializer and in partial trust.
    [DataContract(Name = "Exception")]
    [XmlRoot("Exception")]
    public class ExceptionSurrogate
    {
        private ExceptionSurrogate()
        {
        }

        internal ExceptionSurrogate(Exception exception)
        {
            Contract.Assert(exception != null);

            Message = exception.Message;
            StackTrace = exception.StackTrace;
            if (exception.InnerException != null)
            {
                InnerException = new ExceptionSurrogate(exception.InnerException);
            }
            ExceptionType = exception.GetType().FullName;
        }

        [DataMember]
        public string ExceptionType { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Message { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string StackTrace { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public ExceptionSurrogate InnerException { get; set; }
    }
}
